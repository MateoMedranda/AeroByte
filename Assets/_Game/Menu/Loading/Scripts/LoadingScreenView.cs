using System.Collections;
using AeroByte.Menu.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AeroByte.Menu.Loading
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class LoadingScreenView : MonoBehaviour
    {
        private const string BackgroundPath = "Assets/_Game/Menu/Art/Backgrounds/LOAD SCREENS/BG-LOADSCREEN.png";
        private const float MinimumDisplayTime = 8f;
        private const float TipInterval = 3.5f;
        private const float VisualProgressSpeed = 0.16f;
        private const float ReadyDisplayTime = 1f;

        private static readonly string[] Tips =
        {
            "Usa movimientos suaves. Las correcciones pequenas mantienen el avion estable.",
            "Reduce la potencia antes de descender para conservar una aproximacion controlada.",
            "El minimapa te ayuda a anticipar rutas, objetivos y zonas de riesgo.",
            "Cada entorno cambia la visibilidad y exige una estrategia de vuelo distinta.",
            "Una buena alineacion antes de aterrizar evita maniobras bruscas al final.",
            "Vigila la altitud y la velocidad: ambos datos trabajan siempre en conjunto.",
            "El clima puede alterar la respuesta del avion incluso en una ruta conocida.",
            "Planifica el siguiente giro antes de llegar al punto de referencia."
        };

        [SerializeField] private Texture2D loadingBackground;
        [SerializeField] private RectTransform progressFill;
        [SerializeField] private Text progressText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text destinationText;
        [SerializeField] private Text tipText;
        [SerializeField] private RectTransform spinner;

        private CanvasGroup _canvasGroup;
        private bool _isLoading;
        private int _tipIndex;
        private float _tipTimer;

        public bool IsLoading => _isLoading;

        public void Initialize(Font displayFont, Font bodyFont)
        {
#if UNITY_EDITOR
            if (loadingBackground == null)
            {
                loadingBackground = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(BackgroundPath);
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif

            _canvasGroup = GetComponent<CanvasGroup>();
            if (transform.Find("Loading Screen Content") == null) Build(displayFont, bodyFont);
        }

        public void SetImmediate(bool visible)
        {
            _canvasGroup ??= GetComponent<CanvasGroup>();
            gameObject.SetActive(visible);
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.blocksRaycasts = visible;
            _canvasGroup.interactable = visible;
            if (!visible) _isLoading = false;
        }

        public void BeginLoad(string sceneName)
        {
            if (_isLoading) return;
            gameObject.SetActive(true);
            StartCoroutine(LoadScene(sceneName));
        }

        private IEnumerator LoadScene(string sceneName)
        {
            _isLoading = true;
            _canvasGroup ??= GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
            destinationText.text = GetDestinationName(sceneName);
            statusText.text = "PREPARANDO RECURSOS DE VUELO";
            _tipIndex = Random.Range(0, Tips.Length);
            _tipTimer = 0f;
            tipText.text = Tips[_tipIndex];
            SetProgress(0f);

            // Let the loading overlay render before scene deserialization begins.
            yield return null;

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                statusText.text = "NO SE PUDO ENCONTRAR EL NIVEL";
                _isLoading = false;
                yield break;
            }

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;
            float startTime = Time.realtimeSinceStartup;
            float displayedProgress = 0f;

            while (true)
            {
                float dt = Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, 1f, dt * 3.2f);
                float actualProgress = Mathf.Clamp01(operation.progress / 0.9f);
                displayedProgress = Mathf.MoveTowards(displayedProgress, actualProgress, dt * VisualProgressSpeed);
                SetProgress(displayedProgress);
                UpdateStatus(displayedProgress);
                UpdateTip(dt);
                if (spinner != null) spinner.Rotate(0f, 0f, -100f * dt);

                bool minimumTimeElapsed = Time.realtimeSinceStartup - startTime >= MinimumDisplayTime;
                bool ready = operation.progress >= 0.9f && displayedProgress >= 0.995f;
                if (ready && minimumTimeElapsed) break;
                yield return null;
            }

            SetProgress(1f);
            statusText.text = "LISTO PARA DESPEGAR";
            yield return new WaitForSecondsRealtime(ReadyDisplayTime);
            operation.allowSceneActivation = true;
        }

        private void UpdateStatus(float progress)
        {
            statusText.text = progress switch
            {
                < 0.22f => "PREPARANDO RECURSOS DE VUELO",
                < 0.50f => "CARGANDO ESCENARIO Y OBJETOS",
                < 0.78f => "CONFIGURANDO AERONAVE Y SISTEMAS",
                _ => "VERIFICANDO RUTA DE VUELO"
            };
        }

        private void UpdateTip(float deltaTime)
        {
            _tipTimer += deltaTime;
            float fade = Mathf.Clamp01(Mathf.Min(_tipTimer / 0.28f, (TipInterval - _tipTimer) / 0.28f));
            Color color = tipText.color;
            color.a = fade;
            tipText.color = color;

            if (_tipTimer < TipInterval) return;
            _tipTimer = 0f;
            _tipIndex = (_tipIndex + 1) % Tips.Length;
            tipText.text = Tips[_tipIndex];
        }

        private void SetProgress(float value)
        {
            value = Mathf.Clamp01(value);
            if (progressFill != null)
            {
                progressFill.gameObject.SetActive(value > 0.001f);
                Vector2 max = progressFill.anchorMax;
                max.x = value;
                progressFill.anchorMax = max;
            }
            if (progressText != null) progressText.text = $"{Mathf.RoundToInt(value * 100f):00}%";
        }

        private void Build(Font displayFont, Font bodyFont)
        {
            var content = CreateRect(transform, "Loading Screen Content", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var background = CreateRawImage(content, "Loading Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
            background.texture = loadingBackground;
            background.raycastTarget = true;
            CreateImage(content, "Loading Cinematic Shade", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.005f, 0.02f, 0.035f, 0.38f));
            CreateImage(content, "Loading Bottom Gradient", Vector2.zero, new Vector2(1f, 0.48f), Vector2.zero, Vector2.zero, new Color(0.002f, 0.015f, 0.028f, 0.78f));

            var spinnerObject = new GameObject("Loading Spinner", typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuIconGraphic));
            spinnerObject.transform.SetParent(content, false);
            SetRect(spinnerObject.GetComponent<RectTransform>(), new Vector2(72f, -716f), new Vector2(42f, 42f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            spinnerObject.GetComponent<MenuIconGraphic>().Configure(MenuIconType.Project, new Color(0.24f, 0.78f, 1f, 1f), 2f);
            spinner = spinnerObject.GetComponent<RectTransform>();

            statusText = CreateText(content, "Loading Status", "PREPARANDO RECURSOS DE VUELO", 13, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(132f, -710f), new Vector2(520f, 24f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.32f, 0.80f, 1f, 1f), displayFont);
            destinationText = CreateText(content, "Loading Destination", "DESTINO", 34, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(132f, -740f), new Vector2(680f, 48f), new Vector2(0f, 1f), new Vector2(0f, 1f), Color.white, displayFont);

            var tipPanel = CreateRounded(content, "Loading Tip Panel", new Vector2(-72f, -674f), new Vector2(680f, 148f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Color(0.015f, 0.10f, 0.15f, 0.92f), new Color(0.004f, 0.035f, 0.06f, 0.96f), 20f, new Color(0.20f, 0.70f, 0.94f, 0.38f), 2f);
            CreateText(tipPanel.transform, "Tip Label", "CONSEJO DE VUELO", 13, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(30f, -20f), new Vector2(330f, 24f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.32f, 0.82f, 1f, 1f), displayFont);
            tipText = CreateText(tipPanel.transform, "Tip Text", Tips[0], 19, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(30f, -54f), new Vector2(620f, 78f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.90f, 0.96f, 1f, 1f), bodyFont);

            var barBackground = CreateRounded(content, "Loading Bar Background", new Vector2(72f, -884f), new Vector2(1776f, 22f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.005f, 0.025f, 0.04f, 0.96f), new Color(0.005f, 0.025f, 0.04f, 0.96f), 11f, new Color(0.22f, 0.66f, 0.88f, 0.30f), 1f);
            var fillObject = CreateRounded(barBackground.transform, "Loading Bar Fill", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Color(0.16f, 0.78f, 1f, 1f), new Color(0.02f, 0.40f, 0.86f, 1f), 9f, Color.clear, 0f);
            progressFill = fillObject.GetComponent<RectTransform>();
            progressFill.anchorMin = new Vector2(0f, 0f);
            progressFill.anchorMax = new Vector2(0f, 1f);
            progressFill.offsetMin = new Vector2(3f, 3f);
            progressFill.offsetMax = new Vector2(-3f, -3f);

            progressText = CreateText(content, "Loading Percentage", "00%", 28, FontStyle.Bold, TextAnchor.MiddleRight, new Vector2(-72f, -830f), new Vector2(180f, 42f), new Vector2(1f, 1f), new Vector2(1f, 1f), Color.white, displayFont);
            CreateText(content, "Loading Footer", "CARGANDO DATOS DEL ENTORNO Y SISTEMAS DE VUELO", 11, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(72f, -924f), new Vector2(700f, 24f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.48f, 0.65f, 0.74f, 1f), bodyFont);
        }

        private static string GetDestinationName(string sceneName)
        {
            return sceneName switch
            {
                "Beach" => "PLAYA / OPERACION LITORAL",
                "Ciudad" => "CIUDAD / RUTA URBANA",
                "Desert" => "DESIERTO / TRAVESIA ENTRE DUNAS",
                "Forest" => "BOSQUE / VUELO DE MONTANA",
                _ => sceneName.ToUpperInvariant()
            };
        }

        private static GameObject CreateRounded(Transform parent, string objectName, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot, Color top, Color bottom, float radius, Color border, float borderWidth)
        {
            var roundedObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(MenuRoundedGraphic));
            roundedObject.transform.SetParent(parent, false);
            SetRect(roundedObject.GetComponent<RectTransform>(), position, size, anchor, pivot);
            roundedObject.GetComponent<MenuRoundedGraphic>().SetStyle(top, bottom, radius, border, borderWidth);
            return roundedObject;
        }

        private static Text CreateText(Transform parent, string objectName, string value, int fontSize, FontStyle style, TextAnchor alignment, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot, Color color, Font font)
        {
            var textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            SetRect(textObject.GetComponent<RectTransform>(), position, size, anchor, pivot);
            var text = textObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.text = value;
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform CreateRect(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rectObject = new GameObject(objectName, typeof(RectTransform));
            rectObject.transform.SetParent(parent, false);
            var rect = rectObject.GetComponent<RectTransform>();
            SetStretchRect(rect, anchorMin, anchorMax, offsetMin, offsetMax);
            return rect;
        }

        private static Image CreateImage(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            SetStretchRect(imageObject.GetComponent<RectTransform>(), anchorMin, anchorMax, offsetMin, offsetMax);
            var image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static RawImage CreateRawImage(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(RawImage));
            imageObject.transform.SetParent(parent, false);
            SetStretchRect(imageObject.GetComponent<RectTransform>(), anchorMin, anchorMax, offsetMin, offsetMax);
            var image = imageObject.GetComponent<RawImage>();
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
