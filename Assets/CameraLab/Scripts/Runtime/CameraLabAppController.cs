using System.Collections;
using CameraLab.Content;
using CameraLab.Core.Coaching;
using CameraLab.Core.Controllers;
using CameraLab.Core.Evaluation;
using CameraLab.Core.Models;
using CameraLab.ScriptableObjects;
using CameraLab.UI;
using UnityEngine;

namespace CameraLab.Runtime
{
    public class CameraLabAppController : MonoBehaviour
    {
        private enum PlayMode
        {
            Sandbox,
            Challenge
        }

        private CameraLabContentSet contentSet;
        private CameraStateController cameraStateController;
        private SceneStateController sceneStateController;
        private SimulationCoordinator simulationCoordinator;
        private CaptureManager captureManager;
        private CameraViewController cameraViewController;
        private ShotEvaluationService shotEvaluationService;
        private CoachingEngine coachingEngine;
        private CameraLabUIController uiController;
        private PlayMode currentMode;
        private int currentSceneIndex;

        public void Initialize(
            CameraLabContentSet runtimeContentSet,
            CameraStateController runtimeCameraStateController,
            SceneStateController runtimeSceneStateController,
            SimulationCoordinator runtimeSimulationCoordinator,
            CaptureManager runtimeCaptureManager,
            CameraViewController runtimeCameraViewController,
            ShotEvaluationService runtimeShotEvaluationService,
            CoachingEngine runtimeCoachingEngine,
            CameraLabUIController runtimeUiController)
        {
            contentSet = runtimeContentSet;
            cameraStateController = runtimeCameraStateController;
            sceneStateController = runtimeSceneStateController;
            simulationCoordinator = runtimeSimulationCoordinator;
            captureManager = runtimeCaptureManager;
            cameraViewController = runtimeCameraViewController;
            shotEvaluationService = runtimeShotEvaluationService;
            coachingEngine = runtimeCoachingEngine;
            uiController = runtimeUiController;

            uiController.Initialize(this, cameraStateController, cameraViewController.PreviewTexture);

            sceneStateController.SceneLoaded += HandleSceneLoaded;
            simulationCoordinator.SimulationUpdated += HandleSimulationUpdated;

            uiController.ShowMainMenu();
        }

        public void StartSandbox()
        {
            currentMode = PlayMode.Sandbox;
            uiController.ShowSceneSelection(contentSet.scenes, false);
        }

        public void StartChallenge()
        {
            currentMode = PlayMode.Challenge;
            uiController.ShowSceneSelection(contentSet.scenes, true);
        }

        public void BackToMainMenu()
        {
            uiController.ShowMainMenu();
        }

        public void BackToSceneSelection()
        {
            uiController.ShowSceneSelection(contentSet.scenes, IsChallengeMode);
        }

        public void SelectScene(int sceneIndex)
        {
            currentSceneIndex = Mathf.Clamp(sceneIndex, 0, contentSet.scenes.Count - 1);
            var config = contentSet.scenes[currentSceneIndex];
            var maxFocus = Mathf.Max(config.backgroundDistanceMeters + 3f, config.subjectDistanceMeters + 2f);
            cameraStateController.ConfigureFocusRange(0.3f, maxFocus);
            sceneStateController.LoadScene(config, IsChallengeMode);
            cameraStateController.ResetTo(CameraState.FromDefaults(config.defaultCameraSettings));
            simulationCoordinator.Refresh();
            uiController.ShowSimulation(config, IsChallengeMode);
        }

        public void ResetCurrentScene()
        {
            if (sceneStateController.CurrentSceneConfig == null)
            {
                return;
            }

            SelectScene(currentSceneIndex);
        }

        public void RetryCurrentScene()
        {
            SelectScene(currentSceneIndex);
        }

        public void NextChallenge()
        {
            if (!IsChallengeMode)
            {
                BackToSceneSelection();
                return;
            }

            var nextIndex = currentSceneIndex + 1;
            if (nextIndex >= contentSet.scenes.Count)
            {
                uiController.ShowSceneSelection(contentSet.scenes, true);
                return;
            }

            SelectScene(nextIndex);
        }

        public void CaptureShot()
        {
            if (sceneStateController.CurrentSceneConfig == null || simulationCoordinator.CurrentResult == null)
            {
                return;
            }

            var challenge = IsChallengeMode ? sceneStateController.CurrentState.challengeConfig : null;
            var scoreBreakdown = shotEvaluationService.Evaluate(simulationCoordinator.CurrentResult, challenge);
            var coachingMessages = coachingEngine.GenerateMessages(
                cameraStateController.CurrentState,
                sceneStateController.CurrentState,
                simulationCoordinator.CurrentResult,
                challenge);

            StartCoroutine(CaptureRoutine(scoreBreakdown, coachingMessages));
        }

        public void QuitApplication()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private bool IsChallengeMode => currentMode == PlayMode.Challenge;

        private IEnumerator CaptureRoutine(ShotScoreBreakdown scoreBreakdown, System.Collections.Generic.IReadOnlyList<string> coachingMessages)
        {
            CapturedShotData captured = null;
            yield return captureManager.CaptureShot(
                cameraViewController.PreviewTexture,
                sceneStateController.CurrentSceneConfig.sceneId,
                cameraStateController.CurrentState,
                simulationCoordinator.CurrentResult,
                scoreBreakdown,
                coachingMessages,
                shot => captured = shot);

            if (captured != null)
            {
                uiController.ShowReview(captured, IsChallengeMode, IsChallengeMode && currentSceneIndex < contentSet.scenes.Count - 1);
            }
        }

        private void HandleSceneLoaded(SceneState sceneState, SceneConfig sceneConfig)
        {
            uiController.UpdateSceneHeader(sceneConfig, sceneState.challengeConfig, IsChallengeMode);
        }

        private void HandleSimulationUpdated(SimulationResult result)
        {
            if (sceneStateController.CurrentSceneConfig == null)
            {
                return;
            }

            uiController.RefreshHud(
                cameraStateController.CurrentState,
                sceneStateController.CurrentSceneConfig,
                sceneStateController.CurrentState,
                result,
                IsChallengeMode);
        }
    }
}
