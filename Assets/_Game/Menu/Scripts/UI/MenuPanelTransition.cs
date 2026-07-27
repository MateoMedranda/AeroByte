using UnityEngine;

namespace AeroByte.Menu.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class MenuPanelTransition : MonoBehaviour
    {
        [SerializeField] private Vector2 basePosition;
        [SerializeField] private Vector2 offset = new Vector2(0f, 24f);
        [SerializeField] private bool configured;

        private float _visibility = 1f;
        private bool _targetVisible = true;

        public void Configure(Vector2 transitionOffset)
        {
            if (transform is not RectTransform rect) return;
            basePosition = rect.anchoredPosition;
            offset = transitionOffset;
            configured = true;
            EnsureCanvasGroup();
        }

        public void SetImmediate(bool visible)
        {
            if (visible && !gameObject.activeSelf) gameObject.SetActive(true);
            if (!TryGetParts(out var rect, out var group))
            {
                gameObject.SetActive(visible);
                return;
            }

            if (!configured)
            {
                basePosition = rect.anchoredPosition;
                configured = true;
            }

            _targetVisible = visible;
            _visibility = visible ? 1f : 0f;
            Apply(rect, group);
            group.blocksRaycasts = visible;
            group.interactable = visible;
            if (!visible) gameObject.SetActive(false);
        }

        public void Show(bool immediate = false)
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (!TryGetParts(out var rect, out var group)) return;
            if (!configured)
            {
                basePosition = rect.anchoredPosition;
                configured = true;
            }

            _targetVisible = true;
            group.blocksRaycasts = true;
            group.interactable = true;
            if (immediate || !Application.isPlaying)
            {
                _visibility = 1f;
            }
            else
            {
                _visibility = 0f;
            }
            Apply(rect, group);
        }

        public void Hide(bool immediate = false)
        {
            if (!TryGetParts(out var rect, out var group))
            {
                gameObject.SetActive(false);
                return;
            }

            _targetVisible = false;
            group.blocksRaycasts = false;
            group.interactable = false;
            if (immediate || !Application.isPlaying)
            {
                _visibility = 0f;
                Apply(rect, group);
                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (!Application.isPlaying || !configured || !TryGetParts(out var rect, out var group)) return;

            _visibility = Mathf.MoveTowards(_visibility, _targetVisible ? 1f : 0f, Time.unscaledDeltaTime * 5.2f);
            Apply(rect, group);
            if (!_targetVisible && _visibility <= 0.001f) gameObject.SetActive(false);
        }

        private void Apply(RectTransform rect, CanvasGroup group)
        {
            float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(_visibility), 3f);
            group.alpha = eased;
            rect.anchoredPosition = basePosition + offset * (1f - eased);
            rect.localScale = Vector3.one * Mathf.Lerp(0.985f, 1f, eased);
        }

        private bool TryGetParts(out RectTransform rect, out CanvasGroup group)
        {
            rect = transform as RectTransform;
            group = EnsureCanvasGroup();
            return rect != null && group != null;
        }

        private CanvasGroup EnsureCanvasGroup()
        {
            var group = GetComponent<CanvasGroup>();
            if (group == null && gameObject != null) group = gameObject.AddComponent<CanvasGroup>();
            return group;
        }
    }
}
