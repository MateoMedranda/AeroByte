using System.Text;
using FlightSystem.Adapters;
using UnityEngine;
using UnityEngine.UI;

namespace AeroByte.UI_System
{
    public class AeroByteRuntimeHud : MonoBehaviour
    {
        private static AeroByteRuntimeHud _instance;

        private sealed class DialUi
        {
            public RectTransform root;
            public RectTransform needle;
            public Text title;
            public Text value;
            public float minValue;
            public float maxValue;
            public float startAngle;
            public float endAngle;
            public bool wrap;
        }

        private PlaneController _plane;
        private CameraController _cameraController;

        private Canvas _canvas;
        private Text _flightText;
        private Text _warningText;
        private Text _attitudeText;
        private Text[] _compassLabels;
        private Text _leftPrimaryText;
        private Text _leftSecondaryText;
        private Text _speedUnitText;
        private Text _rightPrimaryText;
        private Text _altUnitText;
        private Text _altitudeAxisTitle;
        private RectTransform _altitudeTapeLine;
        private RectTransform _altitudeTapeCursor;
        private RectTransform[] _altitudeTapeTicks;
        private Text _headingText;
        private Text _centerPitchText;
        private Text _centerHorizonText;
        private RectTransform _centerMarker;
        private RectTransform _centerBracketFrame;
        private RectTransform _horizonLine;
        private RectTransform _centerLadder;
        private RectTransform _leftTapeNeedle;
        private RectTransform _rightTapeNeedle;
        private DialUi _speedDial;
        private DialUi _altDial;
        private DialUi _headingDial;
        private DialUi _gDial;

        private Image _oobOverlayImage;
        private Text _oobCenterText;

        private static Sprite _panelSprite;
        private static Texture2D _panelTexture;
        private static Sprite _dialSprite;
        private static Texture2D _dialTexture;
        private static Sprite _circleSprite;
        private static Texture2D _circleTexture;
        private static Font _hudFont;

        private readonly StringBuilder _builder = new StringBuilder(256);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;

            var go = new GameObject("AeroByteRuntimeHud");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<AeroByteRuntimeHud>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            BuildHud();
        }

        private void Update()
        {
            if (_plane == null)
            {
                _plane = Object.FindFirstObjectByType<PlaneController>();
            }

            if (_cameraController == null)
            {
                _cameraController = Object.FindFirstObjectByType<CameraController>();
            }

            UpdateHud();
        }

        private void BuildHud()
        {
            var canvasGo = new GameObject("HUD Canvas");
            canvasGo.transform.SetParent(transform, false);

            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 5000;

            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var font = GetHudFont();

            var bottomDialStrip = CreatePanel(canvasGo.transform, "Bottom Dial Strip", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-210f, 18f), new Vector2(210f, 150f), new Color(0.45f, 0.45f, 0.45f, 0.30f));
            _speedDial = CreateDial(bottomDialStrip.transform, "Velocidad", font, new Vector2(-100f, 0f), new Color(0.18f, 1f, 0.18f, 1f), 0f, 500f, 225f, -45f, false, new Color(0.18f, 1f, 0.18f, 1f));
            _altDial = CreateDial(bottomDialStrip.transform, "Altitud", font, new Vector2(0f, 0f), new Color(0.18f, 1f, 0.18f, 1f), 0f, 3000f, 225f, -45f, false, new Color(0.18f, 1f, 0.18f, 1f));
            _headingDial = CreateDial(bottomDialStrip.transform, "Rumbo", font, new Vector2(100f, 0f), new Color(0.18f, 1f, 0.18f, 1f), 0f, 360f, 225f, -135f, true, new Color(0.18f, 1f, 0.18f, 1f));

