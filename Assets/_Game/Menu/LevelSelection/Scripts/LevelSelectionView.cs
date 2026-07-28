using System;
using AeroByte.Menu.Profile;
using AeroByte.Menu.UI;
using UnityEngine;
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

        [SerializeField] private Texture2D selectorBackground;
        [SerializeField] private Texture2D beachBackground;
        [SerializeField] private Texture2D cityBackground;
        [SerializeField] private Texture2D desertBackground;
        [SerializeField] private Texture2D forestBackground;

        private Font _displayFont;
        private Font _bodyFont;
        private Action<string> _onLevelSelected;
        private Action _onBack;
        [SerializeField] private PilotAvatarGraphic pilotAvatar;
        [SerializeField] private Text pilotName;

        private void OnEnable()
        {
            RefreshPilotProfile();
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

            foreach (var card in GetComponentsInChildren<LevelSelectionCard>(true)) card.Bind(_onLevelSelected);
            var backButton = FindDescendant(transform, "Level Selection Back Button")?.GetComponent<Button>();
            if (backButton == null) return;
            backButton.onClick.RemoveAllListeners();
            if (_onBack != null) backButton.onClick.AddListener(() => _onBack());
        }

        private void Build()
        {
            var content = CreateRect(transform, "Level Selection Content", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var background = CreateRawImage(content, "Level Selector Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
            background.texture = selectorBackground;
            CreateImage(content, "Background Shade", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.005f, 0.025f, 0.045f, 0.34f));
            CreateImage(content, "Top Glow", new Vector2(0f, 0.76f), Vector2.one, Vector2.zero, Vector2.zero, new Color(0.02f, 0.20f, 0.34f, 0.38f));

            CreateText(content, "Level Selection Eyebrow", "AEROBYTE  /  CENTRO DE OPERACIONES", 14, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0f, -52f), new Vector2(900f, 28f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Color(0.28f, 0.76f, 1f, 1f));
            CreateText(content, "Level Selection Heading", "SELECCIONA TU DESTINO", 42, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0f, -86f), new Vector2(1000f, 58f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Color.white);
            CreatePilotSummary(content);
            CreateCard(content, 0, "PLAYA", "Beach", "OPERACION LITORAL", LevelTheme.Beach, beachBackground, 208f);
            CreateCard(content, 1, "CIUDAD", "Ciudad", "RUTA URBANA", LevelTheme.City, cityBackground, 584f);
            CreateCard(content, 2, "DESIERTO", "Desert", "TRAVESIA ENTRE DUNAS", LevelTheme.Desert, desertBackground, 960f);
            CreateCard(content, 3, "BOSQUE", "Forest", "VUELO DE MONTANA", LevelTheme.Forest, forestBackground, 1336f);
            CreateBackButton(content);
            RefreshPilotProfile();
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

            var statusBadge = new GameObject("Pilot Active Badge", typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuRoundedGraphic));
            statusBadge.transform.SetParent(summary.transform, false);
            SetRect(statusBadge.GetComponent<RectTransform>(), new Vector2(254f, -14f), new Vector2(70f, 22f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            statusBadge.GetComponent<MenuRoundedGraphic>().SetStyle(new Color(0.10f, 0.48f, 0.32f, 0.96f), new Color(0.03f, 0.23f, 0.16f, 0.98f), 11f, new Color(0.26f, 0.92f, 0.60f, 0.50f), 1f);
            CreateText(statusBadge.transform, "Active Badge Label", "ACTIVO", 9, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(70f, 22f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.72f, 1f, 0.84f, 1f)).raycastTarget = false;
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
            var glow = CreateImage(cardObject.transform, "Selection Glow", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(22f, 8f), new Vector2(-22f, 13f), new Color(0.15f, 0.72f, 1f, 0.08f));
            glow.raycastTarget = false;

            var number = CreateText(cardObject.transform, "Route Number", $"0{order + 1}", 13, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(24f, -552f), new Vector2(80f, 24f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.24f, 0.76f, 1f, 1f));
            number.raycastTarget = false;
            var title = CreateText(cardObject.transform, "Route Title", titleValue, 28, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(24f, -582f), new Vector2(280f, 42f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.88f, 0.94f, 0.98f, 1f));
            title.raycastTarget = false;
            var subtitle = CreateText(cardObject.transform, "Route Subtitle", subtitleValue, 12, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(24f, -628f), new Vector2(280f, 26f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.56f, 0.70f, 0.79f, 1f));
            subtitle.raycastTarget = false;
            var status = CreateText(cardObject.transform, "Route Status", "DISPONIBLE   >", 12, FontStyle.Bold, TextAnchor.MiddleRight, new Vector2(136f, -674f), new Vector2(176f, 24f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.30f, 0.84f, 0.58f, 1f));
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
    }
}
