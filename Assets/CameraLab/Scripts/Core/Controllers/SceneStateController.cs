using System;
using System.Collections.Generic;
using CameraLab.Content;
using CameraLab.Core.Models;
using CameraLab.ScriptableObjects;
using CameraLab.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

namespace CameraLab.Core.Controllers
{
    public class SceneStateController : MonoBehaviour
    {
        [SerializeField]
        private Transform sceneRoot;

        [SerializeField]
        private Transform cameraRigRoot;

        [SerializeField]
        private Camera previewCamera;

        private readonly List<SceneConfig> availableScenes = new List<SceneConfig>();
        private GameObject currentSceneRoot;
        private Light keyLight;
        private SceneConfig currentSceneConfig;
        private SceneState currentState;

        public event Action<SceneState, SceneConfig> SceneLoaded;

        public IReadOnlyList<SceneConfig> AvailableScenes => availableScenes;

        public SceneState CurrentState => currentState;

        public SceneConfig CurrentSceneConfig => currentSceneConfig;

        public void Initialize(IReadOnlyList<SceneConfig> scenes, Transform runtimeSceneRoot, Transform runtimeCameraRig, Camera runtimePreviewCamera)
        {
            availableScenes.Clear();
            availableScenes.AddRange(scenes);
            sceneRoot = runtimeSceneRoot;
            cameraRigRoot = runtimeCameraRig;
            previewCamera = runtimePreviewCamera;
        }

        public void LoadScene(int sceneIndex, bool challengeMode)
        {
            if (sceneIndex < 0 || sceneIndex >= availableScenes.Count)
            {
                return;
            }

            LoadScene(availableScenes[sceneIndex], challengeMode);
        }

        public void LoadScene(SceneConfig config, bool challengeMode)
        {
            currentSceneConfig = config;
            currentState = new SceneState
            {
                ambientEV = config.ambientEV,
                sceneColorTemperatureK = config.sceneColorTemperatureK,
                subjectDistanceMeters = config.subjectDistanceMeters,
                backgroundDistanceMeters = config.backgroundDistanceMeters,
                subjectSpeedMetersPerSecond = config.subjectSpeedMetersPerSecond,
                shakeDifficulty = config.shakeDifficulty,
                challengeConfig = challengeMode ? config.challengeConfig : null
            };

            DestroyCurrentScene();
            EnsureLighting();
            BuildRuntimeScene(config);
            ApplyCameraSpawn(config);
            ApplyEnvironmentLighting(currentState);

            SceneLoaded?.Invoke(currentState.Clone(), config);
        }

        public void ResetScene(bool challengeMode)
        {
            if (currentSceneConfig != null)
            {
                LoadScene(currentSceneConfig, challengeMode);
            }
        }

        private void BuildRuntimeScene(SceneConfig config)
        {
            if (config.environmentPrefab != null && config.subjectPrefab != null)
            {
                currentSceneRoot = new GameObject($"{config.displayName}_Scene");
                currentSceneRoot.transform.SetParent(sceneRoot, false);
                Instantiate(config.environmentPrefab, currentSceneRoot.transform);

                var subject = Instantiate(config.subjectPrefab, currentSceneRoot.transform);
                subject.transform.localPosition = new Vector3(0f, 0f, config.subjectDistanceMeters);
            }
            else
            {
                var build = PlaceholderSceneFactory.Build(config, sceneRoot);
                currentSceneRoot = build.root;
            }
        }

        private void ApplyCameraSpawn(SceneConfig config)
        {
            var pose = config.defaultCameraSpawnTransform.ToPose();
            cameraRigRoot.position = pose.position;
            cameraRigRoot.rotation = pose.rotation;
        }

        private void EnsureLighting()
        {
            if (keyLight != null)
            {
                return;
            }

            var lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (var i = 0; i < lights.Length; i++)
            {
                if (lights[i].type == LightType.Directional)
                {
                    keyLight = lights[i];
                    return;
                }
            }

            var lightObject = new GameObject("CameraLabKeyLight");
            lightObject.transform.SetParent(transform, false);
            lightObject.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
            keyLight = lightObject.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.shadows = LightShadows.Soft;
        }

        private void ApplyEnvironmentLighting(SceneState sceneState)
        {
            var lightColor = ColorTemperatureUtility.KelvinToColor(sceneState.sceneColorTemperatureK);
            var intensity = Mathf.Lerp(0.55f, 1.55f, Mathf.InverseLerp(4f, 14f, sceneState.ambientEV));
            keyLight.color = lightColor;
            keyLight.intensity = intensity;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = lightColor * Mathf.Lerp(0.2f, 0.65f, Mathf.InverseLerp(4f, 14f, sceneState.ambientEV));
            previewCamera.backgroundColor = Color.Lerp(new Color(0.04f, 0.05f, 0.07f), lightColor * 0.6f, 0.55f);
        }

        private void DestroyCurrentScene()
        {
            if (currentSceneRoot == null)
            {
                return;
            }

            Destroy(currentSceneRoot);
            currentSceneRoot = null;
        }
    }
}
