using System.Collections;
using AeroByte.Menu.Credits;
using AeroByte.Menu.Audio;
using AeroByte.Menu.LevelSelection;
using AeroByte.Menu.Loading;
using AeroByte.Menu.Profile;
using AeroByte.Menu.Startup;
using AeroByte.Menu.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AeroByte.Menu
{
    [ExecuteAlways]
    public sealed class MainMenuController : MonoBehaviour
    {
        private const string LayoutMarkerName = "AeroByte Credits Layout v36";
        private const string MenuBackgroundPath = "Assets/_Game/Menu/Art/Backgrounds/MAIN MENU/BG-MAINMENU.png";
        private const string SelectorBackgroundPath = "Assets/_Game/Menu/Art/Backgrounds/LEVEL SELECTOR/BG- LEVEL SELECTOR.png";
        private const string BeachBackgroundPath = "Assets/_Game/Menu/Art/Backgrounds/LEVEL SELECTOR/PLAYA/BGE-PLAYA.png";
        private const string CityBackgroundPath = "Assets/_Game/Menu/Art/Backgrounds/LEVEL SELECTOR/CIUDAD/BGE-CIUDAD.png";
        private const string DesertBackgroundPath = "Assets/_Game/Menu/Art/Backgrounds/LEVEL SELECTOR/DESIERTO/BGE-DESIERTO.png";
        private const string ForestBackgroundPath = "Assets/_Game/Menu/Art/Backgrounds/LEVEL SELECTOR/BOSQUE/BGE-BOSQUE.png";
        private const string CessnaArtworkPath = "Assets/_Game/Menu/Missions/A-CESSNA.png";
        private const string BoeingArtworkPath = "Assets/_Game/Menu/Missions/A-BOEING.png";
        private const string TomcatArtworkPath = "Assets/_Game/Menu/Missions/A-F14TOMCAT.png";
        private const string StartupBackgroundPath = "Assets/_Game/Menu/STARTGAME/BG-STARTGAME.png";
        private const string MainMenuMusicPath = "Assets/_Game/Menu/OST/MAINMENU.mp3";
        private const string LevelInfoMusicPath = "Assets/_Game/Menu/OST/INFOLEVEL.mp3";

        [SerializeField] private Texture2D menuBackground;
        [SerializeField] private Texture2D selectorBackground;
        [SerializeField] private Texture2D beachBackground;
        [SerializeField] private Texture2D cityBackground;
        [SerializeField] private Texture2D desertBackground;
        [SerializeField] private Texture2D forestBackground;
        [SerializeField] private Texture2D cessnaArtwork;
        [SerializeField] private Texture2D boeingArtwork;
        [SerializeField] private Texture2D tomcatArtwork;
        [SerializeField] private Texture2D startupBackground;
        [SerializeField] private AudioClip mainMenuMusic;
        [SerializeField] private AudioClip levelInfoMusic;

        private readonly Color _panelColor = new Color(0.018f, 0.075f, 0.125f, 0.89f);
        private readonly Color _secondaryPanelColor = new Color(0.025f, 0.12f, 0.19f, 0.92f);
        private readonly Color _accentColor = new Color(0.04f, 0.55f, 1f, 1f);
        private readonly Color _textColor = new Color(0.93f, 0.97f, 1f, 1f);
        private readonly Color _mutedTextColor = new Color(0.65f, 0.75f, 0.82f, 1f);

        private Font _displayFont;
        private Font _bodyFont;
        private GameObject _mainPanel;
        private GameObject _levelSelectPanel;
        private GameObject _optionsPanel;
        private GameObject _creditsPanel;
        private GameObject _exitPanel;
        private GameObject _profileEditorPanel;
        private Slider _volumeSlider;
        private Text _volumeValue;
        private Slider _musicVolumeSlider;
        private Text _musicVolumeValue;
        private Text _muteLabel;
        private MenuIconGraphic _muteIcon;
        private Text _pilotNameText;
        private PilotAvatarGraphic _pilotAvatar;
        private PilotProfileEditorView _profileEditorView;
        private LevelSelectionView _levelSelectionView;
        private LoadingScreenView _loadingScreen;
        private StartupScreenView _startupScreen;
        private MenuMusicController _menuMusic;
        private InputAction _menuCancelAction;
        private bool _panelsInitialized;

        private void OnEnable()
        {
            _displayFont = MenuFontProvider.DisplayFont;
            _bodyFont = MenuFontProvider.BodyFont;

#if UNITY_EDITOR
            ResolveConfiguredBackground();
#endif

            if (!Application.isPlaying)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.delayCall += EnsureEditorPreview;
#endif
                return;
            }

            Time.timeScale = 1f;
            AudioListener.pause = false;
            MenuSettingsService.Load();
            EnsureHierarchy();
            BindExistingHierarchy();
            SetInitialPanelState();
            BuildInput();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveConfiguredBackground();
            UnityEditor.EditorApplication.delayCall += ApplyConfiguredBackground;
        }

        private void ResolveConfiguredBackground()
        {
            if (menuBackground != null && selectorBackground != null && beachBackground != null && cityBackground != null && desertBackground != null && forestBackground != null && cessnaArtwork != null && boeingArtwork != null && tomcatArtwork != null && startupBackground != null && mainMenuMusic != null && levelInfoMusic != null) return;

            menuBackground = LoadTexture(menuBackground, MenuBackgroundPath);
            selectorBackground = LoadTexture(selectorBackground, SelectorBackgroundPath);
            beachBackground = LoadTexture(beachBackground, BeachBackgroundPath);
            cityBackground = LoadTexture(cityBackground, CityBackgroundPath);
            desertBackground = LoadTexture(desertBackground, DesertBackgroundPath);
            forestBackground = LoadTexture(forestBackground, ForestBackgroundPath);
            cessnaArtwork = LoadTexture(cessnaArtwork, CessnaArtworkPath);
            boeingArtwork = LoadTexture(boeingArtwork, BoeingArtworkPath);
            tomcatArtwork = LoadTexture(tomcatArtwork, TomcatArtworkPath);
            startupBackground = LoadTexture(startupBackground, StartupBackgroundPath);
            mainMenuMusic = LoadAudioClip(mainMenuMusic, MainMenuMusicPath);
            levelInfoMusic = LoadAudioClip(levelInfoMusic, LevelInfoMusicPath);

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }

        private static Texture2D LoadTexture(Texture2D current, string path)
        {
            return current != null ? current : UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static AudioClip LoadAudioClip(AudioClip current, string path)
        {
            return current != null ? current : UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        private void ApplyConfiguredBackground()
        {
            if (this == null || Application.isPlaying) return;

            var canvasRoot = transform.Find("AeroByte Main Menu");
            var backdrop = canvasRoot == null ? null : canvasRoot.Find("Aerodrome Background")?.GetComponent<RawImage>();
            if (backdrop != null && menuBackground != null)
            {
                backdrop.texture = menuBackground;
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }

        private void EnsureEditorPreview()
        {
            if (this == null || Application.isPlaying || gameObject.scene.path != "Assets/_Game/Menu/Scenes/MainMenu.unity") return;

            bool changed = EnsureHierarchy();
            BindExistingHierarchy(false);
            SetInitialPanelState();

            if (changed)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }
#endif

        private bool EnsureHierarchy()
        {
            bool changed = false;
            var cameraRoot = transform.Find("Main Menu Camera");
            if (cameraRoot == null)
            {
                BuildCamera();
                changed = true;
            }
            else if (cameraRoot.GetComponent<AudioListener>() == null)
            {
                cameraRoot.gameObject.AddComponent<AudioListener>();
                changed = true;
            }

            var canvasRoot = transform.Find("AeroByte Main Menu");
            if (canvasRoot != null && canvasRoot.Find(LayoutMarkerName) == null)
            {
                DestroyImmediate(canvasRoot.gameObject);
                canvasRoot = null;
                changed = true;
            }

            if (canvasRoot == null)
            {
                BuildMenu();
                changed = true;
            }
            else
            {
                EnsureBackdropTexture();
            }

            return changed;
        }

        public void Play()
        {
            MenuMusicController.PlayMain();
            _levelSelectionView?.ShowSelection();
            ShowPanel(_levelSelectPanel, false);
        }
        public void ShowMain()
        {
            MenuMusicController.PlayMain();
            ShowPanel(_mainPanel, false);
        }
        public void LoadLevel(string sceneName)
        {
            MenuMusicController.FadeToSilence();
            if (_loadingScreen != null) _loadingScreen.BeginLoad(sceneName);
            else SceneManager.LoadScene(sceneName);
        }
        public void ShowOptions()
        {
            MenuMusicController.PlayMain();
            ShowPanel(_optionsPanel, false);
        }
        public void ShowCredits()
        {
            MenuMusicController.PlayMain();
            ShowPanel(_creditsPanel, false);
        }
        public void ShowExitConfirmation()
        {
            MenuMusicController.PlayMain();
            ShowPanel(_exitPanel, false);
        }

        public void ShowProfileEditor()
        {
            MenuMusicController.PlayMain();
            _profileEditorView?.Prepare();
            ShowPanel(_profileEditorPanel, false);
        }

        public void ToggleMute()
        {
            MenuSettingsService.SetMuted(!MenuSettingsService.IsMuted);
            RefreshSoundControls();
        }

        public void RestoreDefaults()
        {
            MenuSettingsService.RestoreDefaults();
            RefreshSoundControls();
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void BuildCamera()
        {
            var cameraObject = new GameObject("Main Menu Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(transform, false);
            cameraObject.tag = "MainCamera";
            var menuCamera = cameraObject.GetComponent<Camera>();
            menuCamera.clearFlags = CameraClearFlags.SolidColor;
            menuCamera.backgroundColor = new Color(0.01f, 0.04f, 0.07f, 1f);
            menuCamera.cullingMask = 0;
            menuCamera.depth = -100f;
        }

        private void BuildInput()
        {
            if (_menuCancelAction != null) _menuCancelAction.performed -= OnMenuCancel;
            foreach (var existingEventSystem in Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (existingEventSystem.transform.IsChildOf(transform))
                {
                    existingEventSystem.gameObject.SetActive(false);
                    Destroy(existingEventSystem.gameObject);
                }
                else
                {
                    existingEventSystem.gameObject.SetActive(false);
                }
            }

            var eventSystemObject = new GameObject("Menu EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystemObject.transform.SetParent(transform, false);
            var menuEventSystem = eventSystemObject.GetComponent<EventSystem>();
            var module = eventSystemObject.GetComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
            menuEventSystem.sendNavigationEvents = true;
            _menuCancelAction = module.cancel?.action;
            if (_menuCancelAction != null) _menuCancelAction.performed += OnMenuCancel;
            EventSystem.current = menuEventSystem;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            var canvasRoot = transform.Find("AeroByte Main Menu");
            var playButton = canvasRoot == null ? null : FindDescendant(canvasRoot, "JUGAR Button");
            if (playButton != null)
            {
                menuEventSystem.firstSelectedGameObject = playButton.gameObject;
                menuEventSystem.SetSelectedGameObject(playButton.gameObject);
                StartCoroutine(SelectWhenInputReady(playButton.gameObject));
            }
        }

        private void Update()
        {
            if (Application.isPlaying && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (_loadingScreen != null && _loadingScreen.IsLoading) return;
                ShowMain();
            }
        }

        private void BuildMenu()
        {
            var canvasObject = new GameObject("AeroByte Main Menu", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvasObject.AddComponent<MenuUiAudio>();
            _menuMusic = canvasObject.AddComponent<MenuMusicController>();
            _menuMusic.Initialize(mainMenuMusic, levelInfoMusic);
            var layoutMarker = new GameObject(LayoutMarkerName, typeof(RectTransform));
            layoutMarker.transform.SetParent(canvasObject.transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            BuildBackdrop(canvasObject.transform);
            _mainPanel = BuildMainPanel(canvasObject.transform);
            _levelSelectPanel = BuildLevelSelectPanel(canvasObject.transform);
            _optionsPanel = BuildOptionsPanel(canvasObject.transform);
            _creditsPanel = BuildCreditsPanel(canvasObject.transform);
            _exitPanel = BuildExitPanel(canvasObject.transform);
            _profileEditorPanel = BuildProfileEditorPanel(canvasObject.transform);
            _loadingScreen = BuildLoadingScreen(canvasObject.transform);
            _startupScreen = BuildStartupScreen(canvasObject.transform);
            SetInitialPanelState();
        }

        private void BuildBackdrop(Transform parent)
        {
            var backdrop = CreateStretchRawImage(parent, "Aerodrome Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
            backdrop.gameObject.AddComponent<MenuBackgroundMotion>();
            if (menuBackground != null)
            {
                backdrop.texture = menuBackground;
            }

            CreateStretchImage(parent, "Cinematic Shade", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0f, 0.025f, 0.06f, 0.10f));
            CreateStretchImage(parent, "Left Shade", Vector2.zero, new Vector2(0.38f, 1f), Vector2.zero, Vector2.zero, new Color(0f, 0.02f, 0.05f, 0.20f));

            var ambientObject = new GameObject("Ambient Navigation Lights", typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuAmbientOverlay));
            ambientObject.transform.SetParent(parent, false);
            SetStretchRect(ambientObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            ambientObject.GetComponent<MenuAmbientOverlay>().color = new Color(0.12f, 0.72f, 1f, 0.75f);

        }

        private void EnsureBackdropTexture()
        {
            var canvasRoot = transform.Find("AeroByte Main Menu");
            var backdrop = canvasRoot == null ? null : canvasRoot.Find("Aerodrome Background")?.GetComponent<RawImage>();
            if (backdrop == null) return;

            if (menuBackground != null)
            {
                backdrop.texture = menuBackground;
                return;
            }

            backdrop.texture = null;
        }

        private void BindExistingHierarchy(bool bindActions = true)
        {
            var canvasRoot = transform.Find("AeroByte Main Menu");
            if (canvasRoot == null) return;

            RepairUiComponents(canvasRoot);
            EnsureOptionsRuntimeLayout(canvasRoot);

            _mainPanel = FindDescendant(canvasRoot, "Main Panel")?.gameObject;
            _levelSelectPanel = FindDescendant(canvasRoot, "Level Select Panel")?.gameObject;
            _levelSelectionView = _levelSelectPanel == null ? null : _levelSelectPanel.GetComponent<LevelSelectionView>();
            ConfigureLevelSelectionTextures();
            _optionsPanel = FindDescendant(canvasRoot, "Options Panel")?.gameObject;
            _creditsPanel = FindDescendant(canvasRoot, "Credits Panel")?.gameObject;
            _exitPanel = FindDescendant(canvasRoot, "Exit Screen Shade")?.gameObject;
            _profileEditorPanel = FindDescendant(canvasRoot, "Profile Editor Shade")?.gameObject;
            _volumeSlider = FindDescendant(canvasRoot, "Master Volume Slider")?.GetComponent<Slider>();
            _volumeValue = FindDescendant(canvasRoot, "Volume Value")?.GetComponent<Text>();
            _musicVolumeSlider = FindDescendant(canvasRoot, "Music Volume Slider")?.GetComponent<Slider>();
            _musicVolumeValue = FindDescendant(canvasRoot, "Music Volume Value")?.GetComponent<Text>();
            EnsureSliderInput(_volumeSlider);
            EnsureSliderInput(_musicVolumeSlider);
            _muteLabel = FindDescendant(canvasRoot, "SILENCIAR Label")?.GetComponent<Text>();
            _muteIcon = FindDescendant(canvasRoot, "SILENCIAR Icon")?.GetComponent<MenuIconGraphic>();
            _pilotNameText = FindDescendant(canvasRoot, "Pilot Name")?.GetComponent<Text>();
            _pilotAvatar = FindDescendant(canvasRoot, "Active Pilot Avatar")?.GetComponent<PilotAvatarGraphic>();
            _profileEditorView = FindDescendant(canvasRoot, "Profile Editor Card")?.GetComponent<PilotProfileEditorView>();
            _loadingScreen = FindDescendant(canvasRoot, "Loading Screen")?.GetComponent<LoadingScreenView>();
            _startupScreen = FindDescendant(canvasRoot, "Startup Screen")?.GetComponent<StartupScreenView>();
            _menuMusic = canvasRoot.GetComponent<MenuMusicController>();
            _menuMusic?.Initialize(mainMenuMusic, levelInfoMusic);
            ApplyFonts(canvasRoot);

            if (!bindActions) return;

            BindButton(canvasRoot, "JUGAR Button", Play);
            _levelSelectionView?.Bind(LoadLevel, ShowMain);
            BindButton(canvasRoot, "OPCIONES Button", ShowOptions);
            BindButton(canvasRoot, "CRÉDITOS Button", ShowCredits);
            BindButton(canvasRoot, "SALIR Button", ShowExitConfirmation, 0);
            BindButton(canvasRoot, "SILENCIAR Button", ToggleMute);
            BindButton(canvasRoot, "RESTAURAR PREDETERMINADOS Button", RestoreDefaults);
            BindButton(canvasRoot, "VOLVER AL MENÚ Button", ShowMain);
            BindButton(canvasRoot, "Back Button", ShowMain);
            BindButton(canvasRoot, "CANCELAR Button", ShowMain);
            BindButton(canvasRoot, "SALIR Button", Quit, 1);
            BindButton(canvasRoot, "Pilot Profile Button", ShowProfileEditor);

            _profileEditorView?.Bind(_displayFont, _bodyFont, OnProfileSaved, ShowMain);

            if (_volumeSlider != null)
            {
                _volumeSlider.onValueChanged.RemoveAllListeners();
                _volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }
            if (_musicVolumeSlider != null)
            {
                _musicVolumeSlider.onValueChanged.RemoveAllListeners();
                _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }
            RefreshSoundControls();
            RefreshPilotProfile();
        }

        private static void RepairUiComponents(Transform root)
        {
            foreach (var graphic in root.GetComponentsInChildren<Graphic>(true))
            {
                if (graphic.GetComponent<CanvasRenderer>() == null)
                {
                    graphic.gameObject.AddComponent<CanvasRenderer>();
                }
            }

            foreach (var transition in root.GetComponentsInChildren<MenuPanelTransition>(true))
            {
                if (transition.GetComponent<CanvasGroup>() == null)
                {
                    transition.gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        private void ApplyFonts(Transform root)
        {
            foreach (var text in root.GetComponentsInChildren<Text>(true))
            {
                text.font = text.fontStyle == FontStyle.Bold ? _displayFont : _bodyFont;
            }
        }

        private static Transform FindDescendant(Transform root, string objectName)
        {
            if (root.name == objectName) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindDescendant(root.GetChild(i), objectName);
                if (found != null) return found;
            }
            return null;
        }

        private static void BindButton(Transform root, string objectName, UnityEngine.Events.UnityAction action, int occurrence = 0)
        {
            int currentOccurrence = 0;
            var buttons = root.GetComponentsInChildren<Button>(true);
            foreach (var button in buttons)
            {
                if (button.gameObject.name != objectName) continue;
                if (currentOccurrence++ != occurrence) continue;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(action);
                return;
            }
        }

        private GameObject BuildMainPanel(Transform parent)
        {
            var panel = CreateFixedPanel(parent, "Main Panel", new Vector2(90f, -8f), new Vector2(520f, 820f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Color.clear);
            panel.AddComponent<MenuPanelTransition>().Configure(new Vector2(-36f, 0f));
            var navLabel = CreateText(panel.transform, "Navigation Label", 20, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(0f, -174f), new Vector2(440f, 34f), "MENÚ PRINCIPAL");
            navLabel.color = new Color(0.46f, 0.78f, 0.95f, 1f);
            CreateLocalImage(panel.transform, "Navigation Line", new Vector2(0f, -220f), new Vector2(480f, 3f), new Color(0.30f, 0.78f, 1f, 0.34f));

            CreateButton(panel.transform, "JUGAR", new Vector2(0f, -244f), new Vector2(480f, 90f), Play, true, MenuIconType.Play, 0.03f);
            CreateButton(panel.transform, "OPCIONES", new Vector2(0f, -350f), new Vector2(480f, 84f), ShowOptions, false, MenuIconType.Settings, 0.09f);
            CreateButton(panel.transform, "CRÉDITOS", new Vector2(0f, -450f), new Vector2(480f, 84f), ShowCredits, false, MenuIconType.Credits, 0.15f);
            CreateButton(panel.transform, "SALIR", new Vector2(0f, -550f), new Vector2(480f, 84f), ShowExitConfirmation, false, MenuIconType.Exit, 0.21f);

            Color profileTop = new Color(0.025f, 0.16f, 0.24f, 0.97f);
            Color profileBottom = new Color(0.006f, 0.045f, 0.075f, 0.99f);
            var profile = CreateRoundedObject(panel.transform, "Pilot Profile Button", new Vector2(1320f, -704f), new Vector2(460f, 118f), new Vector2(0f, 1f), new Vector2(0f, 1f), profileTop, profileBottom, 20f, new Color(0.20f, 0.74f, 1f, 0.52f), 2f);
            profile.AddComponent<CanvasGroup>();
            var profileBackground = profile.GetComponent<MenuRoundedGraphic>();
            var profileButton = profile.AddComponent<Button>();
            profileButton.targetGraphic = profileBackground;
            profileButton.transition = Selectable.Transition.None;
            profileButton.onClick.AddListener(ShowProfileEditor);
            var avatar = CreateRoundedObject(profile.transform, "Pilot Avatar", new Vector2(16f, -18f), new Vector2(82f, 82f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.03f, 0.34f, 0.49f, 1f), new Color(0.01f, 0.14f, 0.23f, 1f), 18f, new Color(0.30f, 0.86f, 1f, 0.72f), 2f);
            _pilotAvatar = CreatePilotAvatar(avatar.transform, "Active Pilot Avatar", 0, Vector2.zero, new Vector2(74f, 74f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            CreateText(profile.transform, "Pilot Label", 12, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(118f, -18f), new Vector2(180f, 24f), "PILOTO").color = new Color(0.46f, 0.80f, 0.96f, 1f);
            _pilotNameText = CreateText(profile.transform, "Pilot Name", 23, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(118f, -47f), new Vector2(300f, 36f), PilotProfileService.PilotName);
            CreateText(profile.transform, "Edit Profile Hint", 11, FontStyle.Bold, TextAnchor.MiddleRight, new Vector2(318f, -82f), new Vector2(120f, 24f), "EDITAR PERFIL  >").color = new Color(0.32f, 0.82f, 1f, 1f);

            var activeBadge = CreateRoundedObject(profile.transform, "Pilot Active Badge", new Vector2(350f, -18f), new Vector2(90f, 24f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.10f, 0.48f, 0.32f, 0.96f), new Color(0.03f, 0.23f, 0.16f, 0.98f), 12f, new Color(0.26f, 0.92f, 0.60f, 0.50f), 1f);
            CreateText(activeBadge.transform, "Pilot Active Badge Label", 10, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(90f, 24f), "ACTIVO", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).color = new Color(0.72f, 1f, 0.84f, 1f);
            profile.AddComponent<MenuButtonMotion>().Configure(profileBackground, null, null, _pilotNameText, profileTop, profileBottom, new Color(0.04f, 0.24f, 0.34f, 1f), new Color(0.01f, 0.10f, 0.16f, 1f), _textColor, Color.white, 0.24f);
            return panel;
        }

        private GameObject BuildLevelSelectPanel(Transform parent)
        {
            var shade = CreateStretchImage(parent, "Level Select Panel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.005f, 0.025f, 0.045f, 0.98f));
            shade.raycastTarget = true;
            shade.gameObject.AddComponent<MenuPanelTransition>().Configure(new Vector2(0f, 22f));
            _levelSelectionView = shade.gameObject.AddComponent<LevelSelectionView>();
            ConfigureLevelSelectionTextures();
            _levelSelectionView.Initialize(_displayFont, _bodyFont, LoadLevel, ShowMain);
            return shade.gameObject;
        }

        private void ConfigureLevelSelectionTextures()
        {
            _levelSelectionView?.ConfigureTextures(selectorBackground, beachBackground, cityBackground, desertBackground, forestBackground, cessnaArtwork, boeingArtwork, tomcatArtwork);
        }

        private GameObject BuildOptionsPanel(Transform parent)
        {
            var panel = CreateFixedPanel(parent, "Options Panel", Vector2.zero, new Vector2(1160f, 820f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), _panelColor);
            panel.AddComponent<MenuPanelTransition>().Configure(new Vector2(0f, 34f));
            AddPanelShadow(panel);
            CreateIcon(panel.transform, "Options Header Icon", MenuIconType.Settings, new Vector2(58f, -48f), new Vector2(48f, 48f), new Vector2(0f, 1f), new Vector2(0f, 1f), _accentColor);
            CreateText(panel.transform, "Options Title", 42, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(124f, -40f), new Vector2(500f, 58f), "OPCIONES");
            CreateText(panel.transform, "Options Subtitle", 14, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(126f, -96f), new Vector2(600f, 28f), "MENÚ  /  CONFIGURACIÓN DE EXPERIENCIA").color = _mutedTextColor;
            CreateLocalImage(panel.transform, "Header Line", new Vector2(58f, -144f), new Vector2(1004f, 3f), new Color(_accentColor.r, _accentColor.g, _accentColor.b, 0.55f));

            var soundCard = CreateFixedPanel(panel.transform, "Sound Card", new Vector2(58f, -180f), new Vector2(488f, 440f), new Vector2(0f, 1f), new Vector2(0f, 1f), _secondaryPanelColor);
            CreateIcon(soundCard.transform, "Sound Icon", MenuIconType.Sound, new Vector2(32f, -30f), new Vector2(38f, 38f), new Vector2(0f, 1f), new Vector2(0f, 1f), _accentColor);
            CreateText(soundCard.transform, "Sound Title", 23, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(86f, -28f), new Vector2(300f, 38f), "SONIDO");
            CreateText(soundCard.transform, "Sound Caption", 12, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(88f, -64f), new Vector2(320f, 24f), "SALIDA GENERAL DEL SIMULADOR").color = _mutedTextColor;
            CreateText(soundCard.transform, "Volume Label", 14, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(32f, -124f), new Vector2(260f, 28f), "VOLUMEN DE EFECTOS");
            _volumeValue = CreateText(soundCard.transform, "Volume Value", 18, FontStyle.Bold, TextAnchor.MiddleRight, new Vector2(354f, -122f), new Vector2(92f, 30f), "100%");
            _volumeSlider = CreateSlider(soundCard.transform, "Master Volume Slider", new Vector2(32f, -174f), new Vector2(414f, 36f), OnVolumeChanged);
            CreateText(soundCard.transform, "Music Volume Label", 14, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(32f, -224f), new Vector2(260f, 28f), "VOLUMEN DE MÚSICA");
            _musicVolumeValue = CreateText(soundCard.transform, "Music Volume Value", 18, FontStyle.Bold, TextAnchor.MiddleRight, new Vector2(354f, -222f), new Vector2(92f, 30f), "100%");
            _musicVolumeSlider = CreateSlider(soundCard.transform, "Music Volume Slider", new Vector2(32f, -274f), new Vector2(414f, 36f), OnMusicVolumeChanged);
            CreateText(soundCard.transform, "Sound Hint", 12, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(32f, -338f), new Vector2(410f, 28f), "LOS CAMBIOS SE GUARDAN AUTOMÁTICAMENTE.").color = _mutedTextColor;

            var controlsCard = CreateFixedPanel(panel.transform, "Controls Card", new Vector2(614f, -180f), new Vector2(488f, 440f), new Vector2(0f, 1f), new Vector2(0f, 1f), _secondaryPanelColor);
            CreateIcon(controlsCard.transform, "Controls Icon", MenuIconType.Controls, new Vector2(32f, -30f), new Vector2(38f, 38f), new Vector2(0f, 1f), new Vector2(0f, 1f), _accentColor);
            CreateText(controlsCard.transform, "Controls Title", 23, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(86f, -28f), new Vector2(320f, 38f), "CONTROLES");
            CreateText(controlsCard.transform, "Controls Caption", 12, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(88f, -64f), new Vector2(320f, 24f), "TECLADO  /  GAMEPAD").color = _mutedTextColor;
            CreateControlRows(controlsCard.transform);

            CreateButton(panel.transform, "RESTAURAR PREDETERMINADOS", new Vector2(58f, -680f), new Vector2(390f, 66f), RestoreDefaults, false, MenuIconType.Settings, 0.05f);
            CreateButton(panel.transform, "VOLVER AL MENÚ", new Vector2(712f, -680f), new Vector2(390f, 66f), ShowMain, true, MenuIconType.Back, 0.09f);
            RefreshSoundControls();
            return panel;
        }

        private GameObject BuildCreditsPanel(Transform parent)
        {
            var panel = CreateFixedPanel(parent, "Credits Panel", Vector2.zero, new Vector2(1700f, 940f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), _panelColor);
            panel.AddComponent<MenuPanelTransition>().Configure(new Vector2(0f, 34f));
            AddPanelShadow(panel);
            panel.AddComponent<MenuCreditsView>().Initialize(_displayFont, _bodyFont, _textColor, _accentColor, ShowMain);
            return panel;
        }

        private GameObject BuildExitPanel(Transform parent)
        {
            var shade = CreateStretchImage(parent, "Exit Screen Shade", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0f, 0.015f, 0.03f, 0.72f));
            var panel = CreateFixedPanel(shade.transform, "Exit Confirmation Panel", Vector2.zero, new Vector2(620f, 330f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), _panelColor);
            shade.gameObject.AddComponent<MenuPanelTransition>().Configure(new Vector2(0f, 18f));
            AddPanelShadow(panel);
            CreateIcon(panel.transform, "Exit Icon", MenuIconType.Exit, new Vector2(278f, -34f), new Vector2(64f, 64f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(1f, 0.46f, 0.40f, 1f));
            CreateText(panel.transform, "Exit Title", 30, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(40f, -108f), new Vector2(540f, 44f), "SALIR DEL JUEGO");
            CreateText(panel.transform, "Exit Prompt", 14, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(40f, -158f), new Vector2(540f, 32f), "¿QUIERES CERRAR AEROBYTE?").color = _mutedTextColor;
            CreateButton(panel.transform, "CANCELAR", new Vector2(40f, -234f), new Vector2(250f, 62f), ShowMain, false, MenuIconType.Back, 0.03f);
            CreateButton(panel.transform, "SALIR", new Vector2(330f, -234f), new Vector2(250f, 62f), Quit, true, MenuIconType.Exit, 0.07f);
            return shade.gameObject;
        }

        private GameObject BuildProfileEditorPanel(Transform parent)
        {
            var shade = CreateStretchImage(parent, "Profile Editor Shade", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0f, 0.015f, 0.03f, 0.76f));
            shade.gameObject.AddComponent<MenuPanelTransition>().Configure(new Vector2(0f, 24f));
            var card = CreateFixedPanel(shade.transform, "Profile Editor Card", Vector2.zero, new Vector2(1180f, 840f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), _panelColor);
            AddPanelShadow(card);
            _profileEditorView = card.AddComponent<PilotProfileEditorView>();
            _profileEditorView.Initialize(_displayFont, _bodyFont, _textColor, _accentColor, OnProfileSaved, ShowMain);
            return shade.gameObject;
        }

        private LoadingScreenView BuildLoadingScreen(Transform parent)
        {
            var loadingObject = new GameObject("Loading Screen", typeof(RectTransform), typeof(CanvasGroup), typeof(LoadingScreenView));
            loadingObject.transform.SetParent(parent, false);
            SetStretchRect(loadingObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var loadingView = loadingObject.GetComponent<LoadingScreenView>();
            loadingView.Initialize(_displayFont, _bodyFont);
            return loadingView;
        }

        private StartupScreenView BuildStartupScreen(Transform parent)
        {
            var startupObject = new GameObject("Startup Screen", typeof(RectTransform), typeof(CanvasGroup), typeof(StartupScreenView));
            startupObject.transform.SetParent(parent, false);
            SetStretchRect(startupObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var startupView = startupObject.GetComponent<StartupScreenView>();
            startupView.Initialize(_displayFont, startupBackground);
            return startupView;
        }

        private void CreateControlRow(Transform parent, string key, string action, float y)
        {
            CreateRoundedObject(parent, $"{key} Key", new Vector2(32f, y), new Vector2(148f, 36f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.03f, 0.18f, 0.28f, 1f), new Color(0.04f, 0.31f, 0.44f, 1f), 8f, new Color(0.20f, 0.68f, 0.92f, 0.28f), 1f);
            CreateText(parent, $"{key} Key Text", 13, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(32f, y), new Vector2(148f, 36f), key);
            CreateText(parent, $"{action} Action", 13, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(202f, y), new Vector2(230f, 36f), action).color = _mutedTextColor;
        }

        private void OnMenuCancel(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed || _loadingScreen != null && _loadingScreen.IsLoading) return;

            if (_levelSelectPanel != null && _levelSelectPanel.activeSelf && _levelSelectionView != null)
            {
                _levelSelectionView.HandleCancel();
                return;
            }

            if (_mainPanel != null && !_mainPanel.activeSelf) ShowMain();
        }

        private void CreateControlRows(Transform parent)
        {
            CreateControlRow(parent, "W / S  ·  A / D", "STICK IZQ.: VUELO", -112f);
            CreateControlRow(parent, "Q / E  ·  SHIFT / CTRL", "HOMBROS / GATILLOS", -164f);
            CreateControlRow(parent, "R  ·  K", "A: FLAPS  /  B: TREN", -216f);
            CreateControlRow(parent, "ESPACIO  ·  RMB / MMB", "X / STICKS: CÁMARA", -268f);
            CreateControlRow(parent, "M  ·  U / J", "DPAD ABAJO / ARRIBA-DER.", -320f);
            CreateControlRow(parent, "G + RUEDA  ·  ESC / P", "MENÚ: STICK IZQ. + A", -372f);
        }

        private void EnsureOptionsRuntimeLayout(Transform canvasRoot)
        {
            var soundCard = FindDescendant(canvasRoot, "Sound Card");
            if (soundCard != null && FindDescendant(soundCard, "Music Volume Slider") == null)
            {
                var effectsLabel = FindDescendant(soundCard, "Volume Label")?.GetComponent<Text>();
                if (effectsLabel != null) effectsLabel.text = "VOLUMEN DE EFECTOS";
                CreateText(soundCard, "Music Volume Label", 14, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(32f, -270f), new Vector2(260f, 28f), "VOLUMEN DE MÚSICA");
                CreateText(soundCard, "Music Volume Value", 18, FontStyle.Bold, TextAnchor.MiddleRight, new Vector2(354f, -268f), new Vector2(92f, 30f), "100%");
                CreateSlider(soundCard, "Music Volume Slider", new Vector2(32f, -316f), new Vector2(414f, 36f), OnMusicVolumeChanged);
            }

            if (soundCard != null)
            {
                SetRectIfPresent(FindDescendant(soundCard, "Volume Label"), new Vector2(32f, -124f), new Vector2(260f, 28f));
                SetRectIfPresent(FindDescendant(soundCard, "Volume Value"), new Vector2(354f, -122f), new Vector2(92f, 30f));
                SetRectIfPresent(FindDescendant(soundCard, "Master Volume Slider"), new Vector2(32f, -174f), new Vector2(414f, 36f));
                SetRectIfPresent(FindDescendant(soundCard, "Music Volume Label"), new Vector2(32f, -224f), new Vector2(260f, 28f));
                SetRectIfPresent(FindDescendant(soundCard, "Music Volume Value"), new Vector2(354f, -222f), new Vector2(92f, 30f));
                SetRectIfPresent(FindDescendant(soundCard, "Music Volume Slider"), new Vector2(32f, -274f), new Vector2(414f, 36f));
                SetRectIfPresent(FindDescendant(soundCard, "Sound Hint"), new Vector2(32f, -338f), new Vector2(410f, 28f));
                var muteButton = FindDescendant(soundCard, "SILENCIAR Button");
                if (muteButton != null) muteButton.gameObject.SetActive(false);
            }

            var controlsCard = FindDescendant(canvasRoot, "Controls Card");
            if (controlsCard == null) return;
            string[] keys = { "W / S", "A / D", "Q / E", "SHIFT / CTRL", "R", "ESPACIO / L" };
            string[] updatedKeys = { "W / S  ·  A / D", "Q / E  ·  SHIFT / CTRL", "R  ·  K", "ESPACIO  ·  RMB / MMB", "M  ·  U / J", "G + RUEDA  ·  ESC / P" };
            string[] actions = { "STICK IZQ.: VUELO", "HOMBROS / GATILLOS", "A: FLAPS  /  B: TREN", "X / STICKS: CÁMARA", "DPAD ABAJO / ARRIBA-DER.", "MENÚ: STICK IZQ. + A" };
            for (int i = 0; i < keys.Length; i++)
            {
                var keyText = FindDescendant(controlsCard, $"{keys[i]} Key Text")?.GetComponent<Text>();
                if (keyText != null) keyText.text = updatedKeys[i];
                var actionText = FindDescendant(controlsCard, $"{new[] { "INCLINACIÓN", "ALABEO", "GUIÑADA", "ACELERADOR", "FLAPS", "CÁMARA / LUCES" }[i]} Action")?.GetComponent<Text>();
                if (actionText != null) actionText.text = actions[i];
            }
        }

        private void OnVolumeChanged(float value)
        {
            MenuSettingsService.SetEffectsVolume(value);
            RefreshSoundControls();
        }

        private void OnMusicVolumeChanged(float value)
        {
            MenuSettingsService.SetMusicVolume(value);
            RefreshSoundControls();
        }

        private static void EnsureSliderInput(Slider slider)
        {
            if (slider == null) return;
            if (slider.GetComponent<CanvasRenderer>() == null) slider.gameObject.AddComponent<CanvasRenderer>();
            var hitArea = slider.GetComponent<Image>() ?? slider.gameObject.AddComponent<Image>();
            hitArea.color = Color.clear;
            hitArea.raycastTarget = true;
            slider.interactable = true;
            if (slider.GetComponent<MenuSliderFocus>() == null) slider.gameObject.AddComponent<MenuSliderFocus>();

            foreach (var image in slider.GetComponentsInChildren<Image>(true))
            {
                if (image.gameObject != slider.gameObject && image.gameObject.name != "Handle") image.raycastTarget = false;
            }
        }

        private void RefreshSoundControls()
        {
            if (_volumeSlider == null || _musicVolumeSlider == null) return;
            _volumeSlider.SetValueWithoutNotify(MenuSettingsService.EffectsVolume);
            _volumeValue.text = $"{Mathf.RoundToInt(MenuSettingsService.EffectsVolume * 100f)}%";
            _musicVolumeSlider.SetValueWithoutNotify(MenuSettingsService.MusicVolume);
            _musicVolumeValue.text = $"{Mathf.RoundToInt(MenuSettingsService.MusicVolume * 100f)}%";
            if (_muteLabel != null) _muteLabel.text = MenuSettingsService.IsMuted ? "ACTIVAR SONIDO" : "SILENCIAR";
            if (_muteIcon != null) _muteIcon.IconType = MenuSettingsService.IsMuted ? MenuIconType.Sound : MenuIconType.Mute;
        }

        private void OnProfileSaved()
        {
            RefreshPilotProfile();
            ShowMain();
        }

        private void RefreshPilotProfile()
        {
            if (_pilotNameText != null) _pilotNameText.text = PilotProfileService.PilotName;
            if (_pilotAvatar != null) _pilotAvatar.AvatarId = PilotProfileService.AvatarId;
        }

        private void ShowPanel(GameObject panel, bool immediate)
        {
            if (_mainPanel == null || _levelSelectPanel == null || _optionsPanel == null || _creditsPanel == null || _exitPanel == null || _profileEditorPanel == null || panel == null) return;

            bool useImmediate = immediate || !Application.isPlaying || !_panelsInitialized;
            HidePanelUnlessTarget(_mainPanel, panel, useImmediate);
            HidePanelUnlessTarget(_levelSelectPanel, panel, useImmediate);
            HidePanelUnlessTarget(_optionsPanel, panel, useImmediate);
            HidePanelUnlessTarget(_creditsPanel, panel, useImmediate);
            HidePanelUnlessTarget(_exitPanel, panel, useImmediate);
            HidePanelUnlessTarget(_profileEditorPanel, panel, useImmediate);
            SetPanelVisible(panel, true, useImmediate);
            if (Application.isPlaying) StartCoroutine(SelectFirstButtonWhenReady(panel));
            _panelsInitialized = true;
        }

        private static IEnumerator SelectWhenInputReady(GameObject target)
        {
            yield return null;
            if (target != null && target.activeInHierarchy && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(target);
            }
        }

        private static IEnumerator SelectFirstButtonWhenReady(GameObject panel)
        {
            yield return null;
            SelectFirstButton(panel);
        }

        private static void SelectFirstButton(GameObject panel)
        {
            if (!Application.isPlaying || EventSystem.current == null || panel == null) return;

            foreach (var button in panel.GetComponentsInChildren<Button>(true))
            {
                if (!button.isActiveAndEnabled || !button.interactable) continue;
                EventSystem.current.SetSelectedGameObject(button.gameObject);
                return;
            }
        }

        private void SetInitialPanelState()
        {
            if (_mainPanel == null || _levelSelectPanel == null || _optionsPanel == null || _creditsPanel == null || _exitPanel == null || _profileEditorPanel == null || _loadingScreen == null || _startupScreen == null) return;
            SetPanelImmediate(_mainPanel, true);
            SetPanelImmediate(_levelSelectPanel, false);
            SetPanelImmediate(_optionsPanel, false);
            SetPanelImmediate(_creditsPanel, false);
            SetPanelImmediate(_exitPanel, false);
            SetPanelImmediate(_profileEditorPanel, false);
            _loadingScreen.SetImmediate(false);
            if (Application.isPlaying) _startupScreen.ShowOnce();
            else _startupScreen.SetImmediate(false);
            _panelsInitialized = true;
        }

        private static void HidePanelUnlessTarget(GameObject candidate, GameObject target, bool immediate)
        {
            if (candidate == target) return;
            SetPanelVisible(candidate, false, immediate);
        }

        private static void SetPanelImmediate(GameObject panel, bool visible)
        {
            var transition = panel.GetComponent<MenuPanelTransition>();
            if (transition != null) transition.SetImmediate(visible);
            else panel.SetActive(visible);
        }

        private static void SetPanelVisible(GameObject panel, bool visible, bool immediate)
        {
            var transition = panel.GetComponent<MenuPanelTransition>();
            if (transition == null)
            {
                panel.SetActive(visible);
                return;
            }

            if (visible) transition.Show(immediate);
            else transition.Hide(immediate);
        }

        private GameObject CreateFixedPanel(Transform parent, string objectName, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot, Color color)
        {
            var bottom = new Color(color.r * 0.72f, color.g * 0.82f, color.b * 0.90f, color.a);
            return CreateRoundedObject(parent, objectName, position, size, anchor, pivot, color, bottom, 26f, new Color(0.28f, 0.72f, 0.95f, color.a > 0f ? 0.18f : 0f), color.a > 0f ? 1f : 0f);
        }

        private Button CreateButton(Transform parent, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action, bool primary, MenuIconType iconType, float introDelay)
        {
            bool isMainNavigation = parent.name == "Main Panel";
            Color normalTop = primary ? new Color(0.05f, 0.55f, 0.98f, 0.96f) : new Color(0.018f, 0.11f, 0.18f, 0.90f);
            Color normalBottom = primary ? new Color(0.02f, 0.31f, 0.70f, 0.98f) : new Color(0.012f, 0.065f, 0.11f, 0.94f);
            Color hoverTop = primary ? new Color(0.10f, 0.68f, 1f, 1f) : new Color(0.04f, 0.27f, 0.40f, 0.98f);
            Color hoverBottom = primary ? new Color(0.03f, 0.43f, 0.84f, 1f) : new Color(0.02f, 0.14f, 0.23f, 0.98f);
            Color normalText = primary ? Color.white : new Color(0.79f, 0.88f, 0.93f, 1f);
            Color hoverText = Color.white;

            var buttonObject = new GameObject($"{label} Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(CanvasGroup), typeof(MenuRoundedGraphic), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            SetRect(buttonObject.GetComponent<RectTransform>(), position, size, new Vector2(0f, 1f), new Vector2(0f, 1f));
            var background = buttonObject.GetComponent<MenuRoundedGraphic>();
            background.SetStyle(normalTop, normalBottom, 16f, new Color(0.32f, 0.78f, 1f, primary ? 0.42f : 0.18f), 1f);

            var button = buttonObject.GetComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(action);

            var accent = CreateRoundedObject(buttonObject.transform, "Active Accent", new Vector2(0f, 0f), new Vector2(5f, size.y - 22f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), _accentColor, _accentColor, 3f, Color.clear, 0f).GetComponent<MenuRoundedGraphic>();
            float iconSize = isMainNavigation ? 42f : 34f;
            float labelOffset = isMainNavigation ? 94f : 82f;
            int labelSize = isMainNavigation ? (primary ? 23 : 21) : (primary ? 19 : 17);
            var icon = CreateIcon(buttonObject.transform, $"{label} Icon", iconType, new Vector2(28f, 0f), new Vector2(iconSize, iconSize), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), normalText);
            var labelText = CreateText(buttonObject.transform, $"{label} Label", labelSize, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(labelOffset, 0f), new Vector2(size.x - labelOffset - 60f, size.y), label, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            labelText.raycastTarget = false;

            var chevron = CreateIcon(buttonObject.transform, "Chevron", MenuIconType.Back, new Vector2(-26f, 0f), new Vector2(18f, 18f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Color(normalText.r, normalText.g, normalText.b, 0.55f));
            chevron.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);

            var motion = buttonObject.AddComponent<MenuButtonMotion>();
            motion.Configure(background, icon, accent, labelText, normalTop, normalBottom, hoverTop, hoverBottom, normalText, hoverText, introDelay);
            return button;
        }

        private Slider CreateSlider(Transform parent, string objectName, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction<float> onChanged)
        {
            var root = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Slider));
            root.transform.SetParent(parent, false);
            SetRect(root.GetComponent<RectTransform>(), position, size, new Vector2(0f, 1f), new Vector2(0f, 1f));
            root.GetComponent<Image>().color = Color.clear;

            var background = CreateStretchImage(root.transform, "Background", Vector2.zero, Vector2.one, new Vector2(0f, 11f), new Vector2(0f, -11f), new Color(0f, 0.025f, 0.05f, 0.75f));
            background.raycastTarget = false;
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(root.transform, false);
            SetStretchRect(fillArea.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(8f, 11f), new Vector2(-8f, -11f));
            var fill = CreateStretchImage(fillArea.transform, "Fill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, _accentColor);
            fill.raycastTarget = false;

            var handleArea = new GameObject("Handle Area", typeof(RectTransform));
            handleArea.transform.SetParent(root.transform, false);
            SetStretchRect(handleArea.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(10f, 0f), new Vector2(-10f, 0f));
            var handle = CreateLocalImage(handleArea.transform, "Handle", Vector2.zero, new Vector2(24f, 34f), _textColor, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f));

            var slider = root.GetComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            EnsureSliderInput(slider);
            slider.onValueChanged.AddListener(onChanged);
            return slider;
        }

        private GameObject CreateRoundedObject(Transform parent, string objectName, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot, Color top, Color bottom, float radius, Color border, float borderWidth)
        {
            var roundedObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuRoundedGraphic));
            roundedObject.transform.SetParent(parent, false);
            SetRect(roundedObject.GetComponent<RectTransform>(), position, size, anchor, pivot);
            roundedObject.GetComponent<MenuRoundedGraphic>().SetStyle(top, bottom, radius, border, borderWidth);
            return roundedObject;
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

        private PilotAvatarGraphic CreatePilotAvatar(Transform parent, string objectName, int avatarId, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot)
        {
            var avatarObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(PilotAvatarGraphic));
            avatarObject.transform.SetParent(parent, false);
            SetRect(avatarObject.GetComponent<RectTransform>(), position, size, anchor, pivot);
            var avatar = avatarObject.GetComponent<PilotAvatarGraphic>();
            avatar.AvatarId = avatarId;
            avatar.raycastTarget = false;
            return avatar;
        }

        private static void AddPanelShadow(GameObject panel)
        {
            var shadow = panel.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0.015f, 0.03f, 0.64f);
            shadow.effectDistance = new Vector2(0f, -12f);
            shadow.useGraphicAlpha = true;
        }

        private Image CreateLocalImage(Transform parent, string objectName, Vector2 position, Vector2 size, Color color, Vector2? anchor = null, Vector2? pivot = null)
        {
            var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            SetRect(imageObject.GetComponent<RectTransform>(), position, size, anchor ?? new Vector2(0f, 1f), pivot ?? new Vector2(0f, 1f));
            var image = imageObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private Image CreateStretchImage(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            SetStretchRect(imageObject.GetComponent<RectTransform>(), anchorMin, anchorMax, offsetMin, offsetMax);
            var image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static RawImage CreateStretchRawImage(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(RawImage));
            imageObject.transform.SetParent(parent, false);
            SetStretchRect(imageObject.GetComponent<RectTransform>(), anchorMin, anchorMax, offsetMin, offsetMax);
            var image = imageObject.GetComponent<RawImage>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private Text CreateText(Transform parent, string objectName, int fontSize, FontStyle style, TextAnchor alignment, Vector2 position, Vector2 size, string value, Vector2? anchor = null, Vector2? pivot = null, Vector2? textPivot = null)
        {
            var textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            SetRect(textObject.GetComponent<RectTransform>(), position, size, anchor ?? new Vector2(0f, 1f), textPivot ?? pivot ?? new Vector2(0f, 1f));
            var text = textObject.GetComponent<Text>();
            text.font = style == FontStyle.Bold ? _displayFont : _bodyFont;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = _textColor;
            text.text = value;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static void SetRect(RectTransform rect, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void SetRectIfPresent(Transform transform, Vector2 position, Vector2 size)
        {
            if (transform != null) SetRect(transform.GetComponent<RectTransform>(), position, size, new Vector2(0f, 1f), new Vector2(0f, 1f));
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
