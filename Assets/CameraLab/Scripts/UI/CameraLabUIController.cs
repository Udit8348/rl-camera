using System.Collections.Generic;
using CameraLab.Core.Controllers;
using CameraLab.Core.Models;
using CameraLab.Runtime;
using CameraLab.ScriptableObjects;
using CameraLab.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace CameraLab.UI
{
    public class CameraLabUIController : MonoBehaviour
    {
        private sealed class StepperControl
        {
            public Text valueText;
            public Button decrementButton;
            public Button incrementButton;
        }

        private CameraLabAppController appController;
        private CameraStateController cameraStateController;
        private Canvas canvas;

        private GameObject mainMenuPanel;
        private GameObject sceneSelectionPanel;
        private GameObject hudPanel;
        private GameObject reviewPanel;
        private RectTransform sceneSelectionList;

        private RawImage previewImage;
        private Text sceneSelectionTitleText;
        private Text sceneTitleText;
        private Text sceneDescriptionText;
        private Text objectiveText;
        private Text exposureMeterText;
        private Text metricsText;
        private Text focalLengthValueText;
        private Text focusDistanceValueText;
        private Slider focalLengthSlider;
        private Slider focusDistanceSlider;
        private Toggle tripodToggle;

        private RawImage reviewImage;
        private Text reviewTitleText;
        private Text reviewSettingsText;
        private Text reviewScoreText;
        private Text reviewCoachingText;
        private Button reviewNextButton;
        private Text reviewNextButtonLabel;

        private StepperControl apertureControl;
        private StepperControl shutterControl;
        private StepperControl isoControl;
        private StepperControl whiteBalanceControl;

        private bool suppressCallbacks;

        public void Initialize(CameraLabAppController runtimeAppController, CameraStateController runtimeCameraStateController, RenderTexture previewTexture)
        {
            appController = runtimeAppController;
            cameraStateController = runtimeCameraStateController;
            RuntimeUiFactory.EnsureEventSystem();

            canvas = RuntimeUiFactory.CreateCanvas(transform);
            BuildMainMenu();
            BuildSceneSelection();
            BuildHud(previewTexture);
            BuildReviewPanel();
            SetAllPanelsInactive();
        }

        public void ShowMainMenu()
        {
            SetAllPanelsInactive();
            mainMenuPanel.SetActive(true);
        }

        public void ShowSceneSelection(IReadOnlyList<SceneConfig> scenes, bool challengeMode)
        {
            SetAllPanelsInactive();
            sceneSelectionPanel.SetActive(true);
            PopulateSceneSelection(scenes, challengeMode);
        }

        public void ShowSimulation(SceneConfig sceneConfig, bool challengeMode)
        {
            SetAllPanelsInactive();
            hudPanel.SetActive(true);
            UpdateSceneHeader(sceneConfig, challengeMode ? sceneConfig.challengeConfig : null, challengeMode);
            RefreshHud(cameraStateController.CurrentState, sceneConfig, null, null, challengeMode);
        }

        public void ShowReview(CapturedShotData shot, bool challengeMode, bool hasNextChallenge)
        {
            SetAllPanelsInactive();
            reviewPanel.SetActive(true);
            reviewImage.texture = shot.screenshotTexture;
            reviewTitleText.text = challengeMode
                ? (shot.scoreSummary.passFail ? "Challenge Passed" : "Challenge Retry")
                : "Shot Review";
            reviewSettingsText.text =
                $"{CameraSettingsCatalog.FormatAperture(shot.aperture)}  •  {CameraSettingsCatalog.FormatShutter(shot.shutterSpeed)}  •  {CameraSettingsCatalog.FormatIso(shot.iso)}\n" +
                $"{shot.focalLength:0}mm  •  Focus {shot.focusDistance:0.0}m  •  WB {CameraSettingsCatalog.FormatWhiteBalance(shot.whiteBalancePreset)}  •  Tripod {(shot.tripodEnabled ? "On" : "Off")}\n" +
                $"Saved to: {shot.screenshotPath}";
            reviewScoreText.text =
                $"Overall: {shot.scoreSummary.overallScore01 * 100f:0}%\n" +
                $"Exposure: {shot.scoreSummary.exposureScore01 * 100f:0}%\n" +
                $"Sharpness: {shot.scoreSummary.sharpnessScore01 * 100f:0}%\n" +
                $"Noise: {shot.scoreSummary.noiseScore01 * 100f:0}%\n" +
                $"White Balance: {shot.scoreSummary.whiteBalanceScore01 * 100f:0}%\n" +
                $"Notes: {string.Join(" | ", shot.scoreSummary.objectiveNotes)}";
            reviewCoachingText.text = string.Join("\n", shot.coachingMessages);

            reviewNextButton.gameObject.SetActive(challengeMode);
            reviewNextButtonLabel.text = hasNextChallenge ? "Next Challenge" : "Back To Scenes";
        }

        public void UpdateSceneHeader(SceneConfig sceneConfig, ChallengeConfig challengeConfig, bool challengeMode)
        {
            if (sceneConfig == null)
            {
                return;
            }

            sceneTitleText.text = sceneConfig.displayName;
            sceneDescriptionText.text = sceneConfig.shortDescription;
            objectiveText.text = challengeMode && challengeConfig != null
                ? $"Challenge: {challengeConfig.objectiveText}"
                : "Sandbox: change one setting at a time and compare exposure, blur, noise, and white balance.";
        }

        public void RefreshHud(CameraState cameraState, SceneConfig sceneConfig, SceneState sceneState, SimulationResult result, bool challengeMode)
        {
            if (cameraState == null)
            {
                return;
            }

            suppressCallbacks = true;
            focalLengthSlider.minValue = cameraStateController.CameraPreset.minFocalLength;
            focalLengthSlider.maxValue = cameraStateController.CameraPreset.maxFocalLength;
            focusDistanceSlider.minValue = cameraStateController.MinFocusDistance;
            focusDistanceSlider.maxValue = cameraStateController.MaxFocusDistance;
            focalLengthSlider.value = cameraState.focalLengthMm;
            focusDistanceSlider.value = cameraState.focusDistanceMeters;
            tripodToggle.isOn = cameraState.tripodEnabled;
            focalLengthValueText.text = $"{cameraState.focalLengthMm:0}mm";
            focusDistanceValueText.text = $"{cameraState.focusDistanceMeters:0.0}m";
            apertureControl.valueText.text = CameraSettingsCatalog.FormatAperture(cameraState.apertureF);
            shutterControl.valueText.text = CameraSettingsCatalog.FormatShutter(cameraState.shutterSeconds);
            isoControl.valueText.text = CameraSettingsCatalog.FormatIso(cameraState.iso);
            whiteBalanceControl.valueText.text = CameraSettingsCatalog.FormatWhiteBalance(cameraState.whiteBalancePreset);
            suppressCallbacks = false;

            if (result == null)
            {
                exposureMeterText.text = "Exposure meter waiting for simulation.";
                metricsText.text = string.Empty;
                return;
            }

            var exposureDirection = result.exposureOffsetStops > 0.25f
                ? "Under"
                : result.exposureOffsetStops < -0.25f ? "Over" : "Balanced";
            exposureMeterText.text = $"Exposure meter: {result.exposureOffsetStops:+0.0;-0.0;0.0} stops ({exposureDirection})";
            metricsText.text =
                $"Subject sharpness: {result.focusSharpness01 * 100f:0}%\n" +
                $"Background blur potential: {result.dofBlurStrength * 100f:0}%\n" +
                $"Subject motion blur: {result.subjectMotionBlur01 * 100f:0}%\n" +
                $"Camera shake blur: {result.cameraShakeBlur01 * 100f:0}%\n" +
                $"Noise: {result.noiseAmount01 * 100f:0}%\n" +
                $"White balance mismatch: {result.whiteBalanceMismatch01 * 100f:0}%";
        }

        private void BuildMainMenu()
        {
            mainMenuPanel = RuntimeUiFactory.CreatePanel("MainMenuPanel", canvas.transform, new Color(0.05f, 0.07f, 0.1f, 0.92f));
            RuntimeUiFactory.Stretch(mainMenuPanel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var center = new GameObject("Center", typeof(RectTransform), typeof(VerticalLayoutGroup));
            center.transform.SetParent(mainMenuPanel.transform, false);
            RuntimeUiFactory.Stretch(center.GetComponent<RectTransform>(), new Vector2(0.3f, 0.2f), new Vector2(0.7f, 0.8f), Vector2.zero, Vector2.zero);
            var layout = center.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;

            var title = RuntimeUiFactory.CreateText("Title", center.transform, "CameraLab", 52, TextAnchor.MiddleCenter, Color.white);
            RuntimeUiFactory.AddLayoutElement(title.gameObject, minHeight: 70f);
            var subtitle = RuntimeUiFactory.CreateText("Subtitle", center.transform, "A beginner-friendly camera sandbox for exposure, focus, blur, noise, and white balance.", 24, TextAnchor.MiddleCenter, new Color(0.83f, 0.85f, 0.89f, 1f));
            RuntimeUiFactory.AddLayoutElement(subtitle.gameObject, minHeight: 90f);

            var sandboxButton = RuntimeUiFactory.CreateButton("SandboxButton", center.transform, "Start Sandbox", new Color(0.89f, 0.57f, 0.22f, 1f), Color.black);
            RuntimeUiFactory.AddLayoutElement(sandboxButton.gameObject, minHeight: 60f);
            sandboxButton.onClick.AddListener(appController.StartSandbox);

            var challengeButton = RuntimeUiFactory.CreateButton("ChallengeButton", center.transform, "Start Challenge", new Color(0.18f, 0.6f, 0.53f, 1f), Color.white);
            RuntimeUiFactory.AddLayoutElement(challengeButton.gameObject, minHeight: 60f);
            challengeButton.onClick.AddListener(appController.StartChallenge);

            var quitButton = RuntimeUiFactory.CreateButton("QuitButton", center.transform, "Quit", new Color(0.2f, 0.23f, 0.28f, 1f), Color.white);
            RuntimeUiFactory.AddLayoutElement(quitButton.gameObject, minHeight: 52f);
            quitButton.onClick.AddListener(appController.QuitApplication);
        }

        private void BuildSceneSelection()
        {
            sceneSelectionPanel = RuntimeUiFactory.CreatePanel("SceneSelectionPanel", canvas.transform, new Color(0.05f, 0.07f, 0.1f, 0.95f));
            RuntimeUiFactory.Stretch(sceneSelectionPanel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
            content.transform.SetParent(sceneSelectionPanel.transform, false);
            RuntimeUiFactory.Stretch(content.GetComponent<RectTransform>(), new Vector2(0.18f, 0.12f), new Vector2(0.82f, 0.88f), Vector2.zero, Vector2.zero);
            var layout = content.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;

            sceneSelectionTitleText = RuntimeUiFactory.CreateText("Title", content.transform, "Select A Scene", 40, TextAnchor.MiddleLeft, Color.white);
            RuntimeUiFactory.AddLayoutElement(sceneSelectionTitleText.gameObject, minHeight: 60f);

            var backButton = RuntimeUiFactory.CreateButton("BackButton", content.transform, "Back To Menu", new Color(0.2f, 0.23f, 0.28f, 1f), Color.white);
            RuntimeUiFactory.AddLayoutElement(backButton.gameObject, minHeight: 48f);
            backButton.onClick.AddListener(appController.BackToMainMenu);

            var listObject = new GameObject("SceneList", typeof(RectTransform), typeof(VerticalLayoutGroup));
            listObject.transform.SetParent(content.transform, false);
            sceneSelectionList = listObject.GetComponent<RectTransform>();
            var listLayout = listObject.GetComponent<VerticalLayoutGroup>();
            listLayout.spacing = 12f;
            listLayout.childAlignment = TextAnchor.UpperLeft;
            listLayout.childControlHeight = true;
            listLayout.childForceExpandHeight = false;
            listLayout.childControlWidth = true;
            RuntimeUiFactory.AddLayoutElement(listObject, minHeight: 400f);
        }

        private void BuildHud(RenderTexture previewTexture)
        {
            hudPanel = new GameObject("HudPanel", typeof(RectTransform));
            hudPanel.transform.SetParent(canvas.transform, false);
            RuntimeUiFactory.Stretch(hudPanel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var topBar = RuntimeUiFactory.CreatePanel("TopBar", hudPanel.transform, new Color(0.07f, 0.09f, 0.13f, 0.92f));
            RuntimeUiFactory.Stretch(topBar.GetComponent<RectTransform>(), new Vector2(0f, 0.84f), new Vector2(1f, 1f), new Vector2(16f, -16f), new Vector2(-16f, -16f));
            sceneTitleText = RuntimeUiFactory.CreateText("SceneTitle", topBar.transform, "Scene", 32, TextAnchor.UpperLeft, Color.white);
            RuntimeUiFactory.Stretch(sceneTitleText.rectTransform, new Vector2(0.02f, 0.48f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero);
            sceneDescriptionText = RuntimeUiFactory.CreateText("SceneDescription", topBar.transform, string.Empty, 20, TextAnchor.UpperLeft, new Color(0.8f, 0.84f, 0.9f, 1f));
            RuntimeUiFactory.Stretch(sceneDescriptionText.rectTransform, new Vector2(0.02f, 0.18f), new Vector2(0.98f, 0.5f), Vector2.zero, Vector2.zero);
            objectiveText = RuntimeUiFactory.CreateText("Objective", topBar.transform, string.Empty, 20, TextAnchor.UpperLeft, new Color(0.95f, 0.82f, 0.56f, 1f));
            RuntimeUiFactory.Stretch(objectiveText.rectTransform, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.22f), Vector2.zero, Vector2.zero);

            var body = new GameObject("Body", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            body.transform.SetParent(hudPanel.transform, false);
            RuntimeUiFactory.Stretch(body.GetComponent<RectTransform>(), new Vector2(0f, 0.12f), new Vector2(1f, 0.83f), new Vector2(16f, 0f), new Vector2(-16f, 0f));
            var bodyLayout = body.GetComponent<HorizontalLayoutGroup>();
            bodyLayout.spacing = 16f;
            bodyLayout.childControlHeight = true;
            bodyLayout.childControlWidth = true;
            bodyLayout.childForceExpandWidth = true;

            var previewPanel = RuntimeUiFactory.CreatePanel("PreviewPanel", body.transform, new Color(0.04f, 0.05f, 0.07f, 0.95f));
            RuntimeUiFactory.AddLayoutElement(previewPanel, flexibleWidth: 7f);
            var previewSurface = new GameObject("PreviewSurface", typeof(RectTransform), typeof(RawImage));
            previewSurface.transform.SetParent(previewPanel.transform, false);
            var previewSurfaceRect = previewSurface.GetComponent<RectTransform>();
            RuntimeUiFactory.Stretch(previewSurfaceRect, Vector2.zero, Vector2.one, new Vector2(8f, 8f), new Vector2(-8f, -8f));
            previewImage = previewSurface.GetComponent<RawImage>();
            previewImage.texture = previewTexture;
            previewImage.color = Color.white;
            previewImage.raycastTarget = false;

            var controlsPanel = RuntimeUiFactory.CreatePanel("ControlsPanel", body.transform, new Color(0.07f, 0.09f, 0.13f, 0.95f));
            RuntimeUiFactory.AddLayoutElement(controlsPanel, preferredWidth: 460f);
            var controlLayoutObject = new GameObject("ControlLayout", typeof(RectTransform), typeof(VerticalLayoutGroup));
            controlLayoutObject.transform.SetParent(controlsPanel.transform, false);
            RuntimeUiFactory.Stretch(controlLayoutObject.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(20f, 20f), new Vector2(-20f, -20f));
            var controlLayout = controlLayoutObject.GetComponent<VerticalLayoutGroup>();
            controlLayout.spacing = 12f;
            controlLayout.childControlHeight = true;
            controlLayout.childForceExpandHeight = false;

            apertureControl = CreateStepper(controlLayoutObject.transform, "Aperture", () => cameraStateController.StepAperture(-1), () => cameraStateController.StepAperture(1));
            shutterControl = CreateStepper(controlLayoutObject.transform, "Shutter", () => cameraStateController.StepShutter(-1), () => cameraStateController.StepShutter(1));
            isoControl = CreateStepper(controlLayoutObject.transform, "ISO", () => cameraStateController.StepIso(-1), () => cameraStateController.StepIso(1));
            whiteBalanceControl = CreateStepper(controlLayoutObject.transform, "White Balance", () => cameraStateController.StepWhiteBalance(-1), () => cameraStateController.StepWhiteBalance(1));

            focalLengthValueText = CreateSliderRow(controlLayoutObject.transform, "Focal Length", out focalLengthSlider);
            focusDistanceValueText = CreateSliderRow(controlLayoutObject.transform, "Focus Distance", out focusDistanceSlider);
            tripodToggle = RuntimeUiFactory.CreateToggle("TripodToggle", controlLayoutObject.transform, "Tripod Enabled", Color.white);
            RuntimeUiFactory.AddLayoutElement(tripodToggle.gameObject, minHeight: 42f);

            exposureMeterText = RuntimeUiFactory.CreateText("ExposureMeter", controlLayoutObject.transform, string.Empty, 22, TextAnchor.UpperLeft, new Color(0.95f, 0.82f, 0.56f, 1f));
            RuntimeUiFactory.AddLayoutElement(exposureMeterText.gameObject, minHeight: 44f);
            metricsText = RuntimeUiFactory.CreateText("Metrics", controlLayoutObject.transform, string.Empty, 18, TextAnchor.UpperLeft, new Color(0.83f, 0.86f, 0.91f, 1f));
            RuntimeUiFactory.AddLayoutElement(metricsText.gameObject, minHeight: 180f);

            var footer = RuntimeUiFactory.CreatePanel("Footer", hudPanel.transform, new Color(0.07f, 0.09f, 0.13f, 0.92f));
            RuntimeUiFactory.Stretch(footer.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0.11f), new Vector2(16f, 16f), new Vector2(-16f, 0f));
            var footerLayoutObject = new GameObject("FooterLayout", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            footerLayoutObject.transform.SetParent(footer.transform, false);
            RuntimeUiFactory.Stretch(footerLayoutObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(20f, 12f), new Vector2(-20f, -12f));
            var footerLayout = footerLayoutObject.GetComponent<HorizontalLayoutGroup>();
            footerLayout.spacing = 12f;
            footerLayout.childControlWidth = true;
            footerLayout.childForceExpandWidth = false;

            var captureButton = RuntimeUiFactory.CreateButton("CaptureButton", footerLayoutObject.transform, "Capture Shot", new Color(0.89f, 0.57f, 0.22f, 1f), Color.black);
            RuntimeUiFactory.AddLayoutElement(captureButton.gameObject, preferredWidth: 220f, minHeight: 54f);
            captureButton.onClick.AddListener(appController.CaptureShot);

            var resetButton = RuntimeUiFactory.CreateButton("ResetButton", footerLayoutObject.transform, "Reset Shot", new Color(0.18f, 0.6f, 0.53f, 1f), Color.white);
            RuntimeUiFactory.AddLayoutElement(resetButton.gameObject, preferredWidth: 180f, minHeight: 54f);
            resetButton.onClick.AddListener(appController.ResetCurrentScene);

            var scenesButton = RuntimeUiFactory.CreateButton("ScenesButton", footerLayoutObject.transform, "Scene Select", new Color(0.2f, 0.23f, 0.28f, 1f), Color.white);
            RuntimeUiFactory.AddLayoutElement(scenesButton.gameObject, preferredWidth: 180f, minHeight: 54f);
            scenesButton.onClick.AddListener(appController.BackToSceneSelection);

            var menuButton = RuntimeUiFactory.CreateButton("MenuButton", footerLayoutObject.transform, "Main Menu", new Color(0.2f, 0.23f, 0.28f, 1f), Color.white);
            RuntimeUiFactory.AddLayoutElement(menuButton.gameObject, preferredWidth: 160f, minHeight: 54f);
            menuButton.onClick.AddListener(appController.BackToMainMenu);

            focalLengthSlider.onValueChanged.AddListener(value =>
            {
                if (!suppressCallbacks)
                {
                    cameraStateController.SetFocalLength(value);
                }
            });
            focusDistanceSlider.onValueChanged.AddListener(value =>
            {
                if (!suppressCallbacks)
                {
                    cameraStateController.SetFocusDistance(value);
                }
            });
            tripodToggle.onValueChanged.AddListener(value =>
            {
                if (!suppressCallbacks)
                {
                    cameraStateController.SetTripodEnabled(value);
                }
            });
        }

        private void BuildReviewPanel()
        {
            reviewPanel = RuntimeUiFactory.CreatePanel("ReviewPanel", canvas.transform, new Color(0.05f, 0.07f, 0.1f, 0.96f));
            RuntimeUiFactory.Stretch(reviewPanel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var layoutObject = new GameObject("ReviewLayout", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            layoutObject.transform.SetParent(reviewPanel.transform, false);
            RuntimeUiFactory.Stretch(layoutObject.GetComponent<RectTransform>(), new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.92f), Vector2.zero, Vector2.zero);
            var layout = layoutObject.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 16f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;

            var left = RuntimeUiFactory.CreatePanel("CapturedImagePanel", layoutObject.transform, new Color(0.04f, 0.05f, 0.07f, 1f));
            RuntimeUiFactory.AddLayoutElement(left, flexibleWidth: 7f);
            var reviewSurface = new GameObject("ReviewSurface", typeof(RectTransform), typeof(RawImage));
            reviewSurface.transform.SetParent(left.transform, false);
            var reviewSurfaceRect = reviewSurface.GetComponent<RectTransform>();
            RuntimeUiFactory.Stretch(reviewSurfaceRect, Vector2.zero, Vector2.one, new Vector2(8f, 8f), new Vector2(-8f, -8f));
            reviewImage = reviewSurface.GetComponent<RawImage>();
            reviewImage.color = Color.white;
            reviewImage.raycastTarget = false;

            var right = RuntimeUiFactory.CreatePanel("ReviewInfoPanel", layoutObject.transform, new Color(0.07f, 0.09f, 0.13f, 1f));
            RuntimeUiFactory.AddLayoutElement(right, preferredWidth: 520f);
            var rightLayoutObject = new GameObject("RightLayout", typeof(RectTransform), typeof(VerticalLayoutGroup));
            rightLayoutObject.transform.SetParent(right.transform, false);
            RuntimeUiFactory.Stretch(rightLayoutObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(20f, 20f), new Vector2(-20f, -20f));
            var rightLayout = rightLayoutObject.GetComponent<VerticalLayoutGroup>();
            rightLayout.spacing = 12f;
            rightLayout.childControlHeight = true;
            rightLayout.childForceExpandHeight = false;

            reviewTitleText = RuntimeUiFactory.CreateText("ReviewTitle", rightLayoutObject.transform, "Shot Review", 36, TextAnchor.MiddleLeft, Color.white);
            RuntimeUiFactory.AddLayoutElement(reviewTitleText.gameObject, minHeight: 48f);
            reviewSettingsText = RuntimeUiFactory.CreateText("ReviewSettings", rightLayoutObject.transform, string.Empty, 18, TextAnchor.UpperLeft, new Color(0.83f, 0.86f, 0.91f, 1f));
            RuntimeUiFactory.AddLayoutElement(reviewSettingsText.gameObject, minHeight: 120f);
            reviewScoreText = RuntimeUiFactory.CreateText("ReviewScore", rightLayoutObject.transform, string.Empty, 19, TextAnchor.UpperLeft, new Color(0.95f, 0.82f, 0.56f, 1f));
            RuntimeUiFactory.AddLayoutElement(reviewScoreText.gameObject, minHeight: 150f);
            reviewCoachingText = RuntimeUiFactory.CreateText("ReviewCoaching", rightLayoutObject.transform, string.Empty, 19, TextAnchor.UpperLeft, Color.white);
            RuntimeUiFactory.AddLayoutElement(reviewCoachingText.gameObject, minHeight: 140f);

            var retryButton = RuntimeUiFactory.CreateButton("RetryButton", rightLayoutObject.transform, "Retry", new Color(0.18f, 0.6f, 0.53f, 1f), Color.white);
            RuntimeUiFactory.AddLayoutElement(retryButton.gameObject, minHeight: 52f);
            retryButton.onClick.AddListener(appController.RetryCurrentScene);

            reviewNextButton = RuntimeUiFactory.CreateButton("NextButton", rightLayoutObject.transform, "Next Challenge", new Color(0.89f, 0.57f, 0.22f, 1f), Color.black);
            RuntimeUiFactory.AddLayoutElement(reviewNextButton.gameObject, minHeight: 52f);
            reviewNextButton.onClick.AddListener(appController.NextChallenge);
            reviewNextButtonLabel = reviewNextButton.GetComponentInChildren<Text>();

            var sceneSelectButton = RuntimeUiFactory.CreateButton("SceneSelectButton", rightLayoutObject.transform, "Scene Select", new Color(0.2f, 0.23f, 0.28f, 1f), Color.white);
            RuntimeUiFactory.AddLayoutElement(sceneSelectButton.gameObject, minHeight: 52f);
            sceneSelectButton.onClick.AddListener(appController.BackToSceneSelection);

            var mainMenuButton = RuntimeUiFactory.CreateButton("MainMenuButton", rightLayoutObject.transform, "Main Menu", new Color(0.2f, 0.23f, 0.28f, 1f), Color.white);
            RuntimeUiFactory.AddLayoutElement(mainMenuButton.gameObject, minHeight: 52f);
            mainMenuButton.onClick.AddListener(appController.BackToMainMenu);
        }

        private StepperControl CreateStepper(Transform parent, string label, UnityEngine.Events.UnityAction decrementAction, UnityEngine.Events.UnityAction incrementAction)
        {
            var row = new GameObject($"{label}Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            RuntimeUiFactory.AddLayoutElement(row, minHeight: 42f);

            var labelText = RuntimeUiFactory.CreateText("Label", row.transform, label, 20, TextAnchor.MiddleLeft, Color.white);
            RuntimeUiFactory.AddLayoutElement(labelText.gameObject, preferredWidth: 150f);
            var decrement = RuntimeUiFactory.CreateButton("Decrement", row.transform, "-", new Color(0.2f, 0.23f, 0.28f, 1f), Color.white);
            RuntimeUiFactory.AddLayoutElement(decrement.gameObject, preferredWidth: 42f, minHeight: 42f);
            var value = RuntimeUiFactory.CreateText("Value", row.transform, string.Empty, 20, TextAnchor.MiddleCenter, new Color(0.95f, 0.82f, 0.56f, 1f));
            RuntimeUiFactory.AddLayoutElement(value.gameObject, preferredWidth: 150f);
            var increment = RuntimeUiFactory.CreateButton("Increment", row.transform, "+", new Color(0.2f, 0.23f, 0.28f, 1f), Color.white);
            RuntimeUiFactory.AddLayoutElement(increment.gameObject, preferredWidth: 42f, minHeight: 42f);

            decrement.onClick.AddListener(decrementAction);
            increment.onClick.AddListener(incrementAction);

            return new StepperControl
            {
                valueText = value,
                decrementButton = decrement,
                incrementButton = increment
            };
        }

        private Text CreateSliderRow(Transform parent, string label, out Slider slider)
        {
            var container = new GameObject($"{label}Container", typeof(RectTransform), typeof(VerticalLayoutGroup));
            container.transform.SetParent(parent, false);
            var layout = container.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            RuntimeUiFactory.AddLayoutElement(container, minHeight: 72f);

            var header = new GameObject($"{label}Header", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            header.transform.SetParent(container.transform, false);
            var headerLayout = header.GetComponent<HorizontalLayoutGroup>();
            headerLayout.spacing = 8f;
            headerLayout.childAlignment = TextAnchor.MiddleLeft;
            headerLayout.childControlHeight = true;
            headerLayout.childForceExpandWidth = false;

            var labelText = RuntimeUiFactory.CreateText("Label", header.transform, label, 20, TextAnchor.MiddleLeft, Color.white);
            RuntimeUiFactory.AddLayoutElement(labelText.gameObject, preferredWidth: 160f);
            var valueText = RuntimeUiFactory.CreateText("Value", header.transform, string.Empty, 20, TextAnchor.MiddleRight, new Color(0.95f, 0.82f, 0.56f, 1f));
            RuntimeUiFactory.AddLayoutElement(valueText.gameObject, preferredWidth: 100f);

            slider = RuntimeUiFactory.CreateSlider($"{label}Slider", container.transform);
            RuntimeUiFactory.AddLayoutElement(slider.gameObject, minHeight: 26f);
            return valueText;
        }

        private void PopulateSceneSelection(IReadOnlyList<SceneConfig> scenes, bool challengeMode)
        {
            for (var i = sceneSelectionList.childCount - 1; i >= 0; i--)
            {
                Destroy(sceneSelectionList.GetChild(i).gameObject);
            }

            sceneSelectionTitleText.text = challengeMode ? "Select A Challenge Scene" : "Select A Sandbox Scene";

            for (var i = 0; i < scenes.Count; i++)
            {
                var sceneIndex = i;
                var card = RuntimeUiFactory.CreatePanel($"SceneCard_{sceneIndex}", sceneSelectionList, new Color(0.09f, 0.12f, 0.16f, 1f));
                RuntimeUiFactory.AddLayoutElement(card, minHeight: 112f);
                var button = card.AddComponent<Button>();
                button.targetGraphic = card.GetComponent<Image>();
                button.onClick.AddListener(() => appController.SelectScene(sceneIndex));

                var nameText = RuntimeUiFactory.CreateText("Name", card.transform, scenes[i].displayName, 28, TextAnchor.UpperLeft, Color.white);
                RuntimeUiFactory.Stretch(nameText.rectTransform, new Vector2(0.03f, 0.5f), new Vector2(0.97f, 0.92f), Vector2.zero, Vector2.zero);
                var descText = RuntimeUiFactory.CreateText("Description", card.transform, scenes[i].shortDescription, 19, TextAnchor.UpperLeft, new Color(0.82f, 0.86f, 0.91f, 1f));
                RuntimeUiFactory.Stretch(descText.rectTransform, new Vector2(0.03f, 0.12f), new Vector2(0.97f, 0.54f), Vector2.zero, Vector2.zero);

                if (challengeMode && scenes[i].challengeConfig != null)
                {
                    var objective = RuntimeUiFactory.CreateText("Objective", card.transform, scenes[i].challengeConfig.objectiveText, 18, TextAnchor.LowerLeft, new Color(0.95f, 0.82f, 0.56f, 1f));
                    RuntimeUiFactory.Stretch(objective.rectTransform, new Vector2(0.03f, 0.02f), new Vector2(0.97f, 0.16f), Vector2.zero, Vector2.zero);
                }
            }
        }

        private void SetAllPanelsInactive()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (sceneSelectionPanel != null) sceneSelectionPanel.SetActive(false);
            if (hudPanel != null) hudPanel.SetActive(false);
            if (reviewPanel != null) reviewPanel.SetActive(false);
        }
    }
}
