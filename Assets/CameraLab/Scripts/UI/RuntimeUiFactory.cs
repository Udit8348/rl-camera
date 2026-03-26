using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace CameraLab.UI
{
    public static class RuntimeUiFactory
    {
        private static Font cachedFont;
        private static Sprite cachedSprite;
        private static Sprite cachedKnobSprite;
        private static Sprite cachedCheckmarkSprite;

        public static Font DefaultFont => cachedFont ?? (cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf"));

        private static Sprite DefaultSprite => cachedSprite ?? (cachedSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd"));

        private static Sprite KnobSprite => cachedKnobSprite ?? (cachedKnobSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd"));

        private static Sprite CheckmarkSprite => cachedCheckmarkSprite ?? (cachedCheckmarkSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Checkmark.psd"));

        public static void EnsureEventSystem()
        {
            var eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                var eventSystemObject = new GameObject("EventSystem");
                eventSystem = eventSystemObject.AddComponent<EventSystem>();
            }

            ConfigureInputModule(eventSystem.gameObject);
        }

        private static void ConfigureInputModule(GameObject eventSystemObject)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var legacyModule = eventSystemObject.GetComponent<StandaloneInputModule>();
            if (legacyModule != null)
            {
                Object.Destroy(legacyModule);
            }

            var inputSystemModule = eventSystemObject.GetComponent<InputSystemUIInputModule>();
            if (inputSystemModule == null)
            {
                inputSystemModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            }

            inputSystemModule.AssignDefaultActions();
#else
            var inputSystemModule = eventSystemObject.GetComponent("InputSystemUIInputModule");
            if (inputSystemModule is Component inputSystemComponent)
            {
                Object.Destroy(inputSystemComponent);
            }

            if (eventSystemObject.GetComponent<StandaloneInputModule>() == null)
            {
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }
#endif
        }

        public static Canvas CreateCanvas(Transform parent)
        {
            var canvasObject = new GameObject("CameraLabCanvas");
            canvasObject.transform.SetParent(parent, false);

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            var image = panel.GetComponent<Image>();
            image.sprite = DefaultSprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            return panel;
        }

        public static Text CreateText(string name, Transform parent, string content, int fontSize, TextAnchor alignment, Color color)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var text = textObject.GetComponent<Text>();
            text.font = DefaultFont;
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        public static Button CreateButton(string name, Transform parent, string label, Color backgroundColor, Color textColor)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var image = buttonObject.GetComponent<Image>();
            image.sprite = DefaultSprite;
            image.type = Image.Type.Sliced;
            image.color = backgroundColor;

            var button = buttonObject.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = backgroundColor;
            colors.highlightedColor = backgroundColor * 1.1f;
            colors.pressedColor = backgroundColor * 0.9f;
            colors.selectedColor = backgroundColor;
            button.colors = colors;

            var labelText = CreateText("Label", buttonObject.transform, label, 22, TextAnchor.MiddleCenter, textColor);
            Stretch(labelText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            return button;
        }

        public static Slider CreateSlider(string name, Transform parent)
        {
            var sliderObject = new GameObject(name, typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(parent, false);

            var slider = sliderObject.GetComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;

            var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(sliderObject.transform, false);
            var backgroundImage = background.GetComponent<Image>();
            backgroundImage.sprite = DefaultSprite;
            backgroundImage.type = Image.Type.Sliced;
            backgroundImage.color = new Color(0.2f, 0.22f, 0.27f, 1f);
            Stretch(background.GetComponent<RectTransform>(), new Vector2(0f, 0.25f), new Vector2(1f, 0.75f), new Vector2(0f, 0f), new Vector2(0f, 0f));

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObject.transform, false);
            Stretch(fillArea.GetComponent<RectTransform>(), new Vector2(0f, 0.25f), new Vector2(1f, 0.75f), new Vector2(12f, 0f), new Vector2(-12f, 0f));

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillImage = fill.GetComponent<Image>();
            fillImage.sprite = DefaultSprite;
            fillImage.type = Image.Type.Sliced;
            fillImage.color = new Color(0.89f, 0.57f, 0.22f, 1f);
            Stretch(fill.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

            var handleSlideArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleSlideArea.transform.SetParent(sliderObject.transform, false);
            Stretch(handleSlideArea.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(10f, 0f), new Vector2(-10f, 0f));

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleSlideArea.transform, false);
            var handleImage = handle.GetComponent<Image>();
            handleImage.sprite = KnobSprite;
            handleImage.color = Color.white;
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(26f, 26f);

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            return slider;
        }

        public static Toggle CreateToggle(string name, Transform parent, string label, Color textColor)
        {
            var toggleObject = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            toggleObject.transform.SetParent(parent, false);

            var toggle = toggleObject.GetComponent<Toggle>();

            var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(toggleObject.transform, false);
            var backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0f, 0.5f);
            backgroundRect.anchorMax = new Vector2(0f, 0.5f);
            backgroundRect.sizeDelta = new Vector2(26f, 26f);
            backgroundRect.anchoredPosition = new Vector2(14f, 0f);
            var backgroundImage = background.GetComponent<Image>();
            backgroundImage.sprite = DefaultSprite;
            backgroundImage.type = Image.Type.Sliced;
            backgroundImage.color = new Color(0.17f, 0.2f, 0.24f, 1f);

            var checkmark = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            checkmark.transform.SetParent(background.transform, false);
            var checkmarkRect = checkmark.GetComponent<RectTransform>();
            Stretch(checkmarkRect, new Vector2(0.15f, 0.15f), new Vector2(0.85f, 0.85f), Vector2.zero, Vector2.zero);
            var checkmarkImage = checkmark.GetComponent<Image>();
            checkmarkImage.sprite = CheckmarkSprite;
            checkmarkImage.color = new Color(0.89f, 0.57f, 0.22f, 1f);

            var labelText = CreateText("Label", toggleObject.transform, label, 20, TextAnchor.MiddleLeft, textColor);
            var labelRect = labelText.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(42f, 0f);
            labelRect.offsetMax = new Vector2(0f, 0f);

            toggle.targetGraphic = backgroundImage;
            toggle.graphic = checkmarkImage;
            return toggle;
        }

        public static LayoutElement AddLayoutElement(GameObject target, float minHeight = 0f, float preferredWidth = -1f, float flexibleWidth = 0f)
        {
            var layout = target.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = target.AddComponent<LayoutElement>();
            }

            layout.minHeight = minHeight;
            layout.preferredWidth = preferredWidth;
            layout.flexibleWidth = flexibleWidth;
            return layout;
        }

        public static void Stretch(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }
    }
}
