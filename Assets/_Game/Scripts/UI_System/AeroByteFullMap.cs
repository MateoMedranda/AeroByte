using FlightSystem.Adapters;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace AeroByte.UI_System
{
    public sealed class AeroByteFullMap : MonoBehaviour
    {
        [Tooltip("Opacidad del mapa. 1 es completamente opaco, 0 es invisible.")]
        public float mapOpacity = 1f;
        [Tooltip("Cantidad de divisiones para la cuadrícula.")]
        public float gridDivisions = 20f;

        [Header("Configuración del Mapa")]
        [Tooltip("Define qué tan grande es el área que cubre el mapa completo (zoom).")]
        public float mapScale = 1500f;

        private const int TextureSize = 1024;
        private const float CameraHeight = 800f;

        private static AeroByteFullMap _instance;

        private PlaneController _plane;
        private Canvas _canvas;
        private RawImage _view;
        private Camera _mapCamera;
        private GameObject _cameraObject;
        private RenderTexture _renderTexture;
        private RectTransform _playerMarker;
        private RectTransform _targetMarker;
        private Vector3 _mapOffset = Vector3.zero;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;

            var go = new GameObject("AeroByteFullMapSystem");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<AeroByteFullMap>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            BuildView();
            // Start hidden
            _canvas.enabled = false;
        }

        private void Update()
        {
            // Toggle map visibility with G key
            if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
            {
                _canvas.enabled = !_canvas.enabled;
                if (_canvas.enabled)
                {
                    _mapOffset = Vector3.zero; // Center on player when opened
                }
            }
            
            if (_canvas.enabled && Mouse.current != null)
            {
                // Zoom
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    float zoomFactor = (scroll > 0) ? 0.85f : 1.15f;
                    mapScale *= zoomFactor;
                    mapScale = Mathf.Clamp(mapScale, 500f, 15000f);
                }

                // Pan
                if (Mouse.current.rightButton.isPressed)
                {
                    Vector2 delta = Mouse.current.delta.ReadValue();
                    float panSpeed = mapScale * 0.002f;
                    _mapOffset.x -= delta.x * panSpeed;
                    _mapOffset.z -= delta.y * panSpeed;
                }
            }
        }

        private void LateUpdate()
        {
            if (!_canvas.enabled) return;

            if (_plane == null)
            {
                _plane = Object.FindFirstObjectByType<PlaneController>();
            }

            if (_plane != null && _mapCamera != null)
            {
                var planePosition = _plane.transform.position;
                _mapCamera.transform.SetPositionAndRotation(new Vector3(planePosition.x + _mapOffset.x, planePosition.y + CameraHeight, planePosition.z + _mapOffset.z), Quaternion.Euler(90f, 0f, 0f));
                if (_mapCamera.orthographicSize != mapScale)
                {
                    _mapCamera.orthographicSize = mapScale;
                }
                _mapCamera.Render();

                if (_playerMarker != null)
                {
                    Vector3 playerViewportPos = _mapCamera.WorldToViewportPoint(planePosition);
                    _playerMarker.anchorMin = playerViewportPos;
                    _playerMarker.anchorMax = playerViewportPos;
                    _playerMarker.anchoredPosition = Vector2.zero;
                    // Rotate the marker based on plane's Y rotation
                    _playerMarker.localRotation = Quaternion.Euler(0f, 0f, -_plane.transform.eulerAngles.y);
                }

                if (_targetMarker != null && MissionSystem.Adapters.AeroByteDeliveryManager.Instance != null)
                {
                    var currentZone = MissionSystem.Adapters.AeroByteDeliveryManager.Instance.GetCurrentActiveZone();
                    if (currentZone != null)
                    {
                        if (!_targetMarker.gameObject.activeSelf) _targetMarker.gameObject.SetActive(true);
                        Vector3 viewportPos = _mapCamera.WorldToViewportPoint(currentZone.transform.position);
                        _targetMarker.anchorMin = viewportPos;
                        _targetMarker.anchorMax = viewportPos;
                    }
                    else
                    {
                        if (_targetMarker.gameObject.activeSelf) _targetMarker.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (_cameraObject != null)
            {
                Destroy(_cameraObject);
            }

            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
            }
        }

        private void BuildView()
        {
            // Create the Canvas
            var canvasGo = new GameObject("FullMap Canvas");
            canvasGo.transform.SetParent(transform, false);

            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 5001; // Above the main HUD which is 5000

            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            // Render Texture for the map
            _renderTexture = new RenderTexture(TextureSize, TextureSize, 16)
            {
                name = "AeroByteFullMapRT",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            _renderTexture.Create();

            // RawImage for the map display
            var viewGo = new GameObject("FullMap View", typeof(RectTransform), typeof(RawImage));
            viewGo.transform.SetParent(canvasGo.transform, false);

            var viewRect = viewGo.GetComponent<RectTransform>();
            viewRect.anchorMin = Vector2.zero;
            viewRect.anchorMax = Vector2.one;
            viewRect.offsetMin = Vector2.zero;
            viewRect.offsetMax = Vector2.zero;

            _view = viewGo.GetComponent<RawImage>();
            _view.texture = _renderTexture;
            _view.color = new Color(1f, 1f, 1f, mapOpacity);

            // Add Grid Overlay
            var gridGo = new GameObject("FullMap Grid", typeof(RectTransform), typeof(RawImage));
            gridGo.transform.SetParent(viewGo.transform, false);
            var gridRect = gridGo.GetComponent<RectTransform>();
            gridRect.anchorMin = Vector2.zero;
            gridRect.anchorMax = Vector2.one;
            gridRect.offsetMin = Vector2.zero;
            gridRect.offsetMax = Vector2.zero;
            var gridImg = gridGo.GetComponent<RawImage>();
            gridImg.texture = GetGridTexture();
            gridImg.uvRect = new Rect(0, 0, gridDivisions, gridDivisions * ((float)Screen.height / Screen.width)); // Preserve square aspect ratio approximately
            gridImg.color = Color.white;

            // Create Camera
            _cameraObject = new GameObject("AeroByte FullMap Camera");
            _mapCamera = _cameraObject.AddComponent<Camera>();
            _mapCamera.enabled = false; // We render manually in LateUpdate
            _mapCamera.orthographic = true;
            _mapCamera.orthographicSize = mapScale;
            _mapCamera.nearClipPlane = 0.1f;
            _mapCamera.farClipPlane = 2000f;
            _mapCamera.clearFlags = CameraClearFlags.SolidColor;
            _mapCamera.backgroundColor = new Color(0.06f, 0.06f, 0.08f, 1f);
            _mapCamera.cullingMask = ~0;
            _mapCamera.allowHDR = false;
            _mapCamera.allowMSAA = false;
            _mapCamera.targetTexture = _renderTexture;
            _mapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // Player Marker
            var markerRoot = new GameObject("Player Marker", typeof(RectTransform));
            markerRoot.transform.SetParent(viewGo.transform, false);
            _playerMarker = markerRoot.GetComponent<RectTransform>();
            _playerMarker.anchorMin = new Vector2(0.5f, 0.5f);
            _playerMarker.anchorMax = new Vector2(0.5f, 0.5f);
            _playerMarker.sizeDelta = new Vector2(64f, 64f);
            _playerMarker.anchoredPosition = Vector2.zero;

            var markerCenter = new GameObject("Marker Center", typeof(RectTransform), typeof(Image));
            markerCenter.transform.SetParent(markerRoot.transform, false);
            var markerCenterRect = markerCenter.GetComponent<RectTransform>();
            markerCenterRect.anchorMin = new Vector2(0.5f, 0.5f);
            markerCenterRect.anchorMax = new Vector2(0.5f, 0.5f);
            markerCenterRect.sizeDelta = new Vector2(24f, 24f);
            markerCenterRect.anchoredPosition = Vector2.zero;
            var markerCenterImg = markerCenter.GetComponent<Image>();
            markerCenterImg.sprite = GetCircleSprite();
            markerCenterImg.color = Color.red;

            var markerTip = new GameObject("Marker Tip", typeof(RectTransform), typeof(Image));
            markerTip.transform.SetParent(markerRoot.transform, false);
            var markerTipRect = markerTip.GetComponent<RectTransform>();
            markerTipRect.anchorMin = new Vector2(0.5f, 0.5f);
            markerTipRect.anchorMax = new Vector2(0.5f, 0.5f);
            markerTipRect.sizeDelta = new Vector2(10f, 40f);
            markerTipRect.anchoredPosition = new Vector2(0f, 20f);
            var markerTipImg = markerTip.GetComponent<Image>();
            markerTipImg.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            markerTipImg.color = Color.red;

            // Target Marker
            var targetRoot = new GameObject("Target Marker", typeof(RectTransform));
            targetRoot.transform.SetParent(viewGo.transform, false);
            _targetMarker = targetRoot.GetComponent<RectTransform>();
            _targetMarker.anchorMin = new Vector2(0.5f, 0.5f);
            _targetMarker.anchorMax = new Vector2(0.5f, 0.5f);
            _targetMarker.sizeDelta = new Vector2(32f, 32f);
            _targetMarker.anchoredPosition = Vector2.zero;

            var targetImg = targetRoot.AddComponent<Image>();
            targetImg.sprite = GetCircleSprite();
            targetImg.color = Color.yellow;
            _targetMarker.gameObject.SetActive(false);
        }

        private static Sprite _circleSprite;
        private static Texture2D _circleTexture;
        private static Texture2D _gridTexture;

        private static Texture2D GetGridTexture()
        {
            if (_gridTexture != null) return _gridTexture;

            const int size = 64;
            _gridTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Repeat,
                name = "AeroByteFullMapGridTexture"
            };

            var pixels = new Color32[size * size];
            var lineColor = new Color32(255, 255, 255, 80); // White semi-transparent line
            var clearColor = new Color32(0, 0, 0, 0);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Draw thin lines on edges
                    if (x < 2 || y < 2)
                        pixels[y * size + x] = lineColor;
                    else
                        pixels[y * size + x] = clearColor;
                }
            }

            _gridTexture.SetPixels32(pixels);
            _gridTexture.Apply(false, false);
            return _gridTexture;
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
                name = "AeroByteFullMapCircleTexture"
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
            _circleSprite.name = "AeroByteFullMapCircleSprite";
            return _circleSprite;
        }
    }
}
