using AeroByte.Menu.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AeroByte.Menu.Credits
{
    public sealed class MenuCreditsView : MonoBehaviour
    {
        private Font _displayFont;
        private Font _bodyFont;
        private Color _textColor;
        private Color _accentColor;
        private Color _mutedTextColor;
        private UnityAction _onBack;

        public void Initialize(Font displayFont, Font bodyFont, Color textColor, Color accentColor, UnityAction onBack)
        {
            _displayFont = displayFont;
            _bodyFont = bodyFont;
            _textColor = textColor;
            _accentColor = accentColor;
            _mutedTextColor = new Color(0.64f, 0.75f, 0.82f, 1f);
            _onBack = onBack;
            Build();
        }

        private void Build()
        {
            CreateIcon("Credits Header Icon", MenuIconType.Credits, new Vector2(58f, -48f), new Vector2(50f, 50f), _accentColor);
            CreateText("Credits Title", 42, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(126f, -40f), new Vector2(520f, 58f), "CRÉDITOS");
            CreateText("Credits Breadcrumb", 13, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(128f, -98f), new Vector2(720f, 28f), "MENÚ  /  EQUIPO Y PROYECTO ACADÉMICO").color = _mutedTextColor;
            CreateImage("Header Line", new Vector2(58f, -144f), new Vector2(1104f, 3f), new Color(_accentColor.r, _accentColor.g, _accentColor.b, 0.50f));

            var teamCard = CreateCard("Team Card", new Vector2(58f, -180f), new Vector2(520f, 500f));
            CreateIcon(teamCard.transform, "Team Icon", MenuIconType.Credits, new Vector2(30f, -28f), new Vector2(36f, 36f), _accentColor);
            CreateText(teamCard.transform, "Team Title", 21, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(82f, -26f), new Vector2(360f, 34f), "EQUIPO DE DESARROLLO");
            CreateText(teamCard.transform, "Team Caption", 12, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(84f, -62f), new Vector2(360f, 24f), "GRUPO 7  /  INGENIERÍA DE SOFTWARE").color = _mutedTextColor;

            CreatePersonRow(teamCard.transform, "01", "Mateo Medranda", "DESARROLLADOR", -116f);
            CreatePersonRow(teamCard.transform, "02", "Elkin Pabón", "DESARROLLADOR UI", -182f);
            CreatePersonRow(teamCard.transform, "03", "Alexander Villacrés", "DESARROLLADOR", -248f);

            CreateImage(teamCard.transform, "Tools Divider", new Vector2(30f, -326f), new Vector2(460f, 2f), new Color(1f, 1f, 1f, 0.09f));
            CreateIcon(teamCard.transform, "Tools Icon", MenuIconType.Tools, new Vector2(30f, -356f), new Vector2(30f, 30f), _accentColor);
            CreateText(teamCard.transform, "Tools Title", 14, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(76f, -352f), new Vector2(330f, 30f), "HERRAMIENTAS UTILIZADAS").color = _accentColor;
            CreateToolChip(teamCard.transform, "UNITY", new Vector2(30f, -408f), 92f);
            CreateToolChip(teamCard.transform, "BLENDER", new Vector2(132f, -408f), 104f);
            CreateToolChip(teamCard.transform, "VISUAL STUDIO", new Vector2(246f, -408f), 146f);
            CreateToolChip(teamCard.transform, "C#", new Vector2(402f, -408f), 58f);
            CreateToolChip(teamCard.transform, "FIGMA", new Vector2(30f, -454f), 88f);

            var academicCard = CreateCard("Academic Card", new Vector2(614f, -180f), new Vector2(548f, 500f));
            CreateAcademicSection(academicCard.transform, MenuIconType.University, "UNIVERSIDAD", "Universidad de las Fuerzas Armadas ESPE", -30f);
            CreateAcademicSection(academicCard.transform, MenuIconType.Project, "CARRERA", "Ingeniería de Software", -142f);
            CreateAcademicSection(academicCard.transform, MenuIconType.Project, "PROYECTO ACADÉMICO", "Aerobyte Flight Simulator", -254f);
            CreateAcademicSection(academicCard.transform, MenuIconType.Calendar, "AÑO", "2026", -366f);

            var projectNote = CreateText("Project Description", 13, FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(58f, -714f), new Vector2(660f, 54f), "Proyecto desarrollado como parte de la carrera de Ingeniería de Software\nde la Universidad de las Fuerzas Armadas ESPE.");
            projectNote.color = _mutedTextColor;
            CreateBackButton();
        }

        private void CreatePersonRow(Transform parent, string number, string personName, string role, float y)
        {
            var badge = CreateRounded(parent, $"Developer {number} Badge", new Vector2(30f, y), new Vector2(48f, 48f), new Color(0.03f, 0.20f, 0.31f, 1f), new Color(0.04f, 0.36f, 0.52f, 1f), 13f, new Color(0.25f, 0.75f, 1f, 0.28f), 1f);
            CreateText(badge.transform, "Number", 13, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(48f, 48f), number, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            CreateText(parent, $"Developer {number}", 18, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(96f, y - 1f), new Vector2(330f, 28f), personName);
            CreateText(parent, $"Developer {number} Role", 11, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(98f, y - 28f), new Vector2(250f, 20f), role).color = _mutedTextColor;
        }

        private void CreateToolChip(Transform parent, string label, Vector2 position, float width)
        {
            var chip = CreateRounded(parent, $"{label} Tool", position, new Vector2(width, 32f), new Color(0.02f, 0.13f, 0.20f, 0.95f), new Color(0.03f, 0.20f, 0.29f, 0.95f), 8f, new Color(0.19f, 0.63f, 0.88f, 0.20f), 1f);
            var text = CreateText(chip.transform, "Label", 11, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(width, 32f), label, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            text.color = new Color(0.74f, 0.87f, 0.94f, 1f);
        }

        private void CreateAcademicSection(Transform parent, MenuIconType iconType, string title, string content, float y)
        {
            var iconContainer = CreateRounded(parent, $"{title} Icon Container", new Vector2(30f, y), new Vector2(48f, 48f), new Color(0.02f, 0.17f, 0.26f, 1f), new Color(0.03f, 0.28f, 0.40f, 1f), 13f, Color.clear, 0f);
            CreateIcon(iconContainer.transform, $"{title} Icon", iconType, Vector2.zero, new Vector2(26f, 26f), _accentColor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            CreateText(parent, $"{title} Title", 13, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(98f, y), new Vector2(400f, 24f), title).color = _accentColor;
            CreateText(parent, $"{title} Content", 17, FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(98f, y - 27f), new Vector2(410f, 32f), content);
            if (title != "AÑO") CreateImage(parent, $"{title} Separator", new Vector2(30f, y - 82f), new Vector2(488f, 2f), new Color(1f, 1f, 1f, 0.08f));
        }

        private GameObject CreateCard(string objectName, Vector2 position, Vector2 size)
        {
            var card = CreateRounded(transform, objectName, position, size, new Color(0.02f, 0.105f, 0.16f, 0.94f), new Color(0.012f, 0.06f, 0.10f, 0.96f), 22f, new Color(0.24f, 0.68f, 0.92f, 0.16f), 1f);
            var shadow = card.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0.01f, 0.02f, 0.44f);
            shadow.effectDistance = new Vector2(0f, -7f);
            return card;
        }

        private void CreateBackButton()
        {
            Color normalTop = new Color(0.05f, 0.55f, 0.98f, 0.98f);
            Color normalBottom = new Color(0.02f, 0.31f, 0.70f, 1f);
            Color hoverTop = new Color(0.10f, 0.68f, 1f, 1f);
            Color hoverBottom = new Color(0.03f, 0.43f, 0.84f, 1f);
            var image = CreateRounded(transform, "Back Button", new Vector2(820f, -754f), new Vector2(342f, 64f), normalTop, normalBottom, 16f, new Color(0.35f, 0.82f, 1f, 0.42f), 1f);
            image.AddComponent<CanvasGroup>();
            var background = image.GetComponent<MenuRoundedGraphic>();
            var button = image.AddComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(_onBack);
            var icon = CreateIcon(image.transform, "Back Icon", MenuIconType.Back, new Vector2(28f, 0f), new Vector2(28f, 28f), Color.white, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            var label = CreateText(image.transform, "Back Button Label", 16, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(76f, 0f), new Vector2(230f, 64f), "VOLVER AL MENÚ", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            label.raycastTarget = false;
            image.AddComponent<MenuButtonMotion>().Configure(background, icon, null, label, normalTop, normalBottom, hoverTop, hoverBottom, Color.white, Color.white, 0.08f);
        }

        private GameObject CreateRounded(Transform parent, string objectName, Vector2 position, Vector2 size, Color top, Color bottom, float radius, Color border, float borderWidth)
        {
            var roundedObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuRoundedGraphic));
            roundedObject.transform.SetParent(parent, false);
            SetRect(roundedObject.GetComponent<RectTransform>(), position, size, new Vector2(0f, 1f), new Vector2(0f, 1f));
            roundedObject.GetComponent<MenuRoundedGraphic>().SetStyle(top, bottom, radius, border, borderWidth);
            return roundedObject;
        }

        private MenuIconGraphic CreateIcon(string objectName, MenuIconType type, Vector2 position, Vector2 size, Color tint)
        {
            return CreateIcon(transform, objectName, type, position, size, tint);
        }

        private static MenuIconGraphic CreateIcon(Transform parent, string objectName, MenuIconType type, Vector2 position, Vector2 size, Color tint, Vector2? anchor = null, Vector2? pivot = null)
        {
            var iconObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuIconGraphic));
            iconObject.transform.SetParent(parent, false);
            SetRect(iconObject.GetComponent<RectTransform>(), position, size, anchor ?? new Vector2(0f, 1f), pivot ?? new Vector2(0f, 1f));
            var icon = iconObject.GetComponent<MenuIconGraphic>();
            icon.Configure(type, tint);
            return icon;
        }

        private Image CreateImage(string objectName, Vector2 position, Vector2 size, Color color) => CreateImage(transform, objectName, position, size, color);

        private static Image CreateImage(Transform parent, string objectName, Vector2 position, Vector2 size, Color color)
        {
            var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            SetRect(imageObject.GetComponent<RectTransform>(), position, size, new Vector2(0f, 1f), new Vector2(0f, 1f));
            var image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private Text CreateText(string objectName, int fontSize, FontStyle style, TextAnchor alignment, Vector2 position, Vector2 size, string value) => CreateText(transform, objectName, fontSize, style, alignment, position, size, value);

        private Text CreateText(Transform parent, string objectName, int fontSize, FontStyle style, TextAnchor alignment, Vector2 position, Vector2 size, string value, Vector2? anchor = null, Vector2? pivot = null)
        {
            var textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            SetRect(textObject.GetComponent<RectTransform>(), position, size, anchor ?? new Vector2(0f, 1f), pivot ?? new Vector2(0f, 1f));
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
    }
}
