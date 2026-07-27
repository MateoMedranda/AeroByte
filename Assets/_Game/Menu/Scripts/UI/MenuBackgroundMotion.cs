using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace AeroByte.Menu.UI
{
    public sealed class MenuBackgroundMotion : MonoBehaviour
    {
        private RectTransform _rect;
        private Vector3 _basePosition;
        private Vector2 _pointerOffset;

        private void Awake()
        {
            _rect = transform as RectTransform;
            _basePosition = _rect.localPosition;
            var graphic = GetComponent<Graphic>();
            if (graphic != null) graphic.raycastTarget = false;
        }

        private void Update()
        {
            float time = Time.unscaledTime;
            Vector2 targetOffset = Vector2.zero;
            if (Mouse.current != null && Screen.width > 0 && Screen.height > 0)
            {
                Vector2 normalizedPointer = Mouse.current.position.ReadValue() / new Vector2(Screen.width, Screen.height);
                targetOffset = (normalizedPointer - Vector2.one * 0.5f) * new Vector2(-14f, -8f);
            }
            _pointerOffset = Vector2.Lerp(_pointerOffset, targetOffset, 1f - Mathf.Exp(-Time.unscaledDeltaTime * 2.5f));
            float scale = 1.025f + Mathf.Sin(time * 0.18f) * 0.004f;
            _rect.localScale = Vector3.one * scale;
            _rect.localPosition = _basePosition + new Vector3(Mathf.Sin(time * 0.12f) * 5f + _pointerOffset.x, Mathf.Cos(time * 0.10f) * 2f + _pointerOffset.y, 0f);
        }
    }
}
