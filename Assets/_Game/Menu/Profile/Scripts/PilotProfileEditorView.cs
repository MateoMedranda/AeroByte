using System;
using AeroByte.Menu.UI;
using UnityEngine;
using UnityEngine.UI;

namespace AeroByte.Menu.Profile
{
    public sealed class PilotProfileEditorView : MonoBehaviour
    {
        private Font _displayFont;
        private Font _bodyFont;
        private Color _textColor;
        private Color _accentColor;
        private Color _mutedTextColor;
        private InputField _nameInput;
        private Text _previewName;
        private Text _previewAvatarName;
        private PilotAvatarGraphic _previewAvatar;
        private PilotAvatarOption[] _options;
        private Action _onSaved;
        private Action _onCancel;
        private int _selectedAvatarId;

        public void Initialize(Font displayFont, Font bodyFont, Color textColor, Color accentColor, Action onSaved, Action onCancel)
        {
            _displayFont = displayFont;
            _bodyFont = bodyFont;
            _textColor = textColor;
            _accentColor = accentColor;
            _mutedTextColor = new Color(0.64f, 0.76f, 0.83f, 1f);
            _onSaved = onSaved;
            _onCancel = onCancel;
            Build();
            Bind(displayFont, bodyFont, onSaved, onCancel);
        }

        public void Bind(Font displayFont, Font bodyFont, Action onSaved, Action onCancel)
        {
            _displayFont = displayFont;
            _bodyFont = bodyFont;
            _onSaved = onSaved;
            _onCancel = onCancel;

            _nameInput = FindDescendant(transform, "Pilot Name Input")?.GetComponent<InputField>();
            _previewName = FindDescendant(transform, "Profile Preview Name")?.GetComponent<Text>();
            _previewAvatarName = FindDescendant(transform, "Profile Preview Avatar Name")?.GetComponent<Text>();
            _previewAvatar = FindDescendant(transform, "Profile Preview Avatar")?.GetComponent<PilotAvatarGraphic>();
            _options = GetComponentsInChildren<PilotAvatarOption>(true);

            foreach (var text in GetComponentsInChildren<Text>(true))
            {
                text.font = text.fontStyle == FontStyle.Bold ? _displayFont : _bodyFont;
            }

            if (_nameInput != null)
            {
                _nameInput.onValueChanged.RemoveAllListeners();
                _nameInput.onValueChanged.AddListener(OnNameChanged);
                _nameInput.onValidateInput = PilotProfileService.ValidateCharacter;
            }

            foreach (var option in _options)
            {
                int avatarId = option.AvatarId;
                option.Button.onClick.RemoveAllListeners();
                option.Button.onClick.AddListener(() => SelectAvatar(avatarId));
            }

            BindButton("Save Profile Button", SaveProfile);
            BindButton("Cancel Profile Button", Cancel);
            Prepare();
        }

        public void Prepare()
        {
            _selectedAvatarId = PilotProfileService.AvatarId;
            if (_nameInput != null) _nameInput.SetTextWithoutNotify(PilotProfileService.PilotName);
            RefreshPreview(PilotProfileService.PilotName);
            UpdateOptions();
        }

