using UnityEngine;

namespace AeroByte.WeatherSystem.Adapters
{
    public class BlinkingEmission : MonoBehaviour
    {
        [Header("Configuración de Señal")]
        [SerializeField, ColorUsage(true, true)] private Color blinkColor = new Color(1f, 0.78f, 0.07f); // Yellowish matching SignalEmision
        [SerializeField] private float blinkSpeed = 4f;
        [SerializeField] private bool useSmoothPulse = false; // Sharp on/off blinking
        [SerializeField] private float emissionBoost = 4f; // Boosts intensity for real HDR bloom glow

        private Material materialInstance;
        private Renderer targetRenderer;

        private void Start()
        {
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                // Create a unique instance of the material to avoid affecting the asset file on disk
                materialInstance = targetRenderer.material;
                // Enable URP emission keyword
                materialInstance.EnableKeyword("_EMISSION");
            }
            else
            {
                Debug.LogError("[BlinkingEmission] No Renderer component found on this GameObject.", this);
            }
        }

        private void Update()
        {
            if (materialInstance == null) return;

            float intensity;
            if (useSmoothPulse)
            {
                // Smooth sine wave between 0 and 1
                intensity = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;
            }
            else
            {
                // Sharp on/off blinking based on floor division of time
                intensity = (Mathf.FloorToInt(Time.time * blinkSpeed) % 2 == 0) ? 1f : 0f;
            }

            // Apply the emission color multiplied by the calculated intensity and emission boost
            materialInstance.SetColor("_EmissionColor", blinkColor * (intensity * emissionBoost));
        }

        public void SetBlinkColor(Color newColor)
        {
            blinkColor = newColor;
        }

        private void OnDestroy()
        {
            // Clean up the instantiated material to prevent memory leaks in the editor/game
            if (materialInstance != null)
            {
                Destroy(materialInstance);
            }
        }
    }
}
