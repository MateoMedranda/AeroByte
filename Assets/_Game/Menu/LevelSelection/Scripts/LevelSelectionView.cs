using System;
using System.Collections;
using AeroByte.Menu.Audio;
using AeroByte.Menu.Profile;
using AeroByte.Menu.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AeroByte.Menu.LevelSelection
{
    public sealed class LevelSelectionView : MonoBehaviour
    {
        private const string BackgroundPath = "Assets/_Game/Menu/Art/Backgrounds/LEVEL SELECTOR/BG- LEVEL SELECTOR.png";
        private const string BeachPath = "Assets/_Game/Menu/Art/Backgrounds/LEVEL SELECTOR/PLAYA/BGE-PLAYA.png";
        private const string CityPath = "Assets/_Game/Menu/Art/Backgrounds/LEVEL SELECTOR/CIUDAD/BGE-CIUDAD.png";
        private const string DesertPath = "Assets/_Game/Menu/Art/Backgrounds/LEVEL SELECTOR/DESIERTO/BGE-DESIERTO.png";
        private const string ForestPath = "Assets/_Game/Menu/Art/Backgrounds/LEVEL SELECTOR/BOSQUE/BGE-BOSQUE.png";
        private const string CessnaPath = "Assets/_Game/Menu/Missions/A-CESSNA.png";
        private const string BoeingPath = "Assets/_Game/Menu/Missions/A-BOEING.png";
        private const string TomcatPath = "Assets/_Game/Menu/Missions/A-F14TOMCAT.png";

        [SerializeField] private Texture2D selectorBackground;
        [SerializeField] private Texture2D beachBackground;
        [SerializeField] private Texture2D cityBackground;
        [SerializeField] private Texture2D desertBackground;
        [SerializeField] private Texture2D forestBackground;
        [SerializeField] private Texture2D cessnaArtwork;
        [SerializeField] private Texture2D boeingArtwork;
        [SerializeField] private Texture2D tomcatArtwork;

        private Font _displayFont;
        private Font _bodyFont;
        private Action<string> _onLevelSelected;
        private Action _onBack;
        [SerializeField] private PilotAvatarGraphic pilotAvatar;
        [SerializeField] private Text pilotName;
        private GameObject _selectionPage;
        private GameObject _detailPage;
        private RawImage _detailBackground;
        private RawImage[] _detailBlurLayers;
        private RawImage _detailEnvironment;
        private RawImage _detailAircraft;
        private Text _detailEyebrow;
        private Text _detailTitle;
        private Text _detailDifficulty;
        private Text _detailAircraftName;
        private Text _detailObjective;
        private Text _detailHazards;
        private MenuIconGraphic _detailHazardIcon;
        private MenuIconGraphic _missionBadgeIcon;
        private string _selectedScene;

        private GameObject _colorCustomizationPanel;
        private Image _colorPreviewSwatch;
        private Text _colorPreviewHexText;
        private Text _customColorStatusText;
        private Slider _rSlider;
        private Slider _gSlider;
        private Slider _bSlider;
        private bool _suppressColorSliderCallback;

        private void OnEnable()
        {
            RefreshPilotProfile();
        }

        public void ConfigureTextures(Texture2D selector, Texture2D beach, Texture2D city, Texture2D desert, Texture2D forest, Texture2D cessna, Texture2D boeing, Texture2D tomcat)
        {
            selectorBackground = selector;
            beachBackground = beach;
            cityBackground = city;
            desertBackground = desert;
            forestBackground = forest;
            cessnaArtwork = cessna;
            boeingArtwork = boeing;
            tomcatArtwork = tomcat;
        }

        public void Initialize(Font displayFont, Font bodyFont, Action<string> onLevelSelected, Action onBack)
        {
            _displayFont = displayFont;
            _bodyFont = bodyFont;
            _onLevelSelected = onLevelSelected;
            _onBack = onBack;

#if UNITY_EDITOR
            ResolveTextures();
#endif

            if (transform.Find("Level Selection Content") == null) Build();
            Bind(onLevelSelected, onBack);
        }

        public void Bind(Action<string> onLevelSelected, Action onBack)
        {
            _onLevelSelected = onLevelSelected;
            _onBack = onBack;

            CachePageReferences();
            EnsureColorCustomizerUIExists();
            foreach (var card in GetComponentsInChildren<LevelSelectionCard>(true)) card.Bind(ShowMissionDetail);
            var backButton = FindDescendant(transform, "Level Selection Back Button")?.GetComponent<Button>();
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                if (_onBack != null) backButton.onClick.AddListener(() => _onBack());
            }

            BindDetailButton("Mission Detail Back Button", ShowSelection);
            BindDetailButton("Mission Detail Start Button", StartSelectedMission);
            BindDetailButton("Mission Detail Customize Color Button", () => ShowColorCustomizer(true));
            BindDetailButton("Color Modal Close Button", () => ShowColorCustomizer(false));
            BindDetailButton("Reset Original Color Button", () => ResetToDefaultPlaneColor());
            BindDetailButton("Color Modal Apply Button", () => ShowColorCustomizer(false));
            ConfigureMissionDetailNavigation();
        }

        private void Build()
        {
            var content = CreateRect(transform, "Level Selection Content", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _selectionPage = CreateRect(content, "Level Selection Page", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
            var background = CreateRawImage(_selectionPage.transform, "Level Selector Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
            background.texture = selectorBackground;
            background.color = new Color(1f, 1f, 1f, 0.58f);
            CreateSelectorBlurLayer(_selectionPage.transform, "Selector Blur Left", selectorBackground, new Vector2(-8f, 0f));
            CreateSelectorBlurLayer(_selectionPage.transform, "Selector Blur Right", selectorBackground, new Vector2(8f, 0f));
            CreateSelectorBlurLayer(_selectionPage.transform, "Selector Blur Up", selectorBackground, new Vector2(0f, 8f));
            CreateSelectorBlurLayer(_selectionPage.transform, "Selector Blur Down", selectorBackground, new Vector2(0f, -8f));
            CreateSelectorBlurLayer(_selectionPage.transform, "Selector Blur Upper Left", selectorBackground, new Vector2(-6f, 6f));
            CreateSelectorBlurLayer(_selectionPage.transform, "Selector Blur Upper Right", selectorBackground, new Vector2(6f, 6f));
            CreateSelectorBlurLayer(_selectionPage.transform, "Selector Blur Lower Left", selectorBackground, new Vector2(-6f, -6f));
            CreateSelectorBlurLayer(_selectionPage.transform, "Selector Blur Lower Right", selectorBackground, new Vector2(6f, -6f));
            CreateImage(_selectionPage.transform, "Background Shade", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.005f, 0.025f, 0.045f, 0.34f));
            CreateImage(_selectionPage.transform, "Top Glow", new Vector2(0f, 0.76f), Vector2.one, Vector2.zero, Vector2.zero, new Color(0.02f, 0.20f, 0.34f, 0.38f));

            CreateText(_selectionPage.transform, "Level Selection Eyebrow", "AEROBYTE  /  CENTRO DE OPERACIONES", 18, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0f, -48f), new Vector2(900f, 32f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Color(0.28f, 0.76f, 1f, 1f));
            CreateText(_selectionPage.transform, "Level Selection Heading", "SELECCIONA TU DESTINO", 50, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0f, -84f), new Vector2(1100f, 68f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Color.white);
            CreatePilotSummary(_selectionPage.transform);
            CreateCard(_selectionPage.transform, 0, "COSTA", "Beach", "CESSNA 172", LevelTheme.Beach, beachBackground, 208f);
            CreateCard(_selectionPage.transform, 1, "CIUDAD", "Ciudad", "CESSNA 172", LevelTheme.City, cityBackground, 584f);
            CreateCard(_selectionPage.transform, 2, "DESIERTO", "Desert", "BOEING DE CARGA", LevelTheme.Desert, desertBackground, 960f);
            CreateCard(_selectionPage.transform, 3, "BOSQUE", "Forest", "F-14 TOMCAT", LevelTheme.Forest, forestBackground, 1336f);
            CreateBackButton(_selectionPage.transform);
            BuildMissionDetail(content);
            ShowSelection();
            RefreshPilotProfile();
        }

        public void ShowSelection()
        {
            MenuMusicController.PlayMain();
            CachePageReferences();
            if (_selectionPage != null) _selectionPage.SetActive(true);
            if (_detailPage != null) _detailPage.SetActive(false);
            if (_colorCustomizationPanel != null) _colorCustomizationPanel.SetActive(false);
            _selectedScene = null;
            StartCoroutine(SelectButtonWhenReady("COSTA Level Card", true));
        }

        public void HandleCancel()
        {
            if (_colorCustomizationPanel != null && _colorCustomizationPanel.activeSelf)
            {
                ShowColorCustomizer(false);
            }
            else if (_detailPage != null && _detailPage.activeSelf)
            {
                ShowSelection();
            }
            else
            {
                _onBack?.Invoke();
            }
        }

        private void ShowMissionDetail(string sceneName)
        {
            MenuMusicController.PlayLevelInfo();
            MissionDetails details = GetMissionDetails(sceneName);
            CachePageReferences();
            _selectedScene = sceneName;

            ApplyCover(_detailBackground, details.Environment);
            if (_detailBlurLayers != null)
            {
                foreach (var layer in _detailBlurLayers) ApplyCover(layer, details.Environment);
            }
            ApplyCover(_detailEnvironment, details.Environment);
            ApplyCover(_detailAircraft, details.AircraftArtwork);
            if (_detailEyebrow != null) _detailEyebrow.text = details.Eyebrow;
            if (_detailTitle != null) _detailTitle.text = details.Title;
            if (_detailDifficulty != null) _detailDifficulty.text = details.Difficulty;
            if (_detailAircraftName != null) _detailAircraftName.text = details.Aircraft;
            if (_detailObjective != null) _detailObjective.text = details.Objective;
            if (_detailHazards != null) _detailHazards.text = details.Hazards;
            if (_detailHazardIcon != null) _detailHazardIcon.IconType = details.HazardIcon;
            if (_missionBadgeIcon != null) _missionBadgeIcon.IconType = details.ObjectiveIcon;

            if (_selectionPage != null) _selectionPage.SetActive(false);
            if (_detailPage != null) _detailPage.SetActive(true);
            EnsureColorCustomizerUIExists();
            UpdateCustomColorStatusDisplay();
            StartCoroutine(SelectButtonWhenReady("Mission Detail Start Button", false));
        }

        private IEnumerator SelectButtonWhenReady(string buttonName, bool selectionPage)
        {
            yield return null;
            if ((selectionPage && (_selectionPage == null || !_selectionPage.activeInHierarchy)) ||
                (!selectionPage && (_detailPage == null || !_detailPage.activeInHierarchy)) ||
                EventSystem.current == null) yield break;

            var button = FindDescendant(transform, buttonName)?.GetComponent<Button>();
            if (button != null && button.isActiveAndEnabled && button.interactable)
            {
                EventSystem.current.SetSelectedGameObject(button.gameObject);
            }
        }

        private void ConfigureMissionDetailNavigation()
        {
            var customize = FindDescendant(transform, "Mission Detail Customize Color Button")?.GetComponent<Button>();
            var start = FindDescendant(transform, "Mission Detail Start Button")?.GetComponent<Button>();
            var back = FindDescendant(transform, "Mission Detail Back Button")?.GetComponent<Button>();
            if (customize == null || start == null || back == null) return;

            SetNavigation(customize, back, start, start, back);
            SetNavigation(start, customize, null, back, back);
            SetNavigation(back, customize, null, start, start);
        }

        private static void SetNavigation(Selectable selectable, Selectable up, Selectable down, Selectable left, Selectable right)
        {
            var navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = up,
                selectOnDown = down,
                selectOnLeft = left,
                selectOnRight = right
            };
            selectable.navigation = navigation;
        }

        private static void ApplyCover(RawImage image, Texture2D texture)
        {
            if (image == null || texture == null) return;

            image.texture = texture;
            float targetAspect = image.rectTransform.rect.width / image.rectTransform.rect.height;
            float textureAspect = texture.width / (float)texture.height;
            if (textureAspect > targetAspect)
            {
                float width = targetAspect / textureAspect;
                image.uvRect = new Rect((1f - width) * 0.5f, 0f, width, 1f);
            }
            else
            {
                float height = textureAspect / targetAspect;
                image.uvRect = new Rect(0f, (1f - height) * 0.5f, 1f, height);
            }
        }

        private static RawImage CreateBlurLayer(Transform parent, int index, Vector2 offset)
        {
            var layer = CreateRawImage(parent, $"Mission Background Blur {index}", Vector2.zero, Vector2.one, offset, offset, new Color(1f, 1f, 1f, 0.09f));
            layer.raycastTarget = false;
            return layer;
        }

        private static void CreateSelectorBlurLayer(Transform parent, string objectName, Texture2D texture, Vector2 offset)
        {
            var layer = CreateRawImage(parent, objectName, Vector2.zero, Vector2.one, offset, offset, new Color(1f, 1f, 1f, 0.075f));
            layer.texture = texture;
            layer.raycastTarget = false;
        }

        private void StartSelectedMission()
        {
            if (string.IsNullOrEmpty(_selectedScene)) return;
            MenuMusicController.FadeToSilence();
            _onLevelSelected?.Invoke(_selectedScene);
        }

        private MissionDetails GetMissionDetails(string sceneName)
        {
            return sceneName switch
            {
                "Beach" => new MissionDetails("MISIÓN  /  COSTA", "ENVÍO DE HELADOS DE SALCEDO", "<size=52>★☆☆☆☆</size>   <size=34>(FÁCIL)</size>", "CESSNA 172", "Aprende a despegar, navegar y aterrizar mientras entregas una carga ligera.", "Tormentas aisladas sobre el mar.", beachBackground, cessnaArtwork, MenuIconType.Storm, MenuIconType.IceCream),
                "Ciudad" => new MissionDetails("NIVEL  /  CIUDAD", "CIUDAD", "<size=52>★★☆☆☆</size>   <size=34>(MEDIA)</size>", "CESSNA 172", "Transporta animales entre edificios manteniendo una altitud precisa y evitando obstáculos urbanos.", "Ráfagas de viento y rascacielos.", cityBackground, cessnaArtwork, MenuIconType.UrbanWind, MenuIconType.Animal),
                "Desert" => new MissionDetails("NIVEL  /  DESIERTO", "DESIERTO", "<size=52>★★★☆☆</size>   <size=34>(MEDIA-ALTA)</size>", "BOEING DE CARGA", "Entrega suministros a bases remotas atravesando largas rutas comerciales.", "Tormentas de arena y baja visibilidad.", desertBackground, boeingArtwork, MenuIconType.Sandstorm, MenuIconType.Supplies),
                "Forest" => new MissionDetails("NIVEL  /  BOSQUE", "BOSQUE", "<size=52>★★★★★</size>   <size=34>(EXTREMA)</size>", "F-14 TOMCAT", "Completa una misión de combate a baja altitud destruyendo objetivos enemigos y esquivando obstáculos.", "Árboles gigantes, montañas e impactos enemigos.", forestBackground, tomcatArtwork, MenuIconType.Combat, MenuIconType.Combat),
                _ => default
            };
        }

        private void BuildMissionDetail(Transform parent)
        {
            _detailPage = CreateRect(parent, "Mission Detail Page", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
            _detailBackground = CreateRawImage(_detailPage.transform, "Mission Detail Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
            _detailBackground.color = new Color(1f, 1f, 1f, 0.48f);
            _detailBlurLayers = new[]
            {
                CreateBlurLayer(_detailPage.transform, 0, new Vector2(-10f, 0f)),
                CreateBlurLayer(_detailPage.transform, 1, new Vector2(10f, 0f)),
                CreateBlurLayer(_detailPage.transform, 2, new Vector2(0f, -10f)),
                CreateBlurLayer(_detailPage.transform, 3, new Vector2(0f, 10f)),
                CreateBlurLayer(_detailPage.transform, 4, new Vector2(-7f, -7f)),
                CreateBlurLayer(_detailPage.transform, 5, new Vector2(7f, 7f)),
                CreateBlurLayer(_detailPage.transform, 6, new Vector2(-7f, 7f)),
                CreateBlurLayer(_detailPage.transform, 7, new Vector2(7f, -7f))
            };
            CreateImage(_detailPage.transform, "Mission Detail Shade", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.002f, 0.025f, 0.045f, 0.76f));
            CreateImage(_detailPage.transform, "Mission Detail Top Glow", new Vector2(0f, 0.74f), Vector2.one, Vector2.zero, Vector2.zero, new Color(0.02f, 0.30f, 0.48f, 0.25f));

            _detailEyebrow = CreateText(_detailPage.transform, "Mission Detail Eyebrow", "MISIÓN", 22, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(82f, -32f), new Vector2(900f, 38f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.32f, 0.82f, 1f, 1f));
            _detailTitle = CreateText(_detailPage.transform, "Mission Detail Title", "DETALLES DE NIVEL", 62, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(80f, -68f), new Vector2(1500f, 82f), new Vector2(0f, 1f), new Vector2(0f, 1f), Color.white);
            CreateText(_detailPage.transform, "Mission Detail Number", "INFORMACIÓN DE VUELO", 20, FontStyle.Bold, TextAnchor.MiddleRight, new Vector2(-82f, -50f), new Vector2(400f, 40f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Color(0.62f, 0.80f, 0.90f, 1f));

            var overview = CreateRoundedPanel(_detailPage.transform, "Mission Overview Card", new Vector2(80f, -158f), new Vector2(1080f, 750f));
            _detailEnvironment = CreateRawImage(overview.transform, "Mission Environment", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Color.white);
            SetRect(_detailEnvironment.rectTransform, new Vector2(16f, -16f), new Vector2(1048f, 386f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            CreateImage(overview.transform, "Mission Environment Shade", new Vector2(0f, 0.47f), new Vector2(1f, 1f), new Vector2(16f, 0f), new Vector2(-16f, -16f), new Color(0f, 0.025f, 0.05f, 0.18f)).raycastTarget = false;
            var missionBadge = CreateRoundedPanel(overview.transform, "Mission Objective Badge", new Vector2(932f, -34f), new Vector2(104f, 104f), new Color(0.22f, 0.76f, 0.94f, 0.98f), new Color(0.03f, 0.30f, 0.48f, 0.98f));
            _missionBadgeIcon = CreateMissionIcon(missionBadge.transform, "Mission Objective Icon", MenuIconType.IceCream, Vector2.zero, new Vector2(72f, 72f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Color.white);

            var aircraftCard = CreateRoundedPanel(overview.transform, "Aircraft Card", new Vector2(16f, -420f), new Vector2(1048f, 306f), new Color(0.018f, 0.105f, 0.16f, 0.98f), new Color(0.006f, 0.04f, 0.07f, 1f));
            _detailAircraft = CreateRawImage(aircraftCard.transform, "Mission Aircraft Artwork", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Color.white);
            SetRect(_detailAircraft.rectTransform, new Vector2(14f, -14f), new Vector2(480f, 270f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            CreateText(aircraftCard.transform, "Aircraft Label", "AERONAVE ASIGNADA", 21, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(530f, -38f), new Vector2(470f, 38f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.35f, 0.78f, 0.96f, 1f));
            _detailAircraftName = CreateText(aircraftCard.transform, "Mission Aircraft Name", "AERONAVE", 52, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(528f, -84f), new Vector2(500f, 72f), new Vector2(0f, 1f), new Vector2(0f, 1f), Color.white);
            CreateText(aircraftCard.transform, "Aircraft Caption", "LISTA PARA LA MISIÓN", 21, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(530f, -168f), new Vector2(470f, 38f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.36f, 0.86f, 0.60f, 1f));
            _customColorStatusText = CreateText(aircraftCard.transform, "Aircraft Color Status", "COLOR: ORIGINAL (POR DEFECTO)", 18, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(530f, -204f), new Vector2(470f, 24f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.85f, 0.95f, 1f, 1f));
            CreateDetailButton(aircraftCard.transform, "Mission Detail Customize Color Button", "🎨  PERSONALIZAR COLOR", new Vector2(530f, -234f), new Vector2(440f, 52f), false);

            var briefing = CreateRoundedPanel(_detailPage.transform, "Mission Briefing Card", new Vector2(1190f, -158f), new Vector2(650f, 750f));
            CreateText(briefing.transform, "Briefing Heading", "DETALLES DEL NIVEL", 30, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(34f, -20f), new Vector2(570f, 50f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.86f, 0.94f, 0.98f, 1f));
            var briefingLine = CreateImage(briefing.transform, "Briefing Line", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Color(0.18f, 0.66f, 0.92f, 0.45f));
            SetRect(briefingLine.rectTransform, new Vector2(34f, -76f), new Vector2(582f, 2f), new Vector2(0f, 1f), new Vector2(0f, 1f));

            CreateInfoBlock(briefing.transform, "Difficulty", "DIFICULTAD", out _detailDifficulty, -96f, 126f, new Color(1f, 0.76f, 0.25f, 1f));
            CreateInfoBlock(briefing.transform, "Objective", "OBJETIVO", out _detailObjective, -242f, 222f, new Color(0.28f, 0.82f, 1f, 1f));
            CreateInfoBlock(briefing.transform, "Hazards", "PELIGROS", out _detailHazards, -484f, 218f, new Color(1f, 0.40f, 0.30f, 1f));

            CreateDetailButton(_detailPage.transform, "Mission Detail Back Button", "<  VOLVER A NIVELES", new Vector2(80f, -932f), new Vector2(330f, 76f), false);
            CreateDetailButton(_detailPage.transform, "Mission Detail Start Button", "INICIAR NIVEL  >", new Vector2(-80f, -932f), new Vector2(390f, 76f), true);
            BuildColorCustomizationPanel(_detailPage.transform);
        }

        private GameObject CreateRoundedPanel(Transform parent, string objectName, Vector2 position, Vector2 size, Color? top = null, Color? bottom = null)
        {
            var panel = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuRoundedGraphic));
            panel.transform.SetParent(parent, false);
            SetRect(panel.GetComponent<RectTransform>(), position, size, new Vector2(0f, 1f), new Vector2(0f, 1f));
            panel.GetComponent<MenuRoundedGraphic>().SetStyle(top ?? new Color(0.025f, 0.12f, 0.18f, 0.98f), bottom ?? new Color(0.006f, 0.04f, 0.07f, 1f), 22f, new Color(0.18f, 0.64f, 0.90f, 0.42f), 2f);
            return panel;
        }

        private void CreateInfoBlock(Transform parent, string objectName, string labelValue, out Text value, float y, float height, Color accent)
        {
            var block = CreateRoundedPanel(parent, $"{objectName} Block", new Vector2(28f, y), new Vector2(594f, height), new Color(0.03f, 0.15f, 0.21f, 0.96f), new Color(0.01f, 0.065f, 0.10f, 0.98f));
            CreateImage(block.transform, $"{objectName} Accent", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, accent).raycastTarget = false;
            SetRect(FindDescendant(block.transform, $"{objectName} Accent").GetComponent<RectTransform>(), new Vector2(16f, -18f), new Vector2(4f, height - 36f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            CreateText(block.transform, $"{objectName} Label", labelValue, 21, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(38f, -8f), new Vector2(520f, 34f), new Vector2(0f, 1f), new Vector2(0f, 1f), accent);
            bool hasDangerIcon = objectName == "Hazards";
            if (hasDangerIcon)
            {
                _detailHazardIcon = CreateMissionIcon(block.transform, "Mission Hazard Icon", MenuIconType.Storm, new Vector2(34f, -58f), new Vector2(70f, 70f), new Vector2(0f, 1f), new Vector2(0f, 1f), accent);
            }

            float valueX = hasDangerIcon ? 122f : 38f;
            float valueWidth = hasDangerIcon ? 436f : 520f;
            value = CreateText(block.transform, $"Mission {objectName} Value", string.Empty, objectName == "Difficulty" ? 34 : 30, FontStyle.Bold, TextAnchor.UpperLeft, new Vector2(valueX, -46f), new Vector2(valueWidth, height - 52f), new Vector2(0f, 1f), new Vector2(0f, 1f), Color.white);
            value.supportRichText = true;
            value.horizontalOverflow = HorizontalWrapMode.Wrap;
            value.verticalOverflow = VerticalWrapMode.Truncate;
            value.lineSpacing = 1.15f;
        }

        private void CreateDetailButton(Transform parent, string objectName, string label, Vector2 position, Vector2 size, bool primary)
        {
            var buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuRoundedGraphic), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            Vector2 anchor = primary ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            Vector2 pivot = anchor;
            SetRect(buttonObject.GetComponent<RectTransform>(), position, size, anchor, pivot);
            var background = buttonObject.GetComponent<MenuRoundedGraphic>();
            background.SetStyle(primary ? new Color(0.05f, 0.62f, 0.94f, 1f) : new Color(0.03f, 0.13f, 0.20f, 0.98f), primary ? new Color(0.015f, 0.31f, 0.55f, 1f) : new Color(0.01f, 0.06f, 0.10f, 1f), 14f, new Color(0.30f, 0.82f, 1f, 0.62f), 2f);
            var button = buttonObject.GetComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;
            var buttonLabel = CreateText(buttonObject.transform, $"{objectName} Label", label, 26, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, size, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Color.white);
            buttonLabel.raycastTarget = false;
            buttonObject.AddComponent<MenuButtonMotion>().Configure(
                background,
                null,
                null,
                buttonLabel,
                primary ? new Color(0.05f, 0.62f, 0.94f, 1f) : new Color(0.03f, 0.13f, 0.20f, 0.98f),
                primary ? new Color(0.015f, 0.31f, 0.55f, 1f) : new Color(0.01f, 0.06f, 0.10f, 1f),
                primary ? new Color(0.16f, 0.78f, 1f, 1f) : new Color(0.07f, 0.32f, 0.46f, 1f),
                primary ? new Color(0.04f, 0.46f, 0.82f, 1f) : new Color(0.02f, 0.17f, 0.28f, 1f),
                new Color(0.84f, 0.93f, 0.98f, 1f),
                Color.white,
                0f);
        }

        private static MenuIconGraphic CreateMissionIcon(Transform parent, string objectName, MenuIconType type, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot, Color color)
        {
            var iconObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuIconGraphic));
            iconObject.transform.SetParent(parent, false);
            SetRect(iconObject.GetComponent<RectTransform>(), position, size, anchor, pivot);
            var icon = iconObject.GetComponent<MenuIconGraphic>();
            icon.Configure(type, color, 3.2f);
            return icon;
        }

        private void CachePageReferences()
        {
            _selectionPage ??= FindDescendant(transform, "Level Selection Page")?.gameObject;
            _detailPage ??= FindDescendant(transform, "Mission Detail Page")?.gameObject;
            _detailBackground ??= FindDescendant(transform, "Mission Detail Background")?.GetComponent<RawImage>();
            if (_detailBlurLayers == null)
            {
                _detailBlurLayers = new RawImage[8];
                for (int i = 0; i < _detailBlurLayers.Length; i++)
                {
                    _detailBlurLayers[i] = FindDescendant(transform, $"Mission Background Blur {i}")?.GetComponent<RawImage>();
                }
            }
            _detailEnvironment ??= FindDescendant(transform, "Mission Environment")?.GetComponent<RawImage>();
            _detailAircraft ??= FindDescendant(transform, "Mission Aircraft Artwork")?.GetComponent<RawImage>();
            _detailEyebrow ??= FindDescendant(transform, "Mission Detail Eyebrow")?.GetComponent<Text>();
            _detailTitle ??= FindDescendant(transform, "Mission Detail Title")?.GetComponent<Text>();
            _detailDifficulty ??= FindDescendant(transform, "Mission Difficulty Value")?.GetComponent<Text>();
            _detailAircraftName ??= FindDescendant(transform, "Mission Aircraft Name")?.GetComponent<Text>();
            _detailObjective ??= FindDescendant(transform, "Mission Objective Value")?.GetComponent<Text>();
            _detailHazards ??= FindDescendant(transform, "Mission Hazards Value")?.GetComponent<Text>();
            _detailHazardIcon ??= FindDescendant(transform, "Mission Hazard Icon")?.GetComponent<MenuIconGraphic>();
            _missionBadgeIcon ??= FindDescendant(transform, "Mission Objective Icon")?.GetComponent<MenuIconGraphic>();
            _customColorStatusText ??= FindDescendant(transform, "Aircraft Color Status")?.GetComponent<Text>();
            _colorCustomizationPanel ??= FindDescendant(transform, "Color Customizer Panel")?.gameObject;
            _colorPreviewSwatch ??= FindDescendant(transform, "Color Preview Swatch")?.GetComponent<Image>();
            _colorPreviewHexText ??= FindDescendant(transform, "Color RGB Text")?.GetComponent<Text>();
            _rSlider ??= FindDescendant(transform, "R Slider Container")?.GetComponentInChildren<Slider>(true);
            _gSlider ??= FindDescendant(transform, "G Slider Container")?.GetComponentInChildren<Slider>(true);
            _bSlider ??= FindDescendant(transform, "B Slider Container")?.GetComponentInChildren<Slider>(true);
        }

        private void BindDetailButton(string objectName, Action action)
        {
            var button = FindDescendant(transform, objectName)?.GetComponent<Button>();
            if (button == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action());
        }

        private readonly struct MissionDetails
        {
            public MissionDetails(string eyebrow, string title, string difficulty, string aircraft, string objective, string hazards, Texture2D environment, Texture2D aircraftArtwork, MenuIconType hazardIcon, MenuIconType objectiveIcon)
            {
                Eyebrow = eyebrow;
                Title = title;
                Difficulty = difficulty;
                Aircraft = aircraft;
                Objective = objective;
                Hazards = hazards;
                Environment = environment;
                AircraftArtwork = aircraftArtwork;
                HazardIcon = hazardIcon;
                ObjectiveIcon = objectiveIcon;
            }

            public string Eyebrow { get; }
            public string Title { get; }
            public string Difficulty { get; }
            public string Aircraft { get; }
            public string Objective { get; }
            public string Hazards { get; }
            public Texture2D Environment { get; }
            public Texture2D AircraftArtwork { get; }
            public MenuIconType HazardIcon { get; }
            public MenuIconType ObjectiveIcon { get; }
        }

        private void CreatePilotSummary(Transform parent)
        {
            var summary = new GameObject("Level Selection Pilot Summary", typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuRoundedGraphic));
            summary.transform.SetParent(parent, false);
            SetRect(summary.GetComponent<RectTransform>(), new Vector2(-64f, -42f), new Vector2(340f, 88f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            summary.GetComponent<MenuRoundedGraphic>().SetStyle(new Color(0.025f, 0.16f, 0.24f, 0.97f), new Color(0.006f, 0.045f, 0.075f, 0.99f), 18f, new Color(0.20f, 0.74f, 1f, 0.52f), 2f);

            var avatarFrame = new GameObject("Pilot Avatar Frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuRoundedGraphic));
            avatarFrame.transform.SetParent(summary.transform, false);
            SetRect(avatarFrame.GetComponent<RectTransform>(), new Vector2(12f, -12f), new Vector2(64f, 64f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            avatarFrame.GetComponent<MenuRoundedGraphic>().SetStyle(new Color(0.03f, 0.34f, 0.49f, 1f), new Color(0.01f, 0.14f, 0.23f, 1f), 15f, new Color(0.30f, 0.86f, 1f, 0.72f), 2f);

            var avatarObject = new GameObject("Level Selection Pilot Avatar", typeof(RectTransform), typeof(CanvasRenderer), typeof(PilotAvatarGraphic));
            avatarObject.transform.SetParent(avatarFrame.transform, false);
            SetRect(avatarObject.GetComponent<RectTransform>(), Vector2.zero, new Vector2(58f, 58f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            pilotAvatar = avatarObject.GetComponent<PilotAvatarGraphic>();
            pilotAvatar.raycastTarget = false;

            var label = CreateText(summary.transform, "Pilot Summary Label", "PILOTO", 11, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(92f, -14f), new Vector2(150f, 22f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.46f, 0.80f, 0.96f, 1f));
            label.raycastTarget = false;
            pilotName = CreateText(summary.transform, "Pilot Summary Name", PilotProfileService.PilotName, 20, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(92f, -40f), new Vector2(226f, 32f), new Vector2(0f, 1f), new Vector2(0f, 1f), Color.white);
            pilotName.raycastTarget = false;

        }

        private void RefreshPilotProfile()
        {
            if (pilotName != null) pilotName.text = PilotProfileService.PilotName;
            if (pilotAvatar != null) pilotAvatar.AvatarId = PilotProfileService.AvatarId;
        }

        private void CreateCard(Transform parent, int order, string titleValue, string sceneName, string subtitleValue, LevelTheme theme, Texture2D backgroundTexture, float x)
        {
            var cardObject = new GameObject($"{titleValue} Level Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(CanvasGroup), typeof(MenuRoundedGraphic), typeof(Button), typeof(LevelSelectionCard));
            cardObject.transform.SetParent(parent, false);
            SetRect(cardObject.GetComponent<RectTransform>(), new Vector2(x, -206f), new Vector2(340f, 720f), new Vector2(0f, 1f), new Vector2(0f, 1f));

            var frame = cardObject.GetComponent<MenuRoundedGraphic>();
            frame.SetStyle(new Color(0.025f, 0.09f, 0.14f, 0.98f), new Color(0.008f, 0.035f, 0.06f, 1f), 22f, new Color(0.18f, 0.64f, 0.90f, 0.42f), 2f);
            var button = cardObject.GetComponent<Button>();
            button.targetGraphic = frame;
            button.transition = Selectable.Transition.None;

            Graphic artwork;
            if (backgroundTexture != null)
            {
                var artworkObject = new GameObject("Environment Artwork", typeof(RectTransform), typeof(RawImage));
                artworkObject.transform.SetParent(cardObject.transform, false);
                SetRect(artworkObject.GetComponent<RectTransform>(), new Vector2(10f, -10f), new Vector2(320f, 520f), new Vector2(0f, 1f), new Vector2(0f, 1f));
                var rawImage = artworkObject.GetComponent<RawImage>();
                rawImage.texture = backgroundTexture;
                rawImage.raycastTarget = false;
                artwork = rawImage;
            }
            else
            {
                var artworkObject = new GameObject("Environment Artwork", typeof(RectTransform), typeof(CanvasRenderer), typeof(LevelCardArtwork));
                artworkObject.transform.SetParent(cardObject.transform, false);
                SetRect(artworkObject.GetComponent<RectTransform>(), new Vector2(10f, -10f), new Vector2(320f, 520f), new Vector2(0f, 1f), new Vector2(0f, 1f));
                var vectorArtwork = artworkObject.GetComponent<LevelCardArtwork>();
                vectorArtwork.Configure(theme);
                artwork = vectorArtwork;
            }

            var vignette = CreateImage(cardObject.transform, "Artwork Vignette", new Vector2(0f, 0.49f), new Vector2(1f, 0.76f), new Vector2(10f, 0f), new Vector2(-10f, 0f), new Color(0f, 0.03f, 0.06f, 0.22f));
            vignette.raycastTarget = false;
            MenuIconType objectiveIcon = sceneName switch
            {
                "Beach" => MenuIconType.IceCream,
                "Ciudad" => MenuIconType.Animal,
                "Desert" => MenuIconType.Supplies,
                "Forest" => MenuIconType.Combat,
                _ => MenuIconType.Supplies
            };
            var objectiveBadge = CreateRoundedPanel(cardObject.transform, "Level Objective Badge", new Vector2(244f, -26f), new Vector2(70f, 70f), new Color(0.22f, 0.76f, 0.94f, 0.98f), new Color(0.03f, 0.30f, 0.48f, 0.98f));
            CreateMissionIcon(objectiveBadge.transform, "Level Objective Icon", objectiveIcon, Vector2.zero, new Vector2(48f, 48f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Color.white);
            var glow = CreateImage(cardObject.transform, "Selection Glow", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(22f, 8f), new Vector2(-22f, 13f), new Color(0.15f, 0.72f, 1f, 0.08f));
            glow.raycastTarget = false;

            var number = CreateText(cardObject.transform, "Route Number", $"0{order + 1}", 22, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(24f, -542f), new Vector2(90f, 34f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.24f, 0.76f, 1f, 1f));
            number.raycastTarget = false;
            var title = CreateText(cardObject.transform, "Route Title", titleValue, 44, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(24f, -578f), new Vector2(292f, 56f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.88f, 0.94f, 0.98f, 1f));
            title.raycastTarget = false;
            var subtitle = CreateText(cardObject.transform, "Route Subtitle", subtitleValue, 20, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(24f, -636f), new Vector2(292f, 38f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.72f, 0.84f, 0.91f, 1f));
            subtitle.raycastTarget = false;
            var status = CreateText(cardObject.transform, "Route Status", "DISPONIBLE   >", 19, FontStyle.Bold, TextAnchor.MiddleRight, new Vector2(92f, -680f), new Vector2(220f, 32f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.30f, 0.92f, 0.62f, 1f));
            status.raycastTarget = false;

            cardObject.GetComponent<LevelSelectionCard>().Configure(sceneName, order, frame, artwork, glow, title);
        }

#if UNITY_EDITOR
        private void ResolveTextures()
        {
            selectorBackground = LoadTexture(selectorBackground, BackgroundPath);
            beachBackground = LoadTexture(beachBackground, BeachPath);
            cityBackground = LoadTexture(cityBackground, CityPath);
            desertBackground = LoadTexture(desertBackground, DesertPath);
            forestBackground = LoadTexture(forestBackground, ForestPath);
            cessnaArtwork = LoadTexture(cessnaArtwork, CessnaPath);
            boeingArtwork = LoadTexture(boeingArtwork, BoeingPath);
            tomcatArtwork = LoadTexture(tomcatArtwork, TomcatPath);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        private static Texture2D LoadTexture(Texture2D current, string path)
        {
            return current != null ? current : UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
#endif

        private void CreateBackButton(Transform parent)
        {
            var buttonObject = new GameObject("Level Selection Back Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuRoundedGraphic), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            SetRect(buttonObject.GetComponent<RectTransform>(), new Vector2(64f, -48f), new Vector2(240f, 68f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            var background = buttonObject.GetComponent<MenuRoundedGraphic>();
            background.SetStyle(new Color(0.03f, 0.13f, 0.20f, 0.95f), new Color(0.01f, 0.06f, 0.10f, 0.98f), 13f, new Color(0.18f, 0.62f, 0.86f, 0.35f), 1f);
            var button = buttonObject.GetComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.ColorTint;
            CreateText(buttonObject.transform, "Back Label", "<  VOLVER", 17, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(240f, 68f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.82f, 0.91f, 0.96f, 1f)).raycastTarget = false;
        }

        private void EnsureFontsLoaded()
        {
            if (_displayFont != null && _bodyFont != null) return;

            foreach (var t in GetComponentsInChildren<Text>(true))
            {
                if (t != null && t.font != null)
                {
                    _displayFont ??= t.font;
                    _bodyFont ??= t.font;
                    break;
                }
            }

            if (_displayFont == null)
            {
                _displayFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (_displayFont == null) _displayFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            if (_bodyFont == null) _bodyFont = _displayFont;
        }

        private Text CreateText(Transform parent, string objectName, string value, int fontSize, FontStyle style, TextAnchor alignment, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot, Color color)
        {
            EnsureFontsLoaded();
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
            return text;
        }

        private static Image CreateImage(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            SetStretchRect(imageObject.GetComponent<RectTransform>(), anchorMin, anchorMax, offsetMin, offsetMax);
            var image = imageObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static RawImage CreateRawImage(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(RawImage));
            imageObject.transform.SetParent(parent, false);
            SetStretchRect(imageObject.GetComponent<RectTransform>(), anchorMin, anchorMax, offsetMin, offsetMax);
            var image = imageObject.GetComponent<RawImage>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static RectTransform CreateRect(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rectObject = new GameObject(objectName, typeof(RectTransform));
            rectObject.transform.SetParent(parent, false);
            var rect = rectObject.GetComponent<RectTransform>();
            SetStretchRect(rect, anchorMin, anchorMax, offsetMin, offsetMax);
            return rect;
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

        private void EnsureColorCustomizerUIExists()
        {
            EnsureFontsLoaded();
            foreach (var t in GetComponentsInChildren<Text>(true))
            {
                if (t != null && t.font == null)
                {
                    t.font = _displayFont;
                }
            }

            var aircraftCard = FindDescendant(transform, "Aircraft Card");
            if (aircraftCard != null)
            {
                var existingBtn = FindDescendant(aircraftCard, "Mission Detail Customize Color Button");
                if (existingBtn == null)
                {
                    _customColorStatusText = CreateText(aircraftCard, "Aircraft Color Status", "COLOR: ORIGINAL (POR DEFECTO)", 17, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(760f, -168f), new Vector2(270f, 28f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.85f, 0.95f, 1f, 1f));
                    CreateDetailButton(aircraftCard, "Mission Detail Customize Color Button", "🎨  PERSONALIZAR COLOR", new Vector2(530f, -205f), new Vector2(460f, 44f), false);
                }
                else
                {
                    _customColorStatusText ??= FindDescendant(aircraftCard, "Aircraft Color Status")?.GetComponent<Text>();
                    if (_customColorStatusText != null)
                    {
                        SetRect(_customColorStatusText.rectTransform, new Vector2(760f, -168f), new Vector2(270f, 28f), new Vector2(0f, 1f), new Vector2(0f, 1f));
                    }
                    var rect = existingBtn.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        SetRect(rect, new Vector2(530f, -205f), new Vector2(460f, 44f), new Vector2(0f, 1f), new Vector2(0f, 1f));
                    }
                }
            }

            var detailPage = FindDescendant(transform, "Mission Detail Page");
            if (detailPage != null)
            {
                var existingPanel = FindDescendant(detailPage, "Color Customizer Panel");
                if (existingPanel == null)
                {
                    BuildColorCustomizationPanel(detailPage);
                }
            }

            CachePageReferences();
            BindDetailButton("Mission Detail Customize Color Button", () => ShowColorCustomizer(true));
            BindDetailButton("Color Modal Close Button", () => ShowColorCustomizer(false));
            BindDetailButton("Reset Original Color Button", () => ResetToDefaultPlaneColor());
            BindDetailButton("Color Modal Apply Button", () => ShowColorCustomizer(false));
        }

        private void BuildColorCustomizationPanel(Transform parent)
        {
            _colorCustomizationPanel = CreateRect(parent, "Color Customizer Panel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
            var shade = CreateImage(_colorCustomizationPanel.transform, "Color Customizer Shade", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.005f, 0.02f, 0.045f, 0.90f));
            shade.raycastTarget = true;

            var modal = CreateRoundedPanel(_colorCustomizationPanel.transform, "Color Customizer Modal Card", new Vector2(520f, -180f), new Vector2(880f, 720f), new Color(0.02f, 0.11f, 0.18f, 0.99f), new Color(0.007f, 0.04f, 0.07f, 1f));

            CreateText(modal.transform, "Color Eyebrow", "TALLER DE PINTURA  /  AERONAVE", 20, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(36f, -24f), new Vector2(500f, 30f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.32f, 0.82f, 1f, 1f));
            CreateText(modal.transform, "Color Title", "COLOR DEL MATERIAL", 38, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(36f, -56f), new Vector2(650f, 48f), new Vector2(0f, 1f), new Vector2(0f, 1f), Color.white);

            CreateDetailButton(modal.transform, "Color Modal Close Button", "✕", new Vector2(790f, -24f), new Vector2(64f, 64f), false);

            CreateText(modal.transform, "Preview Label", "VISTA PREVIA DE COLOR:", 19, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(36f, -126f), new Vector2(360f, 28f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.75f, 0.88f, 0.96f, 1f));
            _colorPreviewSwatch = CreateFixedImage(modal.transform, "Color Preview Swatch", new Vector2(36f, -160f), new Vector2(360f, 240f), Color.white, new Vector2(0f, 1f), new Vector2(0f, 1f));

            var swatchGlow = CreateFixedImage(modal.transform, "Swatch Glow Border", new Vector2(34f, -158f), new Vector2(364f, 244f), new Color(0.32f, 0.82f, 1f, 0.35f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            swatchGlow.transform.SetSiblingIndex(_colorPreviewSwatch.transform.GetSiblingIndex());

            _colorPreviewHexText = CreateText(modal.transform, "Color RGB Text", "RGB: (255, 255, 255)", 20, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(36f, -415f), new Vector2(360f, 36f), new Vector2(0f, 1f), new Vector2(0f, 1f), Color.white);

            CreateDetailButton(modal.transform, "Reset Original Color Button", "🔄  COLOR ORIGINAL DEL AVIÓN", new Vector2(36f, -470f), new Vector2(360f, 64f), false);

            CreateText(modal.transform, "RGB Sliders Label", "AJUSTE LIBRE RGB (0 - 255):", 19, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(430f, -126f), new Vector2(410f, 28f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.75f, 0.88f, 0.96f, 1f));

            _rSlider = CreateColorSlider(modal.transform, "R Slider Container", "ROJO (R)", new Vector2(430f, -165f), new Color(1f, 0.28f, 0.28f, 1f));
            _gSlider = CreateColorSlider(modal.transform, "G Slider Container", "VERDE (G)", new Vector2(430f, -240f), new Color(0.28f, 0.92f, 0.45f, 1f));
            _bSlider = CreateColorSlider(modal.transform, "B Slider Container", "AZUL (B)", new Vector2(430f, -315f), new Color(0.28f, 0.65f, 1f, 1f));

            CreateText(modal.transform, "Presets Label", "COLORES PREDEFINIDOS:", 19, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(430f, -390f), new Vector2(410f, 28f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.75f, 0.88f, 0.96f, 1f));

            CreateColorPresetButton(modal.transform, "Preset Blanco", new Vector2(430f, -428f), new Color(1f, 1f, 1f, 1f));
            CreateColorPresetButton(modal.transform, "Preset Rojo", new Vector2(534f, -428f), new Color(0.92f, 0.18f, 0.18f, 1f));
            CreateColorPresetButton(modal.transform, "Preset Azul", new Vector2(638f, -428f), new Color(0.08f, 0.58f, 0.95f, 1f));
            CreateColorPresetButton(modal.transform, "Preset Amarillo", new Vector2(742f, -428f), new Color(0.98f, 0.82f, 0.12f, 1f));

            CreateColorPresetButton(modal.transform, "Preset Verde", new Vector2(430f, -488f), new Color(0.18f, 0.76f, 0.32f, 1f));
            CreateColorPresetButton(modal.transform, "Preset Naranja", new Vector2(534f, -488f), new Color(0.98f, 0.48f, 0.10f, 1f));
            CreateColorPresetButton(modal.transform, "Preset Morado", new Vector2(638f, -488f), new Color(0.68f, 0.22f, 0.95f, 1f));
            CreateColorPresetButton(modal.transform, "Preset Negro", new Vector2(742f, -488f), new Color(0.15f, 0.16f, 0.20f, 1f));

            CreateDetailButton(modal.transform, "Color Modal Apply Button", "✓  APLICAR COLOR Y REGRESAR", new Vector2(36f, -615f), new Vector2(808f, 74f), true);

            _colorCustomizationPanel.SetActive(false);
        }

        private void ShowColorCustomizer(bool show)
        {
            MenuUiAudio.PlayClick();
            if (_colorCustomizationPanel == null)
            {
                _colorCustomizationPanel = FindDescendant(transform, "Color Customizer Panel")?.gameObject;
            }
            if (_colorCustomizationPanel != null)
            {
                _colorCustomizationPanel.SetActive(show);
                if (show)
                {
                    bool isCustom = PlayerPrefs.GetInt("CustomPlaneColor_Enabled", 0) == 1;
                    float r = isCustom ? PlayerPrefs.GetFloat("CustomPlaneColor_R", 1f) : 1f;
                    float g = isCustom ? PlayerPrefs.GetFloat("CustomPlaneColor_G", 1f) : 1f;
                    float b = isCustom ? PlayerPrefs.GetFloat("CustomPlaneColor_B", 1f) : 1f;

                    _suppressColorSliderCallback = true;
                    if (_rSlider != null) _rSlider.value = r;
                    if (_gSlider != null) _gSlider.value = g;
                    if (_bSlider != null) _bSlider.value = b;
                    _suppressColorSliderCallback = false;

                    if (_colorPreviewSwatch != null) _colorPreviewSwatch.color = isCustom ? new Color(r, g, b, 1f) : Color.white;
                    if (_colorPreviewHexText != null)
                    {
                        if (isCustom)
                        {
                            int rInt = Mathf.RoundToInt(r * 255f);
                            int gInt = Mathf.RoundToInt(g * 255f);
                            int bInt = Mathf.RoundToInt(b * 255f);
                            _colorPreviewHexText.text = $"RGB: ({rInt}, {gInt}, {bInt}) - PERSONALIZADO";
                        }
                        else
                        {
                            _colorPreviewHexText.text = "COLOR: ORIGINAL DEL AVIÓN (POR DEFECTO)";
                        }
                    }
                }
            }
            UpdateCustomColorStatusDisplay();
        }

        private void OnColorSliderChanged()
        {
            if (_suppressColorSliderCallback || _rSlider == null || _gSlider == null || _bSlider == null) return;

            float r = _rSlider.value;
            float g = _gSlider.value;
            float b = _bSlider.value;
            Color customColor = new Color(r, g, b, 1f);

            PlayerPrefs.SetFloat("CustomPlaneColor_R", r);
            PlayerPrefs.SetFloat("CustomPlaneColor_G", g);
            PlayerPrefs.SetFloat("CustomPlaneColor_B", b);
            PlayerPrefs.SetInt("CustomPlaneColor_Enabled", 1);
            PlayerPrefs.Save();

            if (_colorPreviewSwatch != null) _colorPreviewSwatch.color = customColor;
            if (_colorPreviewHexText != null)
            {
                int rInt = Mathf.RoundToInt(r * 255f);
                int gInt = Mathf.RoundToInt(g * 255f);
                int bInt = Mathf.RoundToInt(b * 255f);
                _colorPreviewHexText.text = $"RGB: ({rInt}, {gInt}, {bInt}) - PERSONALIZADO";
            }

            UpdateCustomColorStatusDisplay();
        }

        private void ApplyColorPreset(Color color)
        {
            _suppressColorSliderCallback = true;
            if (_rSlider != null) _rSlider.value = color.r;
            if (_gSlider != null) _gSlider.value = color.g;
            if (_bSlider != null) _bSlider.value = color.b;
            _suppressColorSliderCallback = false;
            OnColorSliderChanged();
        }

        private void ResetToDefaultPlaneColor()
        {
            MenuUiAudio.PlayClick();
            PlayerPrefs.SetInt("CustomPlaneColor_Enabled", 0);
            PlayerPrefs.Save();

            _suppressColorSliderCallback = true;
            if (_rSlider != null) _rSlider.value = 1f;
            if (_gSlider != null) _gSlider.value = 1f;
            if (_bSlider != null) _bSlider.value = 1f;
            _suppressColorSliderCallback = false;

            if (_colorPreviewSwatch != null) _colorPreviewSwatch.color = Color.white;
            if (_colorPreviewHexText != null) _colorPreviewHexText.text = "COLOR: ORIGINAL DEL AVIÓN (POR DEFECTO)";

            UpdateCustomColorStatusDisplay();
        }

        private void UpdateCustomColorStatusDisplay()
        {
            bool isCustom = PlayerPrefs.GetInt("CustomPlaneColor_Enabled", 0) == 1;
            if (isCustom)
            {
                float r = PlayerPrefs.GetFloat("CustomPlaneColor_R", 1f);
                float g = PlayerPrefs.GetFloat("CustomPlaneColor_G", 1f);
                float b = PlayerPrefs.GetFloat("CustomPlaneColor_B", 1f);
                Color c = new Color(r, g, b, 1f);

                int rInt = Mathf.RoundToInt(r * 255f);
                int gInt = Mathf.RoundToInt(g * 255f);
                int bInt = Mathf.RoundToInt(b * 255f);

                if (_customColorStatusText != null)
                {
                    _customColorStatusText.text = $"COLOR: PERSONALIZADO ({rInt}, {gInt}, {bInt})  ■";
                    _customColorStatusText.color = new Color(Mathf.Max(0.5f, r), Mathf.Max(0.5f, g), Mathf.Max(0.5f, b), 1f);
                }
                if (_detailAircraft != null)
                {
                    _detailAircraft.color = Color.Lerp(Color.white, c, 0.65f);
                }
            }
            else
            {
                if (_customColorStatusText != null)
                {
                    _customColorStatusText.text = "COLOR: ORIGINAL (POR DEFECTO)";
                    _customColorStatusText.color = new Color(0.85f, 0.95f, 1f, 1f);
                }
                if (_detailAircraft != null)
                {
                    _detailAircraft.color = Color.white;
                }
            }
        }

        private Slider CreateColorSlider(Transform parent, string objectName, string labelText, Vector2 position, Color fillTint)
        {
            var container = new GameObject(objectName, typeof(RectTransform));
            container.transform.SetParent(parent, false);
            SetRect(container.GetComponent<RectTransform>(), position, new Vector2(410f, 56f), new Vector2(0f, 1f), new Vector2(0f, 1f));

            CreateText(container.transform, "Label", labelText, 17, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(0f, 0f), new Vector2(300f, 22f), new Vector2(0f, 1f), new Vector2(0f, 1f), Color.white);

            var root = new GameObject("Slider Control", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Slider));
            root.transform.SetParent(container.transform, false);
            SetRect(root.GetComponent<RectTransform>(), new Vector2(0f, -24f), new Vector2(410f, 28f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            root.GetComponent<Image>().color = Color.clear;

            var background = CreateImage(root.transform, "Background", new Vector2(0f, 0.2f), new Vector2(1f, 0.8f), Vector2.zero, Vector2.zero, new Color(0.003f, 0.02f, 0.04f, 0.94f));
            background.raycastTarget = false;

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(root.transform, false);
            SetStretchRect(fillArea.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(6f, 8f), new Vector2(-6f, -8f));
            var fill = CreateImage(fillArea.transform, "Fill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, fillTint);
            fill.raycastTarget = false;

            var handleArea = new GameObject("Handle Area", typeof(RectTransform));
            handleArea.transform.SetParent(root.transform, false);
            SetStretchRect(handleArea.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(10f, 0f), new Vector2(-10f, 0f));
            var handle = CreateFixedImage(handleArea.transform, "Handle", Vector2.zero, new Vector2(22f, 32f), Color.white, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f));

            var slider = root.GetComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.interactable = true;
            slider.onValueChanged.AddListener(_ => OnColorSliderChanged());
            return slider;
        }

        private void CreateColorPresetButton(Transform parent, string objectName, Vector2 position, Color presetColor)
        {
            var buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuRoundedGraphic), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            SetRect(buttonObject.GetComponent<RectTransform>(), position, new Vector2(94f, 52f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            var background = buttonObject.GetComponent<MenuRoundedGraphic>();
            background.SetStyle(presetColor, presetColor * 0.7f, 12f, Color.white * 0.5f, 2f);
            var button = buttonObject.GetComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.ColorTint;
            button.onClick.AddListener(() =>
            {
                MenuUiAudio.PlayClick();
                ApplyColorPreset(presetColor);
            });
        }

        private static Image CreateFixedImage(Transform parent, string objectName, Vector2 position, Vector2 size, Color color, Vector2 anchor, Vector2 pivot)
        {
            var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            SetRect(imageObject.GetComponent<RectTransform>(), position, size, anchor, pivot);
            var image = imageObject.GetComponent<Image>();
            image.color = color;
            return image;
        }
    }
}
