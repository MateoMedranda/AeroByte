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
        
        [ColorUsage(true, true)]
        [Tooltip("Color del círculo. Admite HDR para brillo nativo con Bloom.")]
        [SerializeField] private Color color = Color.red;

        [Tooltip("Intensidad emisiva del color. Valores > 1 aumentan el brillo en postprocesamiento.")]
        [SerializeField] private float emissionIntensity = 2f;

        [SerializeField] private float dashTiling = 20f;

        [Header("Material Personalizado (Opcional)")]
        [Tooltip("Material base opcional. Si se deja vacío, se generará uno transparente procedural con emisión.")]
        [SerializeField] private Material customMaterial;

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

            if (customMaterial != null)
            {
                // Si el usuario provee un material personalizado, instanciamos una copia para no alterar el original
                _material = new Material(customMaterial);
                if (_material.mainTexture == null)
                {
                    _material.mainTexture = texture;
                }
            }
            else
            {
                // Crear material transparente compatible por defecto
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null)
                {
                    shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply");
                }
                
                _material = new Material(shader);
                _material.mainTexture = texture;
            }
            
            _lineRenderer.material = _material;
            _lineRenderer.textureMode = LineTextureMode.Tile;
            
            // Configurar color y emisión
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
            
            // Calcular el color emisivo multiplicando los canales RGB por la intensidad de emisión
            Color emissiveColor = new Color(newColor.r * emissionIntensity, newColor.g * emissionIntensity, newColor.b * emissionIntensity, newColor.a);
            _originalColor = emissiveColor;
            
            if (_lineRenderer != null)
            {
                _lineRenderer.startColor = emissiveColor;
                _lineRenderer.endColor = emissiveColor;
            }
            
            if (_material != null)
            {
                _material.color = emissiveColor;
                
                // Si el material tiene soporte explícito para color de emisión (shaders URP/Standard)
                if (_material.HasProperty("_EmissionColor"))
                {
                    _material.SetColor("_EmissionColor", emissiveColor);
                    _material.EnableKeyword("_EMISSION");
                }
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
                
                if (_material.HasProperty("_EmissionColor"))
                {
                    _material.SetColor("_EmissionColor", c);
                }
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
