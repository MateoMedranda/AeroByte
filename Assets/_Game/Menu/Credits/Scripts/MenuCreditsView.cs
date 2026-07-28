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
            var headerIcon = CreateRounded(transform, "Credits Header Icon Container", new Vector2(62f, -42f), new Vector2(82f, 82f), new Color(0.04f, 0.24f, 0.36f, 1f), new Color(0.02f, 0.12f, 0.21f, 1f), 22f, new Color(0.25f, 0.78f, 1f, 0.48f), 2f);
            CreateIcon(headerIcon.transform, "Credits Header Icon", MenuIconType.Credits, Vector2.zero, new Vector2(50f, 50f), _accentColor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            CreateText("Credits Title", 58, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(170f, -38f), new Vector2(720f, 72f), "CRÉDITOS");
            CreateText("Credits Breadcrumb", 18, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(174f, -106f), new Vector2(980f, 34f), "AEROBYTE  /  EQUIPO Y PROYECTO ACADÉMICO").color = _mutedTextColor;
            CreateText("Credits Project Mark", 18, FontStyle.Bold, TextAnchor.MiddleRight, new Vector2(1190f, -70f), new Vector2(440f, 38f), "FLIGHT SIMULATOR  /  2026").color = _accentColor;
            CreateImage("Header Line", new Vector2(62f, -154f), new Vector2(1568f, 3f), new Color(_accentColor.r, _accentColor.g, _accentColor.b, 0.58f));

            var teamCard = CreateCard("Team Card", new Vector2(62f, -184f), new Vector2(1020f, 620f));
            CreateIcon(teamCard.transform, "Team Icon", MenuIconType.Credits, new Vector2(32f, -26f), new Vector2(48f, 48f), _accentColor);
            CreateText(teamCard.transform, "Team Title", 30, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(98f, -22f), new Vector2(560f, 44f), "EQUIPO DE DESARROLLO");
            CreateText(teamCard.transform, "Team Caption", 16, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(100f, -68f), new Vector2(600f, 30f), "GRUPO 7  /  INGENIERÍA DE SOFTWARE").color = _mutedTextColor;

            CreatePersonRow(teamCard.transform, "01", "Mateo Medranda", "DESARROLLADOR", MenuIconType.Project, -112f);
            CreatePersonRow(teamCard.transform, "02", "Elkin Pabón", "DESARROLLADOR UI", MenuIconType.Tools, -238f);
            CreatePersonRow(teamCard.transform, "03", "Alexander Villacrés", "DESARROLLADOR", MenuIconType.Controls, -364f);

            CreateImage(teamCard.transform, "Tools Divider", new Vector2(32f, -494f), new Vector2(956f, 2f), new Color(1f, 1f, 1f, 0.10f));
            CreateIcon(teamCard.transform, "Tools Icon", MenuIconType.Tools, new Vector2(34f, -516f), new Vector2(38f, 38f), _accentColor);
            CreateText(teamCard.transform, "Tools Title", 18, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(88f, -512f), new Vector2(300f, 38f), "HERRAMIENTAS").color = _accentColor;
            CreateToolChip(teamCard.transform, "UNITY", new Vector2(330f, -516f), 112f);
            CreateToolChip(teamCard.transform, "BLENDER", new Vector2(454f, -516f), 126f);
            CreateToolChip(teamCard.transform, "VISUAL STUDIO", new Vector2(592f, -516f), 176f);
            CreateToolChip(teamCard.transform, "C#", new Vector2(780f, -516f), 74f);
            CreateToolChip(teamCard.transform, "FIGMA", new Vector2(866f, -516f), 112f);

            var academicCard = CreateCard("Academic Card", new Vector2(1110f, -184f), new Vector2(520f, 620f));
            CreateText(academicCard.transform, "Academic Heading", 27, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(32f, -24f), new Vector2(450f, 44f), "INFORMACIÓN ACADÉMICA");
            CreateImage(academicCard.transform, "Academic Heading Line", new Vector2(32f, -78f), new Vector2(456f, 2f), new Color(_accentColor.r, _accentColor.g, _accentColor.b, 0.38f));
            CreateAcademicSection(academicCard.transform, MenuIconType.University, "UNIVERSIDAD", "Universidad de las Fuerzas Armadas ESPE", -104f);
            CreateAcademicSection(academicCard.transform, MenuIconType.Credits, "CARRERA", "Ingeniería de Software", -232f);
            CreateAcademicSection(academicCard.transform, MenuIconType.Project, "PROYECTO", "AeroByte Flight Simulator", -360f);
            CreateAcademicSection(academicCard.transform, MenuIconType.Calendar, "AÑO", "2026", -488f);

            var descriptionCard = CreateRounded(transform, "Project Description Card", new Vector2(62f, -826f), new Vector2(1080f, 76f), new Color(0.025f, 0.14f, 0.21f, 0.98f), new Color(0.008f, 0.055f, 0.09f, 1f), 18f, new Color(0.20f, 0.68f, 0.92f, 0.24f), 1f);
            CreateIcon(descriptionCard.transform, "Project Description Icon", MenuIconType.Project, new Vector2(26f, -14f), new Vector2(48f, 48f), _accentColor);
            var projectNote = CreateText(descriptionCard.transform, "Project Description", 17, FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(94f, -10f), new Vector2(950f, 56f), "Proyecto desarrollado en la carrera de Ingeniería de Software de la Universidad de las Fuerzas Armadas ESPE.");
            projectNote.color = new Color(0.78f, 0.88f, 0.94f, 1f);
            CreateBackButton();
        }

        private void CreatePersonRow(Transform parent, string number, string personName, string role, MenuIconType iconType, float y)
        {
            var row = CreateRounded(parent, $"Developer {number} Card", new Vector2(30f, y), new Vector2(960f, 108f), new Color(0.028f, 0.15f, 0.22f, 0.98f), new Color(0.01f, 0.07f, 0.11f, 0.99f), 18f, new Color(0.18f, 0.60f, 0.84f, 0.20f), 1f);
            var badge = CreateRounded(row.transform, $"Developer {number} Badge", new Vector2(18f, -18f), new Vector2(72f, 72f), new Color(0.04f, 0.30f, 0.43f, 1f), new Color(0.02f, 0.15f, 0.25f, 1f), 18f, new Color(0.30f, 0.82f, 1f, 0.42f), 2f);
            CreateText(badge.transform, "Number", 20, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(72f, 72f), number, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            CreateText(row.transform, $"Developer {number}", 28, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(116f, -18f), new Vector2(500f, 42f), personName);
            CreateText(row.transform, $"Developer {number} Role", 16, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(118f, -62f), new Vector2(420f, 28f), role).color = _accentColor;
            var roleContainer = CreateRounded(row.transform, $"Developer {number} Role Icon Container", new Vector2(858f, -22f), new Vector2(64f, 64f), new Color(0.02f, 0.18f, 0.27f, 1f), new Color(0.01f, 0.09f, 0.15f, 1f), 16f, Color.clear, 0f);
            CreateIcon(roleContainer.transform, $"Developer {number} Role Icon", iconType, Vector2.zero, new Vector2(38f, 38f), _accentColor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        }

        private void CreateToolChip(Transform parent, string label, Vector2 position, float width)
        {
            var chip = CreateRounded(parent, $"{label} Tool", position, new Vector2(width, 42f), new Color(0.02f, 0.13f, 0.20f, 0.95f), new Color(0.03f, 0.20f, 0.29f, 0.95f), 10f, new Color(0.19f, 0.63f, 0.88f, 0.28f), 1f);
            var text = CreateText(chip.transform, "Label", 14, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(width, 42f), label, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            text.color = new Color(0.74f, 0.87f, 0.94f, 1f);
        }

        private void CreateAcademicSection(Transform parent, MenuIconType iconType, string title, string content, float y)
        {
            var iconContainer = CreateRounded(parent, $"{title} Icon Container", new Vector2(30f, y), new Vector2(70f, 70f), new Color(0.02f, 0.17f, 0.26f, 1f), new Color(0.03f, 0.28f, 0.40f, 1f), 18f, new Color(0.20f, 0.70f, 0.94f, 0.24f), 1f);
            CreateIcon(iconContainer.transform, $"{title} Icon", iconType, Vector2.zero, new Vector2(40f, 40f), _accentColor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            CreateText(parent, $"{title} Title", 16, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(122f, y), new Vector2(350f, 30f), title).color = _accentColor;
            CreateText(parent, $"{title} Content", 21, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(122f, y - 34f), new Vector2(360f, 56f), content);
            if (title != "AÑO") CreateImage(parent, $"{title} Separator", new Vector2(30f, y - 100f), new Vector2(458f, 2f), new Color(1f, 1f, 1f, 0.09f));
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
            var image = CreateRounded(transform, "Back Button", new Vector2(1210f, -826f), new Vector2(420f, 76f), normalTop, normalBottom, 18f, new Color(0.35f, 0.82f, 1f, 0.42f), 2f);
            image.AddComponent<CanvasGroup>();
            var background = image.GetComponent<MenuRoundedGraphic>();
            var button = image.AddComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(_onBack);
            var icon = CreateIcon(image.transform, "Back Icon", MenuIconType.Back, new Vector2(34f, 0f), new Vector2(36f, 36f), Color.white, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            var label = CreateText(image.transform, "Back Button Label", 21, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(92f, 0f), new Vector2(286f, 76f), "VOLVER AL MENÚ", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
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
