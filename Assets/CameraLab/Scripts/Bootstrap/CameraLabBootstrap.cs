using CameraLab.Content;
using CameraLab.Core.Coaching;
using CameraLab.Core.Controllers;
using CameraLab.Core.Evaluation;
using CameraLab.Core.Models;
using CameraLab.Core.Services;
using CameraLab.Rendering;
using CameraLab.Runtime;
using CameraLab.UI;
using UnityEngine;

namespace CameraLab.Bootstrap
{
    public class CameraLabBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrap()
        {
            if (FindAnyObjectByType<CameraLabBootstrap>() != null)
            {
                return;
            }

            var bootstrapObject = new GameObject("CameraLabBootstrap");
            bootstrapObject.AddComponent<CameraLabBootstrap>();
        }

        private void Awake()
        {
            BuildRuntime();
        }

        private void BuildRuntime()
        {
            var contentSet = BuiltinCameraLabContent.Create();

            var sceneRoot = new GameObject("RuntimeSceneRoot");
            sceneRoot.transform.SetParent(transform, false);

            var cameraRig = new GameObject("CameraRig");
            cameraRig.transform.SetParent(transform, false);

            var cameraStateController = gameObject.AddComponent<CameraStateController>();
            cameraStateController.Initialize(contentSet.cameraPreset, CameraState.FromDefaults(contentSet.scenes[0].defaultCameraSettings));

            var cameraViewController = cameraRig.AddComponent<CameraViewController>();
            cameraViewController.Initialize(cameraStateController, contentSet.cameraPreset);

            var sceneStateController = gameObject.AddComponent<SceneStateController>();
            sceneStateController.Initialize(contentSet.scenes, sceneRoot.transform, cameraRig.transform, cameraViewController.PreviewCamera);

            var postFXController = gameObject.AddComponent<PostFXController>();
            postFXController.Initialize(transform, cameraViewController.PreviewJitter);

            var simulationCoordinator = gameObject.AddComponent<SimulationCoordinator>();
            simulationCoordinator.Initialize(
                cameraStateController,
                sceneStateController,
                new ShotSimulationService(new SimulationTuning()),
                postFXController);

            var captureManager = gameObject.AddComponent<CaptureManager>();
            var uiController = gameObject.AddComponent<CameraLabUIController>();
            var appController = gameObject.AddComponent<CameraLabAppController>();

            appController.Initialize(
                contentSet,
                cameraStateController,
                sceneStateController,
                simulationCoordinator,
                captureManager,
                cameraViewController,
                new ShotEvaluationService(),
                new CoachingEngine(),
                uiController);
        }
    }
}
