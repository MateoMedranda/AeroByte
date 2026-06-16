using UnityEngine;

namespace AeroByte.CheckpointSystem.Framework.Visuals
{
    [RequireComponent(typeof(LineRenderer))]
    public class DashedCircleRenderer : MonoBehaviour
    {
        [Header("Configuración del Círculo")]
        [SerializeField] private float radius = 5f;
        [SerializeField] private int segments = 120;
        [SerializeField] private float width = 0.3f;
        [SerializeField] private Color color = Color.red;
        [SerializeField] private float dashTiling = 20f;

        private LineRenderer _lineRenderer;
        private Material _material;
        private Color _originalColor;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            SetupLineRenderer();
            GenerateCircle();
        }

        private void SetupLineRenderer()
        {
            _lineRenderer.useWorldSpace = false;
            _lineRenderer.startWidth = width;
            _lineRenderer.endWidth = width;
            _lineRenderer.positionCount = segments + 1;
            
            // Crear textura discontinua (dash texture)
            Texture2D texture = new Texture2D(16, 1);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Point;
            for (int i = 0; i < 16; i++)
            {
                texture.SetPixel(i, 0, i < 8 ? Color.white : new Color(1, 1, 1, 0));
            }
            texture.Apply();

            // Crear material transparente compatible
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply");
            }
            
            _material = new Material(shader);
            _material.mainTexture = texture;
            
            _lineRenderer.material = _material;
            _lineRenderer.textureMode = LineTextureMode.Tile;
            
            // Configurar color
            SetColor(color);
        }

        private void GenerateCircle()
        {
            if (_lineRenderer == null) return;
            
            float angle = 0f;
            for (int i = 0; i <= segments; i++)
            {
                // Aro vertical en el plano XY (el avión lo atraviesa de frente)
                float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
                float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
                _lineRenderer.SetPosition(i, new Vector3(x, y, 0f));

                angle += (360f / segments);
            }

            if (_material != null)
            {
                // El perímetro es 2 * pi * radius
                float perimeter = 2 * Mathf.PI * radius;
                _material.mainTextureScale = new Vector2(perimeter * dashTiling * 0.1f, 1f);
            }
        }

        public void SetColor(Color newColor)
        {
            color = newColor;
            _originalColor = newColor;
            
            if (_lineRenderer != null)
            {
                _lineRenderer.startColor = newColor;
                _lineRenderer.endColor = newColor;
            }
            
            if (_material != null)
            {
                _material.color = newColor;
            }
        }

        public void SetAlpha(float alpha)
        {
            Color c = new Color(_originalColor.r, _originalColor.g, _originalColor.b, alpha);
            
            if (_lineRenderer != null)
            {
                _lineRenderer.startColor = c;
                _lineRenderer.endColor = c;
            }
            
            if (_material != null)
            {
                _material.color = c;
            }
        }

        public float GetRadius() => radius;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_lineRenderer == null)
            {
                _lineRenderer = GetComponent<LineRenderer>();
            }
            
            if (_lineRenderer != null)
            {
                _lineRenderer.startWidth = width;
                _lineRenderer.endWidth = width;
                SetupLineRenderer();
                GenerateCircle();
            }
        }
#endif
    }
}
