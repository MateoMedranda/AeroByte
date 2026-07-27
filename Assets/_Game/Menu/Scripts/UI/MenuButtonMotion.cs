using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AeroByte.Menu.UI
{
    public sealed class MenuButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private MenuRoundedGraphic _background;
        [SerializeField] private MenuIconGraphic _icon;
        [SerializeField] private Graphic _accent;
        [SerializeField] private Text _label;
        [SerializeField] private Color _normalTop;
        [SerializeField] private Color _normalBottom;
        [SerializeField] private Color _hoverTop;
        [SerializeField] private Color _hoverBottom;
        [SerializeField] private Color _normalText;
        [SerializeField] private Color _hoverText;
        [SerializeField] private Vector2 _basePosition;
        [SerializeField] private float _introDelay;
        [SerializeField] private bool _configured;
        private RectTransform _rect;
        private CanvasGroup _canvasGroup;
        private float _introTime;
        private float _hoverAmount;
        private float _pressAmount;
        private bool _hovered;
        private bool _pressed;

        public void Configure(MenuRoundedGraphic background, MenuIconGraphic icon, Graphic accent, Text label, Color normalTop, Color normalBottom, Color hoverTop, Color hoverBottom, Color normalText, Color hoverText, float introDelay)
        {
            _rect = transform as RectTransform;
            _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            _background = background;
            _icon = icon;
            _accent = accent;
            _label = label;
            _normalTop = normalTop;
            _normalBottom = normalBottom;
            _hoverTop = hoverTop;
            _hoverBottom = hoverBottom;
            _normalText = normalText;
            _hoverText = hoverText;
            _introDelay = introDelay;
            _basePosition = _rect.anchoredPosition;
            _configured = true;
            ResetVisuals();
        }

        private void OnEnable()
        {
            EnsureReferences();
            if (!_configured || !Application.isPlaying) return;
            _introTime = -_introDelay;
            _canvasGroup.alpha = 0f;
            _rect.anchoredPosition = _basePosition + Vector2.left * 28f;
        }

        private void Update()
        {
            EnsureReferences();
            if (!_configured || !Application.isPlaying) return;

            float dt = Time.unscaledDeltaTime;
            _introTime += dt;
            float intro = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_introTime / 0.32f));
            _canvasGroup.alpha = intro;

            _hoverAmount = Mathf.MoveTowards(_hoverAmount, _hovered ? 1f : 0f, dt * 7.5f);
            _pressAmount = Mathf.MoveTowards(_pressAmount, _pressed ? 1f : 0f, dt * 14f);
            float easedHover = Mathf.SmoothStep(0f, 1f, _hoverAmount);
            _rect.anchoredPosition = _basePosition + Vector2.left * (28f * (1f - intro)) + Vector2.right * (10f * easedHover);
            _rect.localScale = Vector3.one * (1f + 0.018f * easedHover - 0.025f * _pressAmount);

            _background.SetFill(Color.Lerp(_normalTop, _hoverTop, easedHover), Color.Lerp(_normalBottom, _hoverBottom, easedHover));
            if (_label != null) _label.color = Color.Lerp(_normalText, _hoverText, easedHover);
            if (_icon != null) _icon.color = Color.Lerp(_normalText, _hoverText, easedHover);
            if (_accent != null) _accent.color = new Color(_hoverText.r, _hoverText.g, _hoverText.b, Mathf.Lerp(0.08f, 1f, easedHover));
        }

        private void ResetVisuals()
        {
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
            if (_rect != null)
            {
                _rect.anchoredPosition = _basePosition;
                _rect.localScale = Vector3.one;
            }
            _background?.SetFill(_normalTop, _normalBottom);
            if (_label != null) _label.color = _normalText;
            if (_icon != null) _icon.color = _normalText;
        }

        private void EnsureReferences()
        {
            if (_rect == null) _rect = transform as RectTransform;
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_hovered) MenuUiAudio.PlayHover();
            _hovered = true;
        }
        public void OnPointerExit(PointerEventData eventData) { _hovered = false; _pressed = false; }
        public void OnPointerDown(PointerEventData eventData)
        {
            _pressed = true;
            MenuUiAudio.PlayClick();
        }
        public void OnPointerUp(PointerEventData eventData) => _pressed = false;
        public void OnSelect(BaseEventData eventData)
        {
            if (!_hovered) MenuUiAudio.PlayHover();
            _hovered = true;
        }
        public void OnDeselect(BaseEventData eventData) => _hovered = false;
    }
}