        private void Build()
        {
            CreateIcon(transform, "Profile Header Icon", MenuIconType.Credits, new Vector2(58f, -48f), new Vector2(50f, 50f), _accentColor);
            CreateText(transform, "Profile Editor Title", 42, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(126f, -40f), new Vector2(600f, 58f), "PERFIL DEL PILOTO");
            CreateText(transform, "Profile Breadcrumb", 13, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(128f, -98f), new Vector2(720f, 28f), "MENÚ  /  IDENTIDAD DEL PILOTO").color = _mutedTextColor;
            CreateImage(transform, "Profile Header Line", new Vector2(58f, -144f), new Vector2(1064f, 3f), new Color(_accentColor.r, _accentColor.g, _accentColor.b, 0.50f));

            var previewCard = CreateRounded(transform, "Profile Preview Card", new Vector2(58f, -180f), new Vector2(350f, 500f), new Color(0.02f, 0.105f, 0.16f, 0.96f), new Color(0.01f, 0.055f, 0.09f, 0.98f), 24f, new Color(0.24f, 0.68f, 0.92f, 0.18f), 1f);
            CreateText(previewCard.transform, "Preview Label", 13, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(30f, -28f), new Vector2(290f, 28f), "VISTA PREVIA").color = _accentColor;
            var avatarFrame = CreateRounded(previewCard.transform, "Preview Avatar Frame", new Vector2(55f, -78f), new Vector2(240f, 240f), new Color(0.012f, 0.07f, 0.11f, 1f), new Color(0.02f, 0.15f, 0.22f, 1f), 120f, new Color(0.25f, 0.78f, 1f, 0.34f), 2f);
            _previewAvatar = CreateAvatar(avatarFrame.transform, "Profile Preview Avatar", 0, Vector2.zero, new Vector2(210f, 210f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            _previewName = CreateText(previewCard.transform, "Profile Preview Name", 24, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(30f, -336f), new Vector2(290f, 38f), "AEROBYTE");
            _previewAvatarName = CreateText(previewCard.transform, "Profile Preview Avatar Name", 12, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(30f, -378f), new Vector2(290f, 24f), "CÓNDOR");
            _previewAvatarName.color = _accentColor;
            CreateText(previewCard.transform, "Profile Status", 11, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(30f, -438f), new Vector2(290f, 22f), "LISTO PARA VOLAR").color = new Color(0.34f, 0.94f, 0.66f, 1f);

            CreateText(transform, "Pilot Name Label", 14, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(452f, -180f), new Vector2(360f, 28f), "NOMBRE DE USUARIO");
            CreateText(transform, "Pilot Name Hint", 11, FontStyle.Normal, TextAnchor.MiddleRight, new Vector2(870f, -180f), new Vector2(252f, 28f), "MÁXIMO 16 CARACTERES").color = _mutedTextColor;
            _nameInput = CreateNameInput(transform, new Vector2(452f, -222f), new Vector2(670f, 64f));

            CreateText(transform, "Avatar Selection Label", 14, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(452f, -318f), new Vector2(450f, 28f), "SELECCIONA TU AVATAR");
            CreateText(transform, "Avatar Selection Hint", 11, FontStyle.Normal, TextAnchor.MiddleRight, new Vector2(900f, -318f), new Vector2(222f, 28f), "6 ESTILOS DISPONIBLES").color = _mutedTextColor;

            for (int i = 0; i < 6; i++)
            {
                int column = i % 3;
                int row = i / 3;
                CreateAvatarOption(transform, i, new Vector2(452f + column * 226f, -360f - row * 176f));
            }

            CreateActionButton(transform, "Cancel Profile", "CANCELAR", MenuIconType.Back, new Vector2(452f, -724f), new Vector2(280f, 66f), false, Cancel);
            CreateActionButton(transform, "Save Profile", "GUARDAR PERFIL", MenuIconType.Credits, new Vector2(842f, -724f), new Vector2(280f, 66f), true, SaveProfile);
        }

        private InputField CreateNameInput(Transform parent, Vector2 position, Vector2 size)
        {
            var inputObject = new GameObject("Pilot Name Input", typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuRoundedGraphic), typeof(InputField));
            inputObject.transform.SetParent(parent, false);
            SetRect(inputObject.GetComponent<RectTransform>(), position, size, new Vector2(0f, 1f), new Vector2(0f, 1f));
            var background = inputObject.GetComponent<MenuRoundedGraphic>();
            background.SetStyle(new Color(0.018f, 0.13f, 0.20f, 0.98f), new Color(0.008f, 0.065f, 0.11f, 1f), 14f, new Color(0.22f, 0.72f, 0.96f, 0.32f), 1f);

            var placeholder = CreateText(inputObject.transform, "Placeholder", 17, FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(24f, 0f), new Vector2(size.x - 48f, size.y), "ESCRIBE TU NOMBRE...", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            placeholder.color = new Color(0.45f, 0.58f, 0.66f, 0.75f);
            var valueText = CreateText(inputObject.transform, "Input Text", 18, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(24f, 0f), new Vector2(size.x - 48f, size.y), string.Empty, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));

            var input = inputObject.GetComponent<InputField>();
            input.targetGraphic = background;
            input.textComponent = valueText;
            input.placeholder = placeholder;
            input.characterLimit = 16;
            input.lineType = InputField.LineType.SingleLine;
            input.contentType = InputField.ContentType.Standard;
            input.caretColor = Color.white;
            input.selectionColor = new Color(0.10f, 0.56f, 0.90f, 0.50f);
            return input;
        }

        private void CreateAvatarOption(Transform parent, int avatarId, Vector2 position)
        {
            var optionObject = new GameObject($"Avatar Option {avatarId}", typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuRoundedGraphic), typeof(Button), typeof(PilotAvatarOption));
            optionObject.transform.SetParent(parent, false);
            SetRect(optionObject.GetComponent<RectTransform>(), position, new Vector2(204f, 154f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            var background = optionObject.GetComponent<MenuRoundedGraphic>();
            background.SetStyle(new Color(0.018f, 0.11f, 0.17f, 0.96f), new Color(0.01f, 0.06f, 0.10f, 0.98f), 16f, new Color(0.20f, 0.64f, 0.86f, 0.18f), 1f);
            var button = optionObject.GetComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;

            var avatar = CreateAvatar(optionObject.transform, "Avatar", avatarId, new Vector2(62f, -14f), new Vector2(80f, 80f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            var label = CreateText(optionObject.transform, "Avatar Name", 12, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(12f, -106f), new Vector2(180f, 28f), PilotAvatarGraphic.GetAvatarName(avatarId));
            label.color = _mutedTextColor;
            optionObject.GetComponent<PilotAvatarOption>().Configure(avatarId, background, avatar, label);
        }

        private void CreateActionButton(Transform parent, string objectName, string label, MenuIconType iconType, Vector2 position, Vector2 size, bool primary, Action action)
        {
            Color normalTop = primary ? new Color(0.05f, 0.55f, 0.98f, 0.98f) : new Color(0.018f, 0.12f, 0.19f, 0.98f);
            Color normalBottom = primary ? new Color(0.02f, 0.31f, 0.70f, 1f) : new Color(0.008f, 0.06f, 0.10f, 1f);
            Color hoverTop = primary ? new Color(0.10f, 0.68f, 1f, 1f) : new Color(0.04f, 0.29f, 0.42f, 1f);
            Color hoverBottom = primary ? new Color(0.03f, 0.43f, 0.84f, 1f) : new Color(0.02f, 0.15f, 0.24f, 1f);
            var buttonObject = CreateRounded(parent, $"{objectName} Button", position, size, normalTop, normalBottom, 16f, new Color(0.30f, 0.78f, 1f, primary ? 0.44f : 0.20f), 1f);
            buttonObject.AddComponent<CanvasGroup>();
            var background = buttonObject.GetComponent<MenuRoundedGraphic>();
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => action());
            var icon = CreateIcon(buttonObject.transform, "Icon", iconType, new Vector2(26f, 0f), new Vector2(30f, 30f), Color.white, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            var text = CreateText(buttonObject.transform, "Label", 16, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(76f, 0f), new Vector2(size.x - 98f, size.y), label, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            text.raycastTarget = false;
            buttonObject.AddComponent<MenuButtonMotion>().Configure(background, icon, null, text, normalTop, normalBottom, hoverTop, hoverBottom, Color.white, Color.white, 0.05f);
        }

        private void SelectAvatar(int avatarId)
        {
            _selectedAvatarId = Mathf.Clamp(avatarId, 0, 5);
            RefreshPreview(_nameInput == null ? PilotProfileService.PilotName : _nameInput.text);
            UpdateOptions();
            MenuUiAudio.PlayClick();
        }

        private void OnNameChanged(string value) => RefreshPreview(value);

        private void RefreshPreview(string pilotName)
        {
            string sanitizedName = PilotProfileService.SanitizeName(pilotName);
            if (_previewName != null) _previewName.text = sanitizedName;
            if (_previewAvatar != null) _previewAvatar.AvatarId = _selectedAvatarId;
            if (_previewAvatarName != null) _previewAvatarName.text = PilotAvatarGraphic.GetAvatarName(_selectedAvatarId);
        }

        private void UpdateOptions()
        {
            if (_options == null) return;
            foreach (var option in _options) option.SetSelected(option.AvatarId == _selectedAvatarId);
        }

        private void SaveProfile()
        {
            PilotProfileService.Save(_nameInput == null ? PilotProfileService.PilotName : _nameInput.text, _selectedAvatarId);
            _onSaved?.Invoke();
        }

        private void Cancel()
        {
            Prepare();
            _onCancel?.Invoke();
        }

        private void BindButton(string objectName, Action action)
        {
            var button = FindDescendant(transform, objectName)?.GetComponent<Button>();
            if (button == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action());
        }

        private GameObject CreateRounded(Transform parent, string objectName, Vector2 position, Vector2 size, Color top, Color bottom, float radius, Color border, float borderWidth)
        {
            var roundedObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuRoundedGraphic));
            roundedObject.transform.SetParent(parent, false);
            SetRect(roundedObject.GetComponent<RectTransform>(), position, size, new Vector2(0f, 1f), new Vector2(0f, 1f));
            roundedObject.GetComponent<MenuRoundedGraphic>().SetStyle(top, bottom, radius, border, borderWidth);
            return roundedObject;
        }

        private PilotAvatarGraphic CreateAvatar(Transform parent, string objectName, int avatarId, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot)
        {
            var avatarObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(PilotAvatarGraphic));
            avatarObject.transform.SetParent(parent, false);
            SetRect(avatarObject.GetComponent<RectTransform>(), position, size, anchor, pivot);
            var avatar = avatarObject.GetComponent<PilotAvatarGraphic>();
            avatar.AvatarId = avatarId;
            avatar.raycastTarget = false;
            return avatar;
        }

        private MenuIconGraphic CreateIcon(Transform parent, string objectName, MenuIconType type, Vector2 position, Vector2 size, Color tint, Vector2? anchor = null, Vector2? pivot = null)
        {
            var iconObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuIconGraphic));
            iconObject.transform.SetParent(parent, false);
            SetRect(iconObject.GetComponent<RectTransform>(), position, size, anchor ?? new Vector2(0f, 1f), pivot ?? new Vector2(0f, 1f));
            var icon = iconObject.GetComponent<MenuIconGraphic>();
            icon.Configure(type, tint);
            return icon;
        }

        private Image CreateImage(Transform parent, string objectName, Vector2 position, Vector2 size, Color color)
        {
            var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            SetRect(imageObject.GetComponent<RectTransform>(), position, size, new Vector2(0f, 1f), new Vector2(0f, 1f));
            var image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

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
    }
}
