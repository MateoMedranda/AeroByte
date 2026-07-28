using System.Collections;
using AeroByte.Menu.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AeroByte.Menu.Pause
{
    [DefaultExecutionOrder(-100)]
    public sealed class PauseMenuController : MonoBehaviour
    {
        private const string MainMenuSceneName = "MainMenu";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindFirstObjectByType<PauseMenuController>() != null) return;

            var pauseMenu = new GameObject("AeroByte Pause Menu", typeof(PauseMenuController));
            DontDestroyOnLoad(pauseMenu);
            Debug.Log("[PauseMenu] Ready. Press Escape or P in a gameplay scene.");
        }

        private readonly Color _panelTop = new Color(0.018f, 0.10f, 0.16f, 0.98f);
        private readonly Color _panelBottom = new Color(0.004f, 0.03f, 0.055f, 0.99f);
        private readonly Color _accent = new Color(0.05f, 0.62f, 1f, 1f);
        private readonly Color _text = new Color(0.93f, 0.97f, 1f, 1f);
        private readonly Color _muted = new Color(0.58f, 0.72f, 0.81f, 1f);

        private Font _displayFont;
        private Font _bodyFont;
        private GameObject _overlay;
        private GameObject _pausePanel;
        private GameObject _optionsPanel;
        private CanvasGroup _overlayGroup;
        private Slider _volumeSlider;
        private Text _volumeValue;
        private Text _muteLabel;
        private MenuIconGraphic _muteIcon;
        private GameObject _ownedEventSystem;
        private bool _paused;
        private bool _closing;
        private float _previousTimeScale = 1f;
        private bool _previousAudioPause;
        private bool _previousCursorVisible;
        private CursorLockMode _previousCursorLockMode;

        private void Awake()
        {
            _displayFont = MenuFontProvider.DisplayFont;
            _bodyFont = MenuFontProvider.BodyFont;
            MenuSettingsService.Load();
            BuildUi();
            EnsureEventSystem();
            _overlay.SetActive(false);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Update()
        {
            if (_overlay.activeSelf && !_closing)
            {
                _overlayGroup.alpha = Mathf.MoveTowards(_overlayGroup.alpha, 1f, Time.unscaledDeltaTime * 6f);
            }

            if (_closing || SceneManager.GetActiveScene().name == MainMenuSceneName) return;

            Keyboard keyboard = Keyboard.current;
            bool pausePressed = keyboard != null &&
                                (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame);
            if (!pausePressed) return;

            if (!_paused)
            {
                Open();
            }
            else if (_optionsPanel.activeSelf)
            {
                ShowPausePanel();
            }
            else
            {
                Resume();
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (_paused) RestoreGameplayState();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode _)
        {
            if (scene.name == MainMenuSceneName)
            {
                if (_ownedEventSystem != null) Destroy(_ownedEventSystem);
                _ownedEventSystem = null;
                _overlay.SetActive(false);
                return;
            }

            EnsureEventSystem();
        }

        public void Open()
        {
            if (_paused || _closing) return;

            EnsureEventSystem();
            _paused = true;
            _previousTimeScale = Time.timeScale;
            _previousAudioPause = AudioListener.pause;
            _previousCursorVisible = Cursor.visible;
            _previousCursorLockMode = Cursor.lockState;

            Time.timeScale = 0f;
            AudioListener.pause = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            _overlay.SetActive(true);
            _overlayGroup.alpha = 0f;
            _overlayGroup.blocksRaycasts = true;
            _overlayGroup.interactable = true;
            ShowPausePanel();
            Debug.Log("[PauseMenu] Paused.");
        }

        public void Resume()
        {
            if (!_paused || _closing) return;
            StartCoroutine(CloseAndResume());
        }

        public void ShowOptions()
        {
            _pausePanel.SetActive(false);
            _optionsPanel.SetActive(true);
            RefreshOptions();
            SelectButton("VOLVER Button");
        }

        public void ShowPausePanel()
        {
            _optionsPanel.SetActive(false);
            _pausePanel.SetActive(true);
            SelectButton("REANUDAR Button");
        }

        public void ReturnToMainMenu()
        {
            StopAllCoroutines();
            _paused = false;
            _closing = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            _overlay.SetActive(false);
            SceneManager.LoadScene(MainMenuSceneName);
        }

        private IEnumerator CloseAndResume()
        {
            _closing = true;
            _overlayGroup.blocksRaycasts = false;
            _overlayGroup.interactable = false;

            while (_overlayGroup.alpha > 0.001f)
            {
                _overlayGroup.alpha = Mathf.MoveTowards(_overlayGroup.alpha, 0f, Time.unscaledDeltaTime * 7f);
                yield return null;
            }

            _overlay.SetActive(false);
            RestoreGameplayState();
            _paused = false;
            _closing = false;
        }

        private void RestoreGameplayState()
        {
            Time.timeScale = _previousTimeScale;
            AudioListener.pause = _previousAudioPause;
            Cursor.visible = _previousCursorVisible;
            Cursor.lockState = _previousCursorLockMode;
        }

        private void BuildUi()
        {
            var canvasObject = new GameObject("Pause Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MenuUiAudio));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30000;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            _overlay = new GameObject("Pause Overlay", typeof(RectTransform), typeof(CanvasGroup));
            _overlay.transform.SetParent(canvasObject.transform, false);
            SetStretchRect(_overlay.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _overlayGroup = _overlay.GetComponent<CanvasGroup>();
            CreateStretchImage(_overlay.transform, "Pause Shade", new Color(0.002f, 0.012f, 0.024f, 0.78f));
            CreateStretchImage(_overlay.transform, "Pause Blue Wash", new Color(0.01f, 0.10f, 0.17f, 0.22f));

            _pausePanel = BuildPausePanel(_overlay.transform);
            _optionsPanel = BuildOptionsPanel(_overlay.transform);
        }

        private GameObject BuildPausePanel(Transform parent)
        {
            var panel = CreateRounded(parent, "Pause Panel", Vector2.zero, new Vector2(700f, 690f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), _panelTop, _panelBottom, 28f, new Color(0.20f, 0.72f, 0.98f, 0.42f), 2f);
            var shadow = panel.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0.01f, 0.02f, 0.72f);
            shadow.effectDistance = new Vector2(0f, -14f);

            CreateText(panel.transform, "Pause Eyebrow", "AEROBYTE  /  VUELO EN PAUSA", 13, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(50f, -48f), new Vector2(600f, 26f), _accent);
            CreateText(panel.transform, "Pause Title", "PAUSA", 48, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(50f, -86f), new Vector2(600f, 68f), _text);
            CreateText(panel.transform, "Pause Caption", "EL SIMULADOR ESTA DETENIDO", 13, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(50f, -150f), new Vector2(600f, 26f), _muted);
            CreateLocalImage(panel.transform, "Pause Divider", new Vector2(60f, -196f), new Vector2(580f, 2f), new Color(0.20f, 0.72f, 1f, 0.38f));

            CreateButton(panel.transform, "REANUDAR", new Vector2(70f, -238f), new Vector2(560f, 88f), Resume, true, MenuIconType.Play, 0f);
            CreateButton(panel.transform, "OPCIONES", new Vector2(70f, -346f), new Vector2(560f, 82f), ShowOptions, false, MenuIconType.Settings, 0.05f);
            CreateButton(panel.transform, "SALIR AL MENU PRINCIPAL", new Vector2(70f, -448f), new Vector2(560f, 82f), ReturnToMainMenu, false, MenuIconType.Exit, 0.10f);
            CreateText(panel.transform, "Pause Hint", "ESC / P  REANUDAR", 12, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(70f, -590f), new Vector2(560f, 30f), _muted);
            return panel;
        }

        private GameObject BuildOptionsPanel(Transform parent)
        {
            var panel = CreateRounded(parent, "Pause Options Panel", Vector2.zero, new Vector2(780f, 690f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), _panelTop, _panelBottom, 28f, new Color(0.20f, 0.72f, 0.98f, 0.42f), 2f);
            var shadow = panel.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0.01f, 0.02f, 0.72f);
            shadow.effectDistance = new Vector2(0f, -14f);

            CreateText(panel.transform, "Options Eyebrow", "PAUSA  /  CONFIGURACION", 13, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(60f, -48f), new Vector2(660f, 26f), _accent);
            CreateText(panel.transform, "Options Title", "OPCIONES", 42, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(60f, -84f), new Vector2(660f, 58f), _text);
            CreateLocalImage(panel.transform, "Options Divider", new Vector2(70f, -160f), new Vector2(640f, 2f), new Color(0.20f, 0.72f, 1f, 0.38f));

            CreateText(panel.transform, "Volume Label", "VOLUMEN MAESTRO", 15, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(80f, -218f), new Vector2(360f, 30f), _text);
            _volumeValue = CreateText(panel.transform, "Volume Value", "100%", 20, FontStyle.Bold, TextAnchor.MiddleRight, new Vector2(590f, -214f), new Vector2(110f, 34f), _text);
            _volumeSlider = CreateSlider(panel.transform, new Vector2(80f, -270f), new Vector2(620f, 42f));
            _volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

            var muteButton = CreateButton(panel.transform, "SILENCIAR", new Vector2(80f, -354f), new Vector2(620f, 76f), ToggleMute, false, MenuIconType.Mute, 0f);
            _muteLabel = muteButton.transform.Find("Label")?.GetComponent<Text>();
            _muteIcon = muteButton.transform.Find("Icon")?.GetComponent<MenuIconGraphic>();
            CreateButton(panel.transform, "RESTAURAR", new Vector2(80f, -450f), new Vector2(290f, 70f), RestoreDefaults, false, MenuIconType.Settings, 0.04f);
            CreateButton(panel.transform, "VOLVER", new Vector2(410f, -450f), new Vector2(290f, 70f), ShowPausePanel, true, MenuIconType.Back, 0.08f);
            CreateText(panel.transform, "Options Hint", "LOS CAMBIOS SE GUARDAN AUTOMATICAMENTE", 12, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(80f, -570f), new Vector2(620f, 28f), _muted);
            RefreshOptions();
            return panel;
        }

        private void OnVolumeChanged(float value)
        {
            MenuSettingsService.SetMasterVolume(value);
            RefreshOptions();
        }

        private void ToggleMute()
        {
            MenuSettingsService.SetMuted(!MenuSettingsService.IsMuted);
            RefreshOptions();
        }

        private void RestoreDefaults()
        {
            MenuSettingsService.RestoreDefaults();
            RefreshOptions();
        }

        private void RefreshOptions()
        {
            if (_volumeSlider == null) return;
            _volumeSlider.SetValueWithoutNotify(MenuSettingsService.MasterVolume);
            _volumeValue.text = $"{Mathf.RoundToInt(MenuSettingsService.MasterVolume * 100f)}%";
            _muteLabel.text = MenuSettingsService.IsMuted ? "ACTIVAR SONIDO" : "SILENCIAR";
            if (_muteIcon != null) _muteIcon.IconType = MenuSettingsService.IsMuted ? MenuIconType.Sound : MenuIconType.Mute;
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null && EventSystem.current.isActiveAndEnabled) return;

            _ownedEventSystem = new GameObject("Pause EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            _ownedEventSystem.transform.SetParent(transform, false);
            _ownedEventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private void SelectButton(string buttonName)
        {
            if (EventSystem.current == null) return;
            foreach (var button in _overlay.GetComponentsInChildren<Button>(true))
            {
                if (button.name != buttonName) continue;
                EventSystem.current.SetSelectedGameObject(button.gameObject);
                return;
            }
        }

        private Button CreateButton(Transform parent, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action, bool primary, MenuIconType iconType, float delay)
        {
            Color normalTop = primary ? new Color(0.05f, 0.58f, 1f, 1f) : new Color(0.025f, 0.14f, 0.21f, 0.98f);
            Color normalBottom = primary ? new Color(0.015f, 0.30f, 0.72f, 1f) : new Color(0.006f, 0.055f, 0.09f, 1f);
            Color hoverTop = primary ? new Color(0.12f, 0.72f, 1f, 1f) : new Color(0.04f, 0.27f, 0.39f, 1f);
            Color hoverBottom = primary ? new Color(0.03f, 0.43f, 0.88f, 1f) : new Color(0.01f, 0.12f, 0.19f, 1f);

            var buttonObject = new GameObject($"{label} Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(CanvasGroup), typeof(MenuRoundedGraphic), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            SetRect(buttonObject.GetComponent<RectTransform>(), position, size, new Vector2(0f, 1f), new Vector2(0f, 1f));
            var background = buttonObject.GetComponent<MenuRoundedGraphic>();
            background.SetStyle(normalTop, normalBottom, 16f, new Color(0.24f, 0.76f, 1f, primary ? 0.52f : 0.25f), primary ? 2f : 1f);
            var button = buttonObject.GetComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(action);

            var icon = CreateIcon(buttonObject.transform, "Icon", iconType, new Vector2(28f, 0f), new Vector2(36f, 36f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), _text);
            var text = CreateText(buttonObject.transform, "Label", label, primary ? 19 : 17, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(86f, 0f), new Vector2(size.x - 130f, size.y), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), _text);
            buttonObject.AddComponent<MenuButtonMotion>().Configure(background, icon, null, text, normalTop, normalBottom, hoverTop, hoverBottom, _text, Color.white, delay);
            return button;
        }

        private Slider CreateSlider(Transform parent, Vector2 position, Vector2 size)
        {
            var root = new GameObject("Pause Volume Slider", typeof(RectTransform), typeof(Slider));
            root.transform.SetParent(parent, false);
            SetRect(root.GetComponent<RectTransform>(), position, size, new Vector2(0f, 1f), new Vector2(0f, 1f));
            CreateStretchImage(root.transform, "Background", new Color(0.002f, 0.02f, 0.035f, 0.92f), new Vector2(0f, 0.32f), new Vector2(1f, 0.68f), Vector2.zero, Vector2.zero);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(root.transform, false);
            SetStretchRect(fillArea.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(8f, 13f), new Vector2(-8f, -13f));
            var fill = CreateStretchImage(fillArea.transform, "Fill", _accent);

            var handleArea = new GameObject("Handle Area", typeof(RectTransform));
            handleArea.transform.SetParent(root.transform, false);
            SetStretchRect(handleArea.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(10f, 0f), new Vector2(-10f, 0f));
            var handle = CreateLocalImage(handleArea.transform, "Handle", Vector2.zero, new Vector2(24f, 38f), _text, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f));

            var slider = root.GetComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            return slider;
        }

        private MenuIconGraphic CreateIcon(Transform parent, string objectName, MenuIconType type, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot, Color color)
        {
            var iconObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuIconGraphic));
            iconObject.transform.SetParent(parent, false);
            SetRect(iconObject.GetComponent<RectTransform>(), position, size, anchor, pivot);
            var icon = iconObject.GetComponent<MenuIconGraphic>();
            icon.Configure(type, color);
            return icon;
        }

        private Text CreateText(Transform parent, string objectName, string value, int fontSize, FontStyle style, TextAnchor alignment, Vector2 position, Vector2 size, Color color)
        {
            return CreateText(parent, objectName, value, fontSize, style, alignment, position, size, new Vector2(0f, 1f), new Vector2(0f, 1f), color);
        }

        private Text CreateText(Transform parent, string objectName, string value, int fontSize, FontStyle style, TextAnchor alignment, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot, Color color)
        {
            var textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            SetRect(textObject.GetComponent<RectTransform>(), position, size, anchor, pivot);
            var text = textObject.GetComponent<Text>();
            text.font = style == FontStyle.Bold ? _displayFont : _bodyFont;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.text = value;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject CreateRounded(Transform parent, string objectName, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot, Color top, Color bottom, float radius, Color border, float borderWidth)
        {
            var roundedObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuRoundedGraphic));
            roundedObject.transform.SetParent(parent, false);
            SetRect(roundedObject.GetComponent<RectTransform>(), position, size, anchor, pivot);
            roundedObject.GetComponent<MenuRoundedGraphic>().SetStyle(top, bottom, radius, border, borderWidth);
            return roundedObject;
        }

        private static Image CreateLocalImage(Transform parent, string objectName, Vector2 position, Vector2 size, Color color, Vector2? anchor = null, Vector2? pivot = null)
        {
            var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            SetRect(imageObject.GetComponent<RectTransform>(), position, size, anchor ?? new Vector2(0f, 1f), pivot ?? new Vector2(0f, 1f));
            var image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Image CreateStretchImage(Transform parent, string objectName, Color color, Vector2? anchorMin = null, Vector2? anchorMax = null, Vector2? offsetMin = null, Vector2? offsetMax = null)
        {
            var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            SetStretchRect(imageObject.GetComponent<RectTransform>(), anchorMin ?? Vector2.zero, anchorMax ?? Vector2.one, offsetMin ?? Vector2.zero, offsetMax ?? Vector2.zero);
            var image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = objectName == "Pause Shade";
            return image;
        }

        private static void SetRect(RectTransform rect, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void SetStretchRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
