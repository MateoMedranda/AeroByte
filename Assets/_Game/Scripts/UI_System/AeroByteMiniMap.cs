using FlightSystem.Adapters;
using UnityEngine;
using UnityEngine.UI;

namespace AeroByte.UI_System
{
    public sealed class AeroByteMiniMap : MonoBehaviour
    {
        private const int TextureSize = 512;
        private const float CameraHeight = 400f;
        private const float OrthographicSize = 250f;

        private static Sprite _circleSprite;
        private static Texture2D _circleTexture;

        private PlaneController _plane;
        private RawImage _view;
        private Camera _mapCamera;
        private GameObject _cameraObject;
        private RenderTexture _renderTexture;
        private RectTransform _playerMarker;
        private RectTransform _playerMarkerTip;
        private RectTransform _targetMarker;

        private void Awake()
        {
            BuildView();
        }

        private void LateUpdate()
        {
            if (_plane == null)
            {
                _plane = Object.FindFirstObjectByType<PlaneController>();
            }

            if (_plane != null && _mapCamera != null)
            {
                var planePosition = _plane.transform.position;
                _mapCamera.transform.SetPositionAndRotation(new Vector3(planePosition.x, planePosition.y + CameraHeight, planePosition.z), Quaternion.Euler(90f, 0f, 0f));
                _mapCamera.Render();

                if (_playerMarker != null)
                {
                    _playerMarker.anchoredPosition = Vector2.zero;
                    _playerMarker.localRotation = Quaternion.Euler(0f, 0f, -_plane.transform.eulerAngles.y);
                }

                if (_targetMarker != null)
                {
                    Transform targetTransform = null;
                    if (AeroByte.CheckpointSystem.Adapters.CheckpointSequenceController.ActiveInstance != null && AeroByte.CheckpointSystem.Adapters.CheckpointSequenceController.ActiveInstance.IsRaceActive)
                    {
                        targetTransform = AeroByte.CheckpointSystem.Adapters.CheckpointSequenceController.ActiveInstance.GetCurrentActiveCheckpointTransform();
                    }
                    else if (MissionSystem.Adapters.CheckpointRaceManager.Instance != null && MissionSystem.Adapters.CheckpointRaceManager.Instance.IsRaceActive)
                    {
                        targetTransform = MissionSystem.Adapters.CheckpointRaceManager.Instance.GetCurrentActiveCheckpointTransform();
                    }
                    else if (MissionSystem.Adapters.AeroByteAttackManager.Instance != null && !MissionSystem.Adapters.AeroByteAttackManager.Instance.IsMissionComplete)
                    {
                        targetTransform = MissionSystem.Adapters.AeroByteAttackManager.Instance.GetCurrentActiveZoneTransform();
                    }
                    else if (MissionSystem.Adapters.AeroByteDeliveryManager.Instance != null)
                    {
                        var currentZone = MissionSystem.Adapters.AeroByteDeliveryManager.Instance.GetCurrentActiveZone();
                        if (currentZone != null) targetTransform = currentZone.transform;
                    }

                    if (targetTransform != null)
                    {
                        if (!_targetMarker.gameObject.activeSelf) _targetMarker.gameObject.SetActive(true);
                        Vector3 viewportPos = _mapCamera.WorldToViewportPoint(targetTransform.position);
                        Vector2 viewCenter = new Vector2(0.5f, 0.5f);
                        Vector2 offset = new Vector2(viewportPos.x, viewportPos.y) - viewCenter;
                        
                        float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
                        _targetMarker.localRotation = Quaternion.Euler(0, 0, angle - 90f);
                        
                        if (offset.magnitude > 0.42f)
                        {
                            offset = offset.normalized * 0.42f;
                        }
                        
                        _targetMarker.anchorMin = viewCenter + offset;
                        _targetMarker.anchorMax = viewCenter + offset;
                        _targetMarker.anchoredPosition = Vector2.zero;
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
            var frame = GetComponent<RectTransform>();
            if (frame != null)
            {
                frame.anchorMin = Vector2.zero;
                frame.anchorMax = Vector2.zero;
            }

            var frameImage = GetComponent<Image>();
            if (frameImage != null)
            {
                frameImage.sprite = GetCircleSprite();
                frameImage.type = Image.Type.Simple;
                frameImage.color = new Color(0f, 0f, 0f, 0.72f);
            }

            var outerRing = new GameObject("MiniMap Outer Ring", typeof(RectTransform), typeof(Image));
            outerRing.transform.SetParent(transform, false);
            var outerRingRect = outerRing.GetComponent<RectTransform>();
            outerRingRect.anchorMin = Vector2.zero;
            outerRingRect.anchorMax = Vector2.one;
            outerRingRect.offsetMin = Vector2.zero;
            outerRingRect.offsetMax = Vector2.zero;
            var outerRingImg = outerRing.GetComponent<Image>();
            outerRingImg.sprite = GetCircleSprite();
            outerRingImg.type = Image.Type.Sliced;
            outerRingImg.color = new Color(0f, 0f, 0f, 0.62f);

            var circleMaskGo = new GameObject("MiniMap Circle Mask", typeof(RectTransform), typeof(Image), typeof(Mask));
            circleMaskGo.transform.SetParent(transform, false);
            var maskRect = circleMaskGo.GetComponent<RectTransform>();
            maskRect.anchorMin = Vector2.zero;
            maskRect.anchorMax = Vector2.one;
            maskRect.offsetMin = new Vector2(10f, 10f);
            maskRect.offsetMax = new Vector2(-10f, -10f);
            var maskImg = circleMaskGo.GetComponent<Image>();
            maskImg.sprite = GetCircleSprite();
            maskImg.type = Image.Type.Sliced;
            maskImg.color = new Color(0.05f, 0.07f, 0.08f, 0.96f);
            var mask = circleMaskGo.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            var viewGo = new GameObject("MiniMap View", typeof(RectTransform), typeof(RawImage));
            viewGo.transform.SetParent(circleMaskGo.transform, false);

            var viewRect = viewGo.GetComponent<RectTransform>();
            viewRect.anchorMin = Vector2.zero;
            viewRect.anchorMax = Vector2.one;
            viewRect.offsetMin = Vector2.zero;
            viewRect.offsetMax = Vector2.zero;

            _view = viewGo.GetComponent<RawImage>();
            _view.color = Color.white;

            _renderTexture = new RenderTexture(TextureSize, TextureSize, 16)
            {
                name = "AeroByteMiniMapRT",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            _renderTexture.Create();
            _view.texture = _renderTexture;

            var rim = new GameObject("MiniMap Rim", typeof(RectTransform), typeof(Image));
            rim.transform.SetParent(transform, false);
            var rimRect = rim.GetComponent<RectTransform>();
            rimRect.anchorMin = Vector2.zero;
            rimRect.anchorMax = Vector2.one;
            rimRect.offsetMin = new Vector2(2f, 2f);
            rimRect.offsetMax = new Vector2(-2f, -2f);
            var rimImg = rim.GetComponent<Image>();
            rimImg.sprite = GetCircleSprite();
            rimImg.type = Image.Type.Sliced;
            rimImg.color = new Color(0.85f, 0.85f, 0.88f, 0.22f);

            _cameraObject = new GameObject("AeroByte MiniMap Camera");
            _mapCamera = _cameraObject.AddComponent<Camera>();
            _mapCamera.enabled = false;
            _mapCamera.orthographic = true;
            _mapCamera.orthographicSize = OrthographicSize;
            _mapCamera.nearClipPlane = 0.1f;
            _mapCamera.farClipPlane = 1500f;
            _mapCamera.clearFlags = CameraClearFlags.SolidColor;
            _mapCamera.backgroundColor = new Color(0.06f, 0.06f, 0.08f, 1f);
            _mapCamera.cullingMask = ~0;
            _mapCamera.allowHDR = false;
            _mapCamera.allowMSAA = false;
            _mapCamera.targetTexture = _renderTexture;
            _mapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            var markerRoot = new GameObject("Player Marker", typeof(RectTransform));
            markerRoot.transform.SetParent(circleMaskGo.transform, false);
            _playerMarker = markerRoot.GetComponent<RectTransform>();
            _playerMarker.anchorMin = new Vector2(0.5f, 0.5f);
            _playerMarker.anchorMax = new Vector2(0.5f, 0.5f);
            _playerMarker.sizeDelta = new Vector2(36f, 36f);
            _playerMarker.anchoredPosition = Vector2.zero;

            var markerCenter = new GameObject("Marker Center", typeof(RectTransform), typeof(Image));
            markerCenter.transform.SetParent(markerRoot.transform, false);
            var markerCenterRect = markerCenter.GetComponent<RectTransform>();
            markerCenterRect.anchorMin = new Vector2(0.5f, 0.5f);
            markerCenterRect.anchorMax = new Vector2(0.5f, 0.5f);
            markerCenterRect.sizeDelta = new Vector2(12f, 12f);
            markerCenterRect.anchoredPosition = Vector2.zero;
            var markerCenterImg = markerCenter.GetComponent<Image>();
            markerCenterImg.sprite = GetCircleSprite();
            markerCenterImg.color = new Color(0.95f, 1f, 0.35f, 1f);

            var markerTip = new GameObject("Marker Tip", typeof(RectTransform), typeof(Image));
            markerTip.transform.SetParent(markerRoot.transform, false);
            _playerMarkerTip = markerTip.GetComponent<RectTransform>();
            _playerMarkerTip.anchorMin = new Vector2(0.5f, 0.5f);
            _playerMarkerTip.anchorMax = new Vector2(0.5f, 0.5f);
            _playerMarkerTip.sizeDelta = new Vector2(4f, 20f);
            _playerMarkerTip.anchoredPosition = new Vector2(0f, 12f);
            var markerTipImg = markerTip.GetComponent<Image>();
            markerTipImg.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            markerTipImg.color = new Color(0.95f, 1f, 0.35f, 1f);

            var centerHalo = new GameObject("Center Halo", typeof(RectTransform), typeof(Image));
            centerHalo.transform.SetParent(markerRoot.transform, false);
            var haloRect = centerHalo.GetComponent<RectTransform>();
            haloRect.anchorMin = new Vector2(0.5f, 0.5f);
            haloRect.anchorMax = new Vector2(0.5f, 0.5f);
            haloRect.sizeDelta = new Vector2(26f, 26f);
            haloRect.anchoredPosition = Vector2.zero;
            var haloImg = centerHalo.GetComponent<Image>();
            haloImg.sprite = GetCircleSprite();
            haloImg.color = new Color(0.95f, 0.95f, 0.98f, 0.16f);

            var targetRoot = new GameObject("Target Marker", typeof(RectTransform), typeof(Image));
            targetRoot.transform.SetParent(circleMaskGo.transform, false);
            _targetMarker = targetRoot.GetComponent<RectTransform>();
            _targetMarker.anchorMin = new Vector2(0.5f, 0.5f);
            _targetMarker.anchorMax = new Vector2(0.5f, 0.5f);
            _targetMarker.sizeDelta = new Vector2(12f, 28f);
            var targetImg = targetRoot.GetComponent<Image>();
            targetImg.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            targetImg.color = new Color(1f, 0.9f, 0.1f, 1f);
            _targetMarker.gameObject.SetActive(false);

            if (frame != null)
            {
                frame.SetAsLastSibling();
            }
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
                name = "AeroByteMiniMapCircleTexture"
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
            _circleSprite.name = "AeroByteMiniMapCircleSprite";
            return _circleSprite;
        }

        private static Sprite GetPanelSprite()
        {
            return Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        }
    }
}
