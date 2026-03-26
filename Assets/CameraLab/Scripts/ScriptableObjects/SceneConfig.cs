using CameraLab.Core.Models;
using UnityEngine;

namespace CameraLab.ScriptableObjects
{
    [CreateAssetMenu(menuName = "CameraLab/Scene Config", fileName = "SceneConfig")]
    public class SceneConfig : ScriptableObject
    {
        [Header("Identity")]
        public string sceneId;
        public string displayName;
        [TextArea]
        public string shortDescription;

        [Header("Content")]
        public GameObject environmentPrefab;
        public GameObject subjectPrefab;
        public SceneSpawnTransformData defaultCameraSpawnTransform;
        public SceneCameraDefaults defaultCameraSettings;

        [Header("Teaching Parameters")]
        public float ambientEV = 10f;
        public float sceneColorTemperatureK = 5500f;
        public float subjectDistanceMeters = 2f;
        public float backgroundDistanceMeters = 10f;
        public float subjectSpeedMetersPerSecond;
        public float shakeDifficulty = 1f;
        public ChallengeConfig challengeConfig;
    }
}
