using System;
using AeroByte.Menu.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AeroByte.Menu.LevelSelection
{
    public sealed class LevelSelectionCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private string sceneName;
        [SerializeField] private int order;
        [SerializeField] private MenuRoundedGraphic frame;
        [SerializeField] private Graphic artwork;
        [SerializeField] private Graphic glow;
        [SerializeField] private Text title;
        [SerializeField] private Vector2 _basePosition;

        private RectTransform _rect;
        private CanvasGroup _group;
        private Button _button;
        private float _introTime;
        private float _hover;
        private float _press;
        private bool _hovered;
        private bool _pressed;

        public void Configure(string targetScene, int cardOrder, MenuRoundedGraphic cardFrame, Graphic cardArtwork, Graphic cardGlow, Text cardTitle)
        {
            sceneName = targetScene;
            order = cardOrder;
            frame = cardFrame;
            artwork = cardArtwork;
            glow = cardGlow;
            title = cardTitle;
            EnsureReferences();
            _basePosition = _rect.anchoredPosition;
            ApplyImmediate();
        }

        public void Bind(Action<string> onSelected)
        {
            EnsureReferences();
            _button.onClick.RemoveAllListeners();
            if (onSelected != null) _button.onClick.AddListener(() => onSelected(sceneName));
        }

        private void OnEnable()
        {
            EnsureReferences();
            if (!Application.isPlaying)
            {
                ApplyImmediate();
                return;
            }

            _introTime = -order * 0.07f;
            _group.alpha = 0f;
            _rect.anchoredPosition = _basePosition + Vector2.down * 42f;
            _rect.localScale = Vector3.one * 0.97f;
        }

        private void Update()
        {
            if (!Application.isPlaying || _rect == null) return;

            float dt = Time.unscaledDeltaTime;
            _introTime += dt;
            float intro = EaseOutCubic(Mathf.Clamp01(_introTime / 0.42f));
            _hover = Mathf.MoveTowards(_hover, _hovered ? 1f : 0f, dt * 6.5f);
            _press = Mathf.MoveTowards(_press, _pressed ? 1f : 0f, dt * 14f);
            float hover = Mathf.SmoothStep(0f, 1f, _hover);

            _group.alpha = intro;
            _rect.anchoredPosition = _basePosition + Vector2.down * (42f * (1f - intro)) + Vector2.up * (12f * hover);
            _rect.localScale = Vector3.one * (Mathf.Lerp(0.97f, 1f, intro) + 0.025f * hover - 0.018f * _press);
            frame.SetFill(Color.Lerp(new Color(0.025f, 0.09f, 0.14f, 0.98f), new Color(0.04f, 0.20f, 0.29f, 1f), hover), Color.Lerp(new Color(0.008f, 0.035f, 0.06f, 1f), new Color(0.015f, 0.10f, 0.16f, 1f), hover));
            artwork.color = Color.Lerp(new Color(0.86f, 0.91f, 0.94f, 1f), Color.white, hover);
            if (artwork is LevelCardArtwork vectorArtwork) vectorArtwork.SetFocus(hover);
            glow.color = new Color(0.15f, 0.72f, 1f, Mathf.Lerp(0.08f, 0.72f, hover));
            title.color = Color.Lerp(new Color(0.88f, 0.94f, 0.98f, 1f), Color.white, hover);
        }

        private void ApplyImmediate()
        {
            if (_rect == null) return;
            _group.alpha = 1f;
            _rect.anchoredPosition = _basePosition;
            _rect.localScale = Vector3.one;
            if (artwork != null) artwork.color = new Color(0.86f, 0.91f, 0.94f, 1f);
            if (artwork is LevelCardArtwork vectorArtwork) vectorArtwork.SetFocus(0f);
        }

        private void EnsureReferences()
        {
            _rect ??= transform as RectTransform;
            _group ??= GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            _button ??= GetComponent<Button>() ?? gameObject.AddComponent<Button>();
        }

        private static float EaseOutCubic(float value) => 1f - Mathf.Pow(1f - value, 3f);

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_hovered) MenuUiAudio.PlayHover();
            _hovered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            _pressed = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pressed = true;
            MenuUiAudio.PlayClick();
        }

        public void OnPointerUp(PointerEventData eventData) => _pressed = false;
        public void OnSelect(BaseEventData eventData) => _hovered = true;
        public void OnDeselect(BaseEventData eventData) => _hovered = false;
    }
}
