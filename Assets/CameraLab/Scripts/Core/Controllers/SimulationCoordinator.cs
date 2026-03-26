using System;
using CameraLab.Core.Models;
using CameraLab.Core.Services;
using CameraLab.Rendering;
using UnityEngine;

namespace CameraLab.Core.Controllers
{
    public class SimulationCoordinator : MonoBehaviour
    {
        private CameraStateController cameraStateController;
        private SceneStateController sceneStateController;
        private ShotSimulationService shotSimulationService;
        private PostFXController postFXController;

        public event Action<SimulationResult> SimulationUpdated;

        public SimulationResult CurrentResult { get; private set; }

        public void Initialize(
            CameraStateController runtimeCameraStateController,
            SceneStateController runtimeSceneStateController,
            ShotSimulationService runtimeShotSimulationService,
            PostFXController runtimePostFXController)
        {
            cameraStateController = runtimeCameraStateController;
            sceneStateController = runtimeSceneStateController;
            shotSimulationService = runtimeShotSimulationService;
            postFXController = runtimePostFXController;

            cameraStateController.StateChanged += _ => Refresh();
            sceneStateController.SceneLoaded += (_, __) => Refresh();
        }

        public void Refresh()
        {
            if (cameraStateController == null || sceneStateController == null || sceneStateController.CurrentState == null)
            {
                return;
            }

            CurrentResult = shotSimulationService.Simulate(cameraStateController.CurrentState, sceneStateController.CurrentState);
            postFXController.Apply(cameraStateController.CurrentState, sceneStateController.CurrentState, CurrentResult);
            SimulationUpdated?.Invoke(CurrentResult.Clone());
        }
    }
}