            var compassPanel = CreatePanel(canvasGo.transform, "Compass Panel", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-400f, -60f), new Vector2(400f, -12f), new Color(0.45f, 0.45f, 0.45f, 0.22f));
            _compassLabels = new Text[7];
            for (int i = 0; i < _compassLabels.Length; i++)
            {
                var label = CreateText(compassPanel.transform, $"Compass Label {i}", font, 18, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, new Color(0.15f, 1f, 0.15f, 0.95f));
                var rect = label.rectTransform;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(60f, 24f);
                rect.anchoredPosition = new Vector2((i - 3) * 88f, 0f);
                _compassLabels[i] = label;
            }

            var compassBug = CreatePanel(compassPanel.transform, "Compass Bug", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-56f, -16f), new Vector2(56f, 16f), new Color(0f, 0f, 0f, 0.78f));
            var compassBugImage = compassBug.GetComponent<Image>();
            compassBugImage.color = new Color(0f, 0f, 0f, 0.22f);
            _headingText = CreateText(compassBug.transform, "Heading Text", font, 24, TextAnchor.MiddleCenter, new Vector2(8f, 2f), new Vector2(-8f, -2f), new Color(0.18f, 1f, 0.18f, 1f));
            _headingText.rectTransform.anchorMin = new Vector2(0f, 0f);
            _headingText.rectTransform.anchorMax = new Vector2(1f, 1f);
            _headingText.rectTransform.sizeDelta = Vector2.zero;
            _headingText.rectTransform.anchoredPosition = Vector2.zero;

            var leftTape = CreatePanel(canvasGo.transform, "Left Tape", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(14f, -120f), new Vector2(142f, 120f), new Color(0.38f, 0.38f, 0.38f, 0.28f));
            _leftPrimaryText = CreateText(leftTape.transform, "Left Primary", font, 62, TextAnchor.MiddleCenter, new Vector2(6f, 30f), new Vector2(-6f, 2f), new Color(0.15f, 1f, 0.15f, 1f));
            _leftPrimaryText.horizontalOverflow = HorizontalWrapMode.Overflow;
            _leftPrimaryText.verticalOverflow = VerticalWrapMode.Overflow;
            _speedUnitText = CreateText(leftTape.transform, "Speed Unit", font, 18, TextAnchor.UpperCenter, new Vector2(6f, -4f), new Vector2(-6f, -38f), new Color(0.15f, 1f, 0.15f, 0.95f));
            _speedUnitText.horizontalOverflow = HorizontalWrapMode.Overflow;
            _speedUnitText.verticalOverflow = VerticalWrapMode.Overflow;
            _leftSecondaryText = CreateText(leftTape.transform, "Left Secondary", font, 14, TextAnchor.LowerCenter, new Vector2(6f, 8f), new Vector2(-6f, -8f), new Color(0.15f, 1f, 0.15f, 0.95f));
            _leftSecondaryText.horizontalOverflow = HorizontalWrapMode.Overflow;
            _leftSecondaryText.verticalOverflow = VerticalWrapMode.Overflow;
            _leftTapeNeedle = CreateNeedle(leftTape.transform, new Vector2(0f, 0f), new Color(0f, 0f, 0f, 0f));

            var rightTape = new GameObject("Right Tape", typeof(RectTransform));
            rightTape.transform.SetParent(canvasGo.transform, false);
            var rightTapeRect = rightTape.GetComponent<RectTransform>();
            rightTapeRect.anchorMin = new Vector2(1f, 0.5f);
            rightTapeRect.anchorMax = new Vector2(1f, 0.5f);
            rightTapeRect.offsetMin = new Vector2(-178f, -220f);
            rightTapeRect.offsetMax = new Vector2(-14f, 220f);

            var altAxisLine = new GameObject("Alt Axis Line", typeof(RectTransform), typeof(Image));
            altAxisLine.transform.SetParent(rightTape.transform, false);
            var altAxisRect = altAxisLine.GetComponent<RectTransform>();
            altAxisRect.anchorMin = new Vector2(0.5f, 0.5f);
            altAxisRect.anchorMax = new Vector2(0.5f, 0.5f);
            altAxisRect.sizeDelta = new Vector2(4f, 320f);
            altAxisRect.anchoredPosition = new Vector2(-18f, 0f);
            var altAxisImage = altAxisLine.GetComponent<Image>();
            altAxisImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            altAxisImage.color = new Color(0.15f, 1f, 0.15f, 0.65f);
            _altitudeTapeLine = altAxisRect;

            _altitudeTapeTicks = new RectTransform[7];
            for (int i = 0; i < _altitudeTapeTicks.Length; i++)
            {
                var tick = new GameObject($"Alt Tick {i}", typeof(RectTransform), typeof(Image));
                tick.transform.SetParent(rightTape.transform, false);
                var tickRect = tick.GetComponent<RectTransform>();
                tickRect.anchorMin = new Vector2(0.5f, 0.5f);
                tickRect.anchorMax = new Vector2(0.5f, 0.5f);
                tickRect.sizeDelta = new Vector2(i == 3 ? 22f : 14f, 3f);
                tickRect.anchoredPosition = new Vector2(-18f + (i == 3 ? 12f : 8f), (3 - i) * 42f);
                var tickImg = tick.GetComponent<Image>();
                tickImg.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                tickImg.color = new Color(0.15f, 1f, 0.15f, 0.75f);
                _altitudeTapeTicks[i] = tickRect;
            }

            var altCursor = new GameObject("Alt Cursor", typeof(RectTransform), typeof(Image));
            altCursor.transform.SetParent(rightTape.transform, false);
            var altCursorRect = altCursor.GetComponent<RectTransform>();
            altCursorRect.anchorMin = new Vector2(0.5f, 0.5f);
            altCursorRect.anchorMax = new Vector2(0.5f, 0.5f);
            altCursorRect.sizeDelta = new Vector2(4f, 24f);
            altCursorRect.anchoredPosition = new Vector2(2f, 0f);
            var altCursorImg = altCursor.GetComponent<Image>();
            altCursorImg.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            altCursorImg.color = new Color(0.15f, 1f, 0.15f, 0.95f);
            _altitudeTapeCursor = altCursorRect;

            _rightPrimaryText = CreateText(rightTape.transform, "Right Primary", font, 44, TextAnchor.LowerRight, new Vector2(8f, 12f), new Vector2(-42f, -10f), new Color(0.15f, 1f, 0.15f, 1f));
            _rightPrimaryText.horizontalOverflow = HorizontalWrapMode.Overflow;
            _rightPrimaryText.verticalOverflow = VerticalWrapMode.Overflow;
            _altUnitText = CreateText(rightTape.transform, "Alt Unit", font, 18, TextAnchor.UpperCenter, new Vector2(14f, -36f), new Vector2(-14f, -58f), new Color(0.15f, 1f, 0.15f, 0.85f));
            _altUnitText.horizontalOverflow = HorizontalWrapMode.Overflow;
            _altUnitText.verticalOverflow = VerticalWrapMode.Overflow;
            _altitudeAxisTitle = CreateText(rightTape.transform, "Altitude Axis", font, 18, TextAnchor.UpperCenter, new Vector2(14f, -6f), new Vector2(-14f, -28f), new Color(0.15f, 1f, 0.15f, 0.9f));
            _altitudeAxisTitle.text = "ALTITUDE";
            _rightTapeNeedle = CreateNeedle(rightTape.transform, new Vector2(-18f, 0f), new Color(0f, 0f, 0f, 0f));

            var attitudeFrame = new GameObject("Attitude Frame", typeof(RectTransform));
            attitudeFrame.transform.SetParent(canvasGo.transform, false);
            var attitudeRect = attitudeFrame.GetComponent<RectTransform>();
            attitudeRect.anchorMin = new Vector2(0.5f, 0.5f);
            attitudeRect.anchorMax = new Vector2(0.5f, 0.5f);
            attitudeRect.sizeDelta = new Vector2(560f, 380f);
            attitudeRect.anchoredPosition = new Vector2(0f, -15f);
            attitudeFrame.AddComponent<RectMask2D>();

            var centerMotionRoot = new GameObject("Center Motion", typeof(RectTransform), typeof(RectMask2D));
            centerMotionRoot.transform.SetParent(attitudeFrame.transform, false);
            var centerMotionRect = centerMotionRoot.GetComponent<RectTransform>();
            centerMotionRect.anchorMin = new Vector2(0.5f, 0.5f);
            centerMotionRect.anchorMax = new Vector2(0.5f, 0.5f);
            centerMotionRect.sizeDelta = new Vector2(440f, 430f);
            centerMotionRect.anchoredPosition = Vector2.zero;

            _centerLadder = CreatePitchLadder(centerMotionRoot.transform, font);

            var horizonGo = new GameObject("Horizon Line", typeof(RectTransform), typeof(Image));
            horizonGo.transform.SetParent(centerMotionRoot.transform, false);
            var horizonRect = horizonGo.GetComponent<RectTransform>();
            horizonRect.anchorMin = new Vector2(0.5f, 0.5f);
            horizonRect.anchorMax = new Vector2(0.5f, 0.5f);
            horizonRect.sizeDelta = new Vector2(120f, 6f);
            horizonRect.anchoredPosition = Vector2.zero;
            var horizonImage = horizonGo.GetComponent<Image>();
            horizonImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            horizonImage.color = new Color(0.2f, 1f, 0.2f, 0.5f);
            _horizonLine = horizonRect;

            var bracketsRoot = new GameObject("Center Brackets", typeof(RectTransform));
            bracketsRoot.transform.SetParent(attitudeFrame.transform, false);
            var bracketsRect = bracketsRoot.GetComponent<RectTransform>();
            bracketsRect.anchorMin = new Vector2(0.5f, 0.5f);
            bracketsRect.anchorMax = new Vector2(0.5f, 0.5f);
            bracketsRect.sizeDelta = new Vector2(440f, 430f);
            bracketsRect.anchoredPosition = Vector2.zero;
            _centerBracketFrame = bracketsRect;

            CreateCenterBracket(bracketsRoot.transform, "Left", -225f);
            CreateCenterBracket(bracketsRoot.transform, "Right", 225f);

            _attitudeText = CreateText(attitudeFrame.transform, "Attitude Text", font, 15, TextAnchor.MiddleCenter, new Vector2(10f, 6f), new Vector2(-10f, -10f), new Color(0.15f, 1f, 0.15f, 0.95f));
            _attitudeText.alignment = TextAnchor.LowerCenter;

            var reticleGo = new GameObject("Reticle", typeof(RectTransform), typeof(Image));
            reticleGo.transform.SetParent(attitudeFrame.transform, false);
            var reticleRect = reticleGo.GetComponent<RectTransform>();
            reticleRect.anchorMin = new Vector2(0.5f, 0.5f);
            reticleRect.anchorMax = new Vector2(0.5f, 0.5f);
            reticleRect.sizeDelta = new Vector2(32f, 32f);
            var reticleImage = reticleGo.GetComponent<Image>();
            reticleImage.sprite = GetCircleSprite();
            reticleImage.type = Image.Type.Simple;
            reticleImage.color = new Color(0.15f, 1f, 0.15f, 1f);
            reticleGo.transform.SetAsLastSibling();
            _centerMarker = reticleRect;

            _centerPitchText = CreateText(attitudeFrame.transform, "Pitch Text", font, 13, TextAnchor.LowerCenter, new Vector2(10f, 10f), new Vector2(-10f, 40f), new Color(0.15f, 1f, 0.15f, 0.82f));
            _centerHorizonText = CreateText(attitudeFrame.transform, "Horizon Info", font, 12, TextAnchor.UpperCenter, new Vector2(10f, 12f), new Vector2(-10f, -120f), new Color(0.15f, 1f, 0.15f, 0.70f));

            var flightPanel = CreatePanel(canvasGo.transform, "Flight Panel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -220f), new Vector2(330f, -18f), new Color(0f, 0f, 0f, 0f));
            _flightText = CreateText(flightPanel.transform, "Flight Text", font, 14, TextAnchor.UpperLeft, new Vector2(10f, 10f), new Vector2(-10f, -10f), new Color(0.88f, 1f, 0.88f, 1f));

            var warningPanel = CreatePanel(canvasGo.transform, "Warning Panel", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-340f, -18f), new Vector2(-18f, -120f), new Color(0.45f, 0.45f, 0.45f, 0.24f));
            _warningText = CreateText(warningPanel.transform, "Warning Text", font, 16, TextAnchor.UpperLeft, new Vector2(10f, 10f), new Vector2(-10f, -10f), new Color(0.15f, 1f, 0.15f, 0.95f));

            var miniMapPanel = CreatePanel(canvasGo.transform, "MiniMap Panel", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(10f, 10f), new Vector2(350f, 350f), new Color(0.38f, 0.38f, 0.38f, 0.22f));
            miniMapPanel.AddComponent<AeroByteMiniMap>();

            // OOB Overlay
            var oobOverlay = new GameObject("OOB Overlay", typeof(RectTransform), typeof(Image));
            oobOverlay.transform.SetParent(canvasGo.transform, false);
            var overlayRect = oobOverlay.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            _oobOverlayImage = oobOverlay.GetComponent<Image>();
            _oobOverlayImage.color = new Color(1f, 0f, 0f, 0f); // Hidden by default
            _oobOverlayImage.raycastTarget = false;

            // OOB Center Text
            _oobCenterText = CreateText(canvasGo.transform, "OOB Text", font, 42, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, new Color(1f, 0.15f, 0.15f, 0f));
            var oobTextRect = _oobCenterText.GetComponent<RectTransform>();
            oobTextRect.anchorMin = new Vector2(0f, 0.5f);
            oobTextRect.anchorMax = new Vector2(1f, 0.5f);
            oobTextRect.sizeDelta = new Vector2(0f, 100f);
            oobTextRect.anchoredPosition = new Vector2(0f, 150f);
            
            // Outline for visibility
            var oobOutline = _oobCenterText.gameObject.AddComponent<Outline>();
            oobOutline.effectColor = Color.black;
            oobOutline.effectDistance = new Vector2(2f, -2f);
        }

        private static Font GetHudFont()
        {
            if (_hudFont != null)
            {
                return _hudFont;
            }

            _hudFont = Font.CreateDynamicFontFromOSFont(new[] { "OCR A Extended", "Consolas", "Lucida Console", "Courier New" }, 18);
            if (_hudFont == null)
            {
                _hudFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            return _hudFont;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            var image = go.GetComponent<Image>();
            image.sprite = GetPanelSprite();
            image.type = Image.Type.Sliced;
            image.color = color;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.18f, 0.18f, 0.18f, 0.65f);
            outline.effectDistance = new Vector2(2f, -2f);

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.12f, 0.12f, 0.12f, 0.30f);
            shadow.effectDistance = new Vector2(3f, -3f);

            return go;
        }

        private static Sprite GetPanelSprite()
        {
            if (_panelSprite != null)
            {
                return _panelSprite;
            }

            const int size = 128;
            const int radius = 28;

            _panelTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "AeroBytePanelTexture"
            };

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inside = IsPointInRoundedRect(x + 0.5f, y + 0.5f, size, radius);
                    pixels[y * size + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
                }
            }

            _panelTexture.SetPixels32(pixels);
            _panelTexture.Apply(false, false);

            _panelSprite = Sprite.Create(
                _panelTexture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size,
                0,
                SpriteMeshType.FullRect,
                new Vector4(radius, radius, radius, radius));
            _panelSprite.name = "AeroBytePanelSprite";
            return _panelSprite;
        }

        private static bool IsPointInRoundedRect(float x, float y, int size, int radius)
        {
            float min = radius;
            float max = size - radius;

            if (x >= min && x <= max)
            {
                return true;
            }

            if (y >= min && y <= max)
            {
                return true;
            }

            float cx = x < min ? min : max;
            float cy = y < min ? min : max;
            float dx = x - cx;
            float dy = y - cy;
            return dx * dx + dy * dy <= radius * radius;
        }

        private static Sprite GetDialSprite()
        {
            if (_dialSprite != null)
            {
                return _dialSprite;
            }

            const int size = 128;
            const int radius = 54;

            _dialTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "AeroByteDialTexture"
            };

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inside = IsPointInRoundedRect(x + 0.5f, y + 0.5f, size, radius);
                    pixels[y * size + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
                }
            }

            _dialTexture.SetPixels32(pixels);
            _dialTexture.Apply(false, false);

            _dialSprite = Sprite.Create(
                _dialTexture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size,
                0,
                SpriteMeshType.FullRect,
                new Vector4(radius, radius, radius, radius));
            _dialSprite.name = "AeroByteDialSprite";
            return _dialSprite;
        }

        private static Sprite GetCircleSprite()
        {
            if (_circleSprite != null)
            {
                return _circleSprite;
            }

            const int size = 64;
            const float radius = 28f;

            _circleTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "AeroByteCircleTexture"
            };

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - (size - 1) * 0.5f;
                    float dy = y - (size - 1) * 0.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    byte alpha = (byte)(dist <= radius ? 255 : 0);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            _circleTexture.SetPixels32(pixels);
            _circleTexture.Apply(false, false);

            _circleSprite = Sprite.Create(
                _circleTexture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size,
                0,
                SpriteMeshType.FullRect,
                new Vector4(16f, 16f, 16f, 16f));
            _circleSprite.name = "AeroByteCircleSprite";
            return _circleSprite;
        }

        private static Text CreateText(Transform parent, string name, Font font, int fontSize, TextAnchor anchor, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            var text = go.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.resizeTextForBestFit = false;
            return text;
        }

        private static RectTransform CreateNeedle(Transform parent, Vector2 anchoredPosition, Color color)
        {
            var go = new GameObject("Needle", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(26f, 4f);
            rect.anchoredPosition = anchoredPosition;
            var img = go.GetComponent<Image>();
            img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            img.color = color;
            return rect;
        }

        private static void CreateCenterBracket(Transform parent, string side, float x)
        {
            var line = new GameObject($"Bracket {side} Line", typeof(RectTransform), typeof(Image));
            line.transform.SetParent(parent, false);
            var lineRect = line.GetComponent<RectTransform>();
            lineRect.anchorMin = new Vector2(0.5f, 0.5f);
            lineRect.anchorMax = new Vector2(0.5f, 0.5f);
            lineRect.sizeDelta = new Vector2(4f, 340f);
            lineRect.anchoredPosition = new Vector2(x, 0f);
            var lineImage = line.GetComponent<Image>();
            lineImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            lineImage.color = new Color(0.15f, 1f, 0.15f, 0.95f);
        }

        private static void CreateCenterBracketCap(Transform parent, string name, Vector2 position, bool leftSide)
        {
            var cap = new GameObject(name, typeof(RectTransform), typeof(Image));
            cap.transform.SetParent(parent, false);
            var capRect = cap.GetComponent<RectTransform>();
            capRect.anchorMin = new Vector2(0.5f, 0.5f);
            capRect.anchorMax = new Vector2(0.5f, 0.5f);
            capRect.sizeDelta = new Vector2(24f, 4f);
            capRect.anchoredPosition = position;
            var capImage = cap.GetComponent<Image>();
            capImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            capImage.color = new Color(0.15f, 1f, 0.15f, 0.95f);

            var stub = new GameObject($"{name} Stub", typeof(RectTransform), typeof(Image));
            stub.transform.SetParent(parent, false);
            var stubRect = stub.GetComponent<RectTransform>();
            stubRect.anchorMin = new Vector2(0.5f, 0.5f);
            stubRect.anchorMax = new Vector2(0.5f, 0.5f);
            stubRect.sizeDelta = new Vector2(4f, 28f);
            stubRect.anchoredPosition = new Vector2(position.x + (leftSide ? 12f : -12f), position.y + (position.y > 0f ? -12f : 12f));
            var stubImage = stub.GetComponent<Image>();
            stubImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            stubImage.color = new Color(0.15f, 1f, 0.15f, 0.95f);
        }

        private static DialUi CreateDial(Transform parent, string label, Font font, Vector2 anchoredPosition, Color tint, float minValue, float maxValue, float startAngle, float endAngle, bool wrap, Color textColor)
        {
            var go = new GameObject($"Dial {label}", typeof(RectTransform), typeof(Image), typeof(Mask));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(82f, 82f);
            rect.anchoredPosition = anchoredPosition;

            var img = go.GetComponent<Image>();
            img.sprite = GetDialSprite();
            img.type = Image.Type.Simple;
            img.color = new Color(0.42f, 0.42f, 0.42f, 0.34f);

            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);

            var ring = new GameObject("Ring", typeof(RectTransform), typeof(Image));
            ring.transform.SetParent(go.transform, false);
            var ringRect = ring.GetComponent<RectTransform>();
            ringRect.anchorMin = Vector2.zero;
            ringRect.anchorMax = Vector2.one;
            ringRect.offsetMin = new Vector2(5f, 5f);
            ringRect.offsetMax = new Vector2(-5f, -5f);
            var ringImg = ring.GetComponent<Image>();
            ringImg.sprite = GetDialSprite();
            ringImg.type = Image.Type.Simple;
            ringImg.color = new Color(0.58f, 0.58f, 0.58f, 0.22f);

            var needle = new GameObject("Needle", typeof(RectTransform), typeof(Image));
            needle.transform.SetParent(go.transform, false);
            var needleRect = needle.GetComponent<RectTransform>();
            needleRect.anchorMin = new Vector2(0.5f, 0.5f);
            needleRect.anchorMax = new Vector2(0.5f, 0.5f);
            needleRect.pivot = new Vector2(0.5f, 0.1f);
            needleRect.sizeDelta = new Vector2(3f, 36f);
            needleRect.anchoredPosition = new Vector2(0f, 1f);
            var needleImg = needle.GetComponent<Image>();
            needleImg.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            needleImg.color = tint;

            var cap = new GameObject("Cap", typeof(RectTransform), typeof(Image));
            cap.transform.SetParent(go.transform, false);
            var capRect = cap.GetComponent<RectTransform>();
            capRect.anchorMin = new Vector2(0.5f, 0.5f);
            capRect.anchorMax = new Vector2(0.5f, 0.5f);
            capRect.sizeDelta = new Vector2(12f, 12f);
            capRect.anchoredPosition = Vector2.zero;
            var capImg = cap.GetComponent<Image>();
            capImg.sprite = GetDialSprite();
            capImg.type = Image.Type.Simple;
            capImg.color = new Color(1f, 1f, 1f, 0.5f);

            var title = CreateText(parent, $"Title {label}", font, 15, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, textColor);
            title.transform.SetAsLastSibling();
            title.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            title.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            title.rectTransform.sizeDelta = new Vector2(84f, 18f);
            title.rectTransform.anchoredPosition = anchoredPosition + new Vector2(0f, 54f);
            title.text = label;
            title.horizontalOverflow = HorizontalWrapMode.Overflow;
            title.verticalOverflow = VerticalWrapMode.Overflow;
            if (label.Length > 8)
            {
                title.fontSize = 12;
            }

            var value = CreateText(parent, $"Value {label}", font, 22, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, textColor);
            value.transform.SetAsLastSibling();
            value.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            value.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            value.rectTransform.sizeDelta = new Vector2(54f, 20f);
            value.rectTransform.anchoredPosition = anchoredPosition + new Vector2(0f, -54f);

            return new DialUi
            {
                root = rect,
                needle = needleRect,
                title = title,
                value = value,
                minValue = minValue,
                maxValue = maxValue,
                startAngle = startAngle,
                endAngle = endAngle,
                wrap = wrap
            };
        }

        private static RectTransform CreatePitchLadder(Transform parent, Font font)
        {
            var root = new GameObject("Pitch Ladder", typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(parent, false);
            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.sizeDelta = new Vector2(440f, 2200f);

            for (int label = -180; label <= 180; label += 5)
            {
                if (label == 0)
                {
                    continue;
                }

                CreateLadderTick(root, font, label, -label * 6f);
            }

            return root;
        }

        private static void CreateLadderTick(RectTransform parent, Font font, int label, float y)
        {
            var left = new GameObject($"Left {label}", typeof(RectTransform), typeof(Image));
            left.transform.SetParent(parent, false);
            var leftRect = left.GetComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0.5f, 0.5f);
            leftRect.anchorMax = new Vector2(0.5f, 0.5f);
            leftRect.pivot = new Vector2(1f, 0.5f);
            leftRect.sizeDelta = new Vector2(92f, 3f);
            leftRect.anchoredPosition = new Vector2(-28f, y);
            var leftImg = left.GetComponent<Image>();
            leftImg.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            leftImg.color = new Color(0.2f, 1f, 0.2f, 0.9f);

            var right = new GameObject($"Right {label}", typeof(RectTransform), typeof(Image));
            right.transform.SetParent(parent, false);
            var rightRect = right.GetComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.5f, 0.5f);
            rightRect.anchorMax = new Vector2(0.5f, 0.5f);
            rightRect.pivot = new Vector2(0f, 0.5f);
            rightRect.sizeDelta = new Vector2(92f, 3f);
            rightRect.anchoredPosition = new Vector2(28f, y);
            var rightImg = right.GetComponent<Image>();
            rightImg.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            rightImg.color = new Color(0.2f, 1f, 0.2f, 0.9f);

            var labelGo = new GameObject($"Label {label}", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(parent, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(48f, 20f);
            labelRect.anchoredPosition = new Vector2(0f, y);
            var txt = labelGo.GetComponent<Text>();
            txt.font = font;
            txt.fontSize = 13;
            txt.color = new Color(0.2f, 1f, 0.2f, 1f);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = Mathf.Abs(label).ToString();
        }

        private void UpdateHud()
        {
            if (_flightText == null || _warningText == null || _attitudeText == null)
            {
                return;
            }

            if (_plane == null)
            {
                _flightText.text = "Waiting for plane...";
                _warningText.text = "HUD ready";
                _attitudeText.text = "";
                return;
            }

            var state = _plane.GetState();
            var rb = _plane.Body;

            float airspeed = state != null ? state.velocity.magnitude : rb.linearVelocity.magnitude;
            float forwardSpeed = state != null ? Mathf.Max(0f, state.localVelocity.z) : 0f;
            float verticalSpeed = rb.linearVelocity.y;
            AircraftAttitudeValidator.GetPitchRoll(_plane.transform, out float pitch, out float roll);
            float heading = GetHeading(_plane.transform.forward);
            float altitude = GetAltitudeAboveGround();
            float climb = rb.linearVelocity.y;
            float gForce = state != null ? state.localGForce.magnitude / 9.81f : 0f;
            float throttle = state != null ? state.throttle : 0f;
            float aoa = state != null ? state.angleOfAttack * Mathf.Rad2Deg : 0f;
            float aoaYaw = state != null ? state.angleOfAttackYaw * Mathf.Rad2Deg : 0f;
            Vector3 effectiveInput = state != null ? state.effectiveInput : Vector3.zero;

            UpdateCompass(heading);
            UpdateTapes(airspeed, altitude, aoa, gForce);
            UpdateCenterHud(pitch, roll, climb, heading);
            UpdateDial(_speedDial, airspeed * 3.6f, $"{Mathf.RoundToInt(airspeed * 3.6f)}");
            UpdateDial(_altDial, altitude, $"{Mathf.RoundToInt(altitude)}");
            UpdateDial(_headingDial, heading, $"{Mathf.RoundToInt(heading):000}");
            UpdateDial(_gDial, gForce, $"{gForce:0.0}G");

            _builder.Clear();
            
            if (MissionSystem.Adapters.AeroByteDeliveryManager.Instance != null)
            {
                var mgr = MissionSystem.Adapters.AeroByteDeliveryManager.Instance;
                _builder.AppendLine("MISSION STATUS");
                _builder.AppendFormat("Entregas: {0} / {1}\n", mgr.CurrentZoneIndex, mgr.TotalZones);
                if (mgr.CurrentZoneIndex >= mgr.TotalZones)
                    _builder.AppendLine("STATUS:   COMPLETED");
                else
                    _builder.AppendLine("STATUS:   ACTIVE (Press G for Map)");
            }

            _flightText.text = _builder.ToString();

            _builder.Clear();
            _builder.AppendLine("WARNINGS");
            bool hasWarnings = false;
            bool stallWarning = state != null && _plane.statsConfig != null && aoa > _plane.statsConfig.StallAngle * 0.8f;
            bool lowSpeed = airspeed < 30f;
            if (stallWarning)
            {
                _builder.AppendLine("STALL RISK");
                hasWarnings = true;
            }

            if (lowSpeed && state != null && !state.isGrounded)
            {
                _builder.AppendLine("LOW SPEED");
                hasWarnings = true;
            }

            if (_plane.IsInWeatherZone)
            {
                _builder.AppendLine("WEATHER ACTIVE");
                hasWarnings = true;
            }

            if (!hasWarnings)
            {
                _builder.AppendLine("OK");
            }

            _warningText.color = stallWarning ? new Color(1f, 0.35f, 0.2f, 1f) : new Color(1f, 0.85f, 0.2f, 1f);
            _warningText.text = _builder.ToString();

            // Animal Cargo & Out Of Bounds Logic
            bool isOOB = MissionSystem.Adapters.OutOfBoundsManager.Instance != null && MissionSystem.Adapters.OutOfBoundsManager.Instance.IsOOB;
            bool isAnimalStressed = MissionSystem.Adapters.AnimalCargoManager.Instance != null && MissionSystem.Adapters.AnimalCargoManager.Instance.IsAnimalStressed;

            if (isOOB || isAnimalStressed)
            {
                float timeLeft = 0f;
                string message = "";

                if (isOOB)
                {
                    timeLeft = MissionSystem.Adapters.OutOfBoundsManager.Instance.CurrentTimer;
                    message = "¡REGRESA AL ÁREA DE JUEGO!";
                }
                else if (isAnimalStressed)
                {
                    timeLeft = MissionSystem.Adapters.AnimalCargoManager.Instance.CurrentStressTimer;
                    message = "¡ALTITUD CRÍTICA! ANIMAL ESTRESADO";
                }
                
                // Pulsing red effect
                float pulse = (Mathf.Sin(Time.time * 10f) + 1f) / 2f; 
                _oobOverlayImage.color = new Color(1f, 0f, 0f, 0.15f + (pulse * 0.25f)); // Flashes between 0.15 and 0.40 alpha
                
                _oobCenterText.color = new Color(1f, 0.15f, 0.15f, 1f);
                _oobCenterText.text = $"{message}\n{timeLeft:F1}s";
            }
            else
            {
                if (_oobOverlayImage.color.a > 0f) _oobOverlayImage.color = new Color(1f, 0f, 0f, 0f);
                if (_oobCenterText.color.a > 0f) _oobCenterText.color = new Color(1f, 0.15f, 0.15f, 0f);
            }
        }

        private void UpdateCompass(float heading)
        {
            if (_compassLabels == null || _compassLabels.Length == 0) return;

            int[] offsets = { -90, -60, -30, 0, 30, 60, 90 };
            for (int i = 0; i < _compassLabels.Length; i++)
            {
                float value = (heading + offsets[i] + 360f) % 360f;
                _compassLabels[i].text = CompassLabel(value, offsets[i] == 0);
            }

            if (_headingText != null)
            {
                _headingText.text = $"{Mathf.RoundToInt(heading):000}";
            }
        }

        private void UpdateTapes(float airspeed, float altitude, float aoa, float gForce)
        {
            float airspeedKph = airspeed * 3.6f;

            if (_leftPrimaryText != null)
            {
                _leftPrimaryText.text = Mathf.RoundToInt(airspeedKph).ToString();
            }

            if (_leftTapeNeedle != null)
            {
                float speedMarker = Mathf.Clamp((airspeedKph - 120f) * 0.7f, -120f, 120f);
                _leftTapeNeedle.anchoredPosition = new Vector2(0f, speedMarker);
            }

            if (_leftSecondaryText != null)
            {
                _leftSecondaryText.text = $"AOA\n{aoa:0.0}\nG\n{gForce:0.0}";
            }

            if (_speedUnitText != null)
            {
                _speedUnitText.text = "KM/H";
            }

            if (_rightPrimaryText != null)
            {
                _rightPrimaryText.text = Mathf.RoundToInt(altitude).ToString();
            }

            if (_altUnitText != null)
            {
                _altUnitText.text = string.Empty;
            }

            if (_altitudeTapeCursor != null)
            {
                float tape = Mathf.Repeat(altitude, 240f) - 120f;
                _altitudeTapeCursor.anchoredPosition = new Vector2(2f, tape * 1.15f);
            }

        }

        private void UpdateCenterHud(float pitch, float roll, float climb, float heading)
        {
            bool showCenterIndicator = _cameraController == null || CameraViewHudRules.ShouldShowCenterIndicator(_cameraController.CurrentIndex);

            if (_horizonLine != null)
            {
                _horizonLine.localRotation = Quaternion.Euler(0f, 0f, roll);
                _horizonLine.anchoredPosition = new Vector2(0f, -pitch * 3.3f);
                _horizonLine.gameObject.SetActive(showCenterIndicator);
                _horizonLine.sizeDelta = new Vector2(340f, 6f);
            }

            if (_centerLadder != null)
            {
                _centerLadder.localRotation = Quaternion.Euler(0f, 0f, roll);
                _centerLadder.anchoredPosition = new Vector2(0f, -pitch * 3.3f);
                _centerLadder.gameObject.SetActive(showCenterIndicator);
            }

            if (_centerBracketFrame != null)
            {
                _centerBracketFrame.gameObject.SetActive(showCenterIndicator);
                _centerBracketFrame.SetAsLastSibling();
            }

            if (_centerMarker != null)
            {
                _centerMarker.anchoredPosition = Vector2.zero;
                _centerMarker.gameObject.SetActive(showCenterIndicator);
                _centerMarker.sizeDelta = new Vector2(34f, 34f);
            }

            if (_attitudeText != null)
            {
                _attitudeText.gameObject.SetActive(false);
            }

            if (_centerPitchText != null)
            {
                _centerPitchText.gameObject.SetActive(false);
            }

            if (_centerHorizonText != null)
            {
                _centerHorizonText.gameObject.SetActive(false);
            }
        }

        private static void UpdateDial(DialUi dial, float value, string display)
        {
            if (dial == null) return;

            float normalized = Mathf.InverseLerp(dial.minValue, dial.maxValue, value);
            if (dial.wrap)
            {
                normalized = Mathf.Repeat((value - dial.minValue) / Mathf.Max(1f, dial.maxValue - dial.minValue), 1f);
            }

            float angle = Mathf.Lerp(dial.startAngle, dial.endAngle, normalized);
            if (dial.needle != null)
            {
                dial.needle.localRotation = Quaternion.Euler(0f, 0f, -angle);
            }

            if (dial.value != null)
            {
                dial.value.text = display;
            }
        }

        private static string CompassLabel(float value, bool center)
        {
            int deg = Mathf.RoundToInt(value) % 360;
            if (deg == 0) return center ? "N" : "0";
            if (deg == 90) return "E";
            if (deg == 180) return "S";
            if (deg == 270) return "W";
            return deg.ToString();
        }

        private float GetHeading(Vector3 forward)
        {
            Vector3 flat = Vector3.ProjectOnPlane(forward, Vector3.up);
            if (flat.sqrMagnitude < 0.0001f) return 0f;

            float angle = Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg;
            return AircraftAttitudeValidator.NormalizeHeading(angle);
        }

        private float GetAltitudeAboveGround()
        {
            Vector3 origin = _plane.transform.position + _plane.transform.up * 2f;
            RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, 100000f, _plane.groundMask);

            float best = float.PositiveInfinity;
            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (hit.collider != null && hit.collider.transform.root == _plane.transform.root)
                {
                    continue;
                }

                if (hit.distance < best)
                {
                    best = hit.distance;
                }
            }

            if (!float.IsPositiveInfinity(best))
            {
                return best;
            }

            return _plane.transform.position.y;
        }
    }
}
