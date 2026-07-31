using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AeroByte.Menu.UI
{
    [RequireComponent(typeof(Slider))]
    public sealed class MenuSliderFocus : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        private Image _background;
        private Image _fill;
        private Image _handle;
        private Color _backgroundColor;
        private Color _fillColor;
        private bool _focused;

        private void Awake()
        {
            _background = transform.Find("Background")?.GetComponent<Image>();
            _fill = transform.Find("Fill Area/Fill")?.GetComponent<Image>();
            _handle = transform.Find("Handle Area/Handle")?.GetComponent<Image>();
            if (_background != null) _backgroundColor = _background.color;
            if (_fill != null) _fillColor = _fill.color;
        }

        private void Update()
        {
            if (_background == null || _fill == null || _handle == null) return;

            float target = _focused ? 1f : 0f;
            _background.color = Color.Lerp(_backgroundColor, new Color(0.05f, 0.28f, 0.42f, 0.95f), target);
            _fill.color = Color.Lerp(_fillColor, Color.white, target);
            _handle.color = Color.Lerp(new Color(0.93f, 0.97f, 1f, 1f), Color.white, target);
            _handle.rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, 1.18f, target);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _focused = true;
            MenuUiAudio.PlayHover();
        }

        public void OnPointerExit(PointerEventData eventData) => _focused = false;
        public void OnSelect(BaseEventData eventData) => _focused = true;
        public void OnDeselect(BaseEventData eventData) => _focused = false;
    }
}
