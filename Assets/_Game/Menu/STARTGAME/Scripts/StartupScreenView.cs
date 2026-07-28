using System.Collections;
using AeroByte.Menu.UI;
using UnityEngine;
using UnityEngine.UI;

namespace AeroByte.Menu.Startup
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class StartupScreenView : MonoBehaviour
    {
        private const float DisplayTime = 12f;
        private const float RevealDuration = 1.6f;

        private static bool _shownThisSession;

        [SerializeField] private Texture2D backgroundTexture;
        [SerializeField] private RectTransform spinner;
        [SerializeField] private Text loadingText;
        [SerializeField] private RectTransform artwork;
        [SerializeField] private RectTransform progressFill;
        [SerializeField] private RectTransform screenContent;

        private CanvasGroup _canvasGroup;
        private bool _isShowing;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetSession()
        {
            _shownThisSession = false;
        }

        public void Initialize(Font displayFont, Texture2D background)
        {
            backgroundTexture = background;
            _canvasGroup = GetComponent<CanvasGroup>();
            if (transform.Find("Startup Screen Content") == null) Build(displayFont);
            SetImmediate(false);
        }

        public void ShowOnce()
        {
            if (_isShowing) return;
            if (_shownThisSession)
            {
                SetImmediate(false);
                return;
            }

            _shownThisSession = true;
            gameObject.SetActive(true);
            StartCoroutine(PlayIntro());
        }

        public void SetImmediate(bool visible)
        {
            _canvasGroup ??= GetComponent<CanvasGroup>();
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.blocksRaycasts = visible;
            _canvasGroup.interactable = visible;
            if (!visible && !_isShowing) gameObject.SetActive(false);
        }

        private IEnumerator PlayIntro()
        {
            _isShowing = true;
            _canvasGroup ??= GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
            if (screenContent != null) screenContent.anchoredPosition = Vector2.zero;
            if (artwork != null) artwork.localScale = Vector3.one;
            double startTime = Time.realtimeSinceStartupAsDouble;

            while (true)
            {
                float elapsed = (float)(Time.realtimeSinceStartupAsDouble - startTime);
                if (elapsed >= DisplayTime) break;

                float deltaTime = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
                float progress = Mathf.Clamp01(elapsed / DisplayTime);
                if (spinner != null) spinner.Rotate(0f, 0f, -125f * deltaTime);
                if (loadingText != null)
                {
                    int dots = 1 + Mathf.FloorToInt(elapsed * 2f) % 3;
                    loadingText.text = "CARGANDO" + new string('.', dots);
                }
                if (progressFill != null)
                {
                    Vector2 anchorMax = progressFill.anchorMax;
                    anchorMax.x = Mathf.SmoothStep(0f, 1f, progress);
                    progressFill.anchorMax = anchorMax;
                }

                float revealStart = DisplayTime - RevealDuration;
                float revealProgress = Mathf.Clamp01((elapsed - revealStart) / RevealDuration);
                float easedReveal = revealProgress < 0.5f
                    ? 4f * revealProgress * revealProgress * revealProgress
                    : 1f - Mathf.Pow(-2f * revealProgress + 2f, 3f) * 0.5f;
                if (screenContent != null)
                {
                    float travel = ((RectTransform)transform).rect.height + 24f;
                    screenContent.anchoredPosition = Vector2.up * travel * easedReveal;
                }
                if (artwork != null)
                {
                    float scale = 1f + progress * 0.01f;
                    artwork.localScale = Vector3.one * scale;
                }
                yield return null;
            }

            _isShowing = false;
            SetImmediate(false);
        }

        private void Build(Font displayFont)
        {
            var content = CreateRect(transform, "Startup Screen Content", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            screenContent = content;
            var whiteBackground = CreateImage(content, "Startup White Background", Color.white);
            whiteBackground.raycastTarget = true;

            var artworkObject = new GameObject("Startup Artwork", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter));
            artworkObject.transform.SetParent(content, false);
            SetStretchRect(artworkObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var artworkImage = artworkObject.GetComponent<RawImage>();
            artworkImage.texture = backgroundTexture;
            artworkImage.color = Color.white;
            artworkImage.raycastTarget = false;
            artwork = artworkObject.GetComponent<RectTransform>();
            var fitter = artworkObject.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = backgroundTexture != null ? backgroundTexture.width / (float)backgroundTexture.height : 1.5f;

            var spinnerFrame = new GameObject("Startup Spinner Frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuRoundedGraphic));
            spinnerFrame.transform.SetParent(content, false);
            SetRect(spinnerFrame.GetComponent<RectTransform>(), Vector2.zero, new Vector2(104f, 104f), new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.5f));
            spinnerFrame.GetComponent<MenuRoundedGraphic>().SetStyle(new Color(0.96f, 0.98f, 1f, 0.96f), new Color(0.88f, 0.94f, 1f, 0.98f), 52f, new Color(0.05f, 0.35f, 0.70f, 0.42f), 2f);

            var spinnerObject = new GameObject("Startup Loading Spinner", typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuIconGraphic));
            spinnerObject.transform.SetParent(spinnerFrame.transform, false);
            SetRect(spinnerObject.GetComponent<RectTransform>(), Vector2.zero, new Vector2(62f, 62f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            spinnerObject.GetComponent<MenuIconGraphic>().Configure(MenuIconType.Settings, new Color(0.03f, 0.28f, 0.62f, 1f), 3f);
            spinner = spinnerObject.GetComponent<RectTransform>();

            var textObject = new GameObject("Startup Loading Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(content, false);
            SetRect(textObject.GetComponent<RectTransform>(), Vector2.zero, new Vector2(420f, 48f), new Vector2(0.5f, 0.095f), new Vector2(0.5f, 0.5f));
            loadingText = textObject.GetComponent<Text>();
            loadingText.font = displayFont;
            loadingText.fontSize = 24;
            loadingText.fontStyle = FontStyle.Bold;
            loadingText.alignment = TextAnchor.MiddleCenter;
            loadingText.color = new Color(0.03f, 0.22f, 0.48f, 1f);
            loadingText.text = "CARGANDO...";
            loadingText.raycastTarget = false;

            var progressTrack = new GameObject("Startup Progress Track", typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuRoundedGraphic));
            progressTrack.transform.SetParent(content, false);
            SetRect(progressTrack.GetComponent<RectTransform>(), Vector2.zero, new Vector2(360f, 10f), new Vector2(0.5f, 0.055f), new Vector2(0.5f, 0.5f));
            progressTrack.GetComponent<MenuRoundedGraphic>().SetStyle(new Color(0.80f, 0.87f, 0.94f, 1f), new Color(0.80f, 0.87f, 0.94f, 1f), 5f, Color.clear, 0f);

            var progressObject = new GameObject("Startup Progress Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuRoundedGraphic));
            progressObject.transform.SetParent(progressTrack.transform, false);
            progressFill = progressObject.GetComponent<RectTransform>();
            progressFill.anchorMin = Vector2.zero;
            progressFill.anchorMax = new Vector2(0f, 1f);
            progressFill.offsetMin = Vector2.zero;
            progressFill.offsetMax = Vector2.zero;
            progressObject.GetComponent<MenuRoundedGraphic>().SetStyle(new Color(0.08f, 0.48f, 0.92f, 1f), new Color(0.02f, 0.24f, 0.64f, 1f), 5f, Color.clear, 0f);

            var revealLine = new GameObject("Startup Reveal Line", typeof(RectTransform), typeof(Image));
            revealLine.transform.SetParent(content, false);
            var revealLineRect = revealLine.GetComponent<RectTransform>();
            revealLineRect.anchorMin = new Vector2(0f, 0f);
            revealLineRect.anchorMax = new Vector2(1f, 0f);
            revealLineRect.pivot = new Vector2(0.5f, 0f);
            revealLineRect.offsetMin = Vector2.zero;
            revealLineRect.offsetMax = new Vector2(0f, 7f);
            var revealImage = revealLine.GetComponent<Image>();
            revealImage.color = new Color(0.02f, 0.42f, 0.90f, 0.92f);
            revealImage.raycastTarget = false;
        }

        private static RectTransform CreateRect(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rectObject = new GameObject(objectName, typeof(RectTransform));
            rectObject.transform.SetParent(parent, false);
            var rect = rectObject.GetComponent<RectTransform>();
            SetStretchRect(rect, anchorMin, anchorMax, offsetMin, offsetMax);
            return rect;
        }

        private static Image CreateImage(Transform parent, string objectName, Color color)
        {
            var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            SetStretchRect(imageObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var image = imageObject.GetComponent<Image>();
            image.color = color;
            return image;
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
