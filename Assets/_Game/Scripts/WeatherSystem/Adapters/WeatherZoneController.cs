using UnityEngine;
using WeatherSystem.UseCases;
using AeroByte.WeatherSystem.Framework.Config;
using FlightSystem.Adapters; // Para encontrar el PlaneController

namespace WeatherSystem.Adapters
{
    [RequireComponent(typeof(BoxCollider))]
    public class WeatherZoneController : MonoBehaviour
    {
        [Header("Configuración del Clima")]
        public WeatherStatsConfig weatherConfig;

        [Header("Efectos Visuales (Tormenta)")]
        public bool overrideFog = true;
        public Color stormFogColor = new Color(0.7f, 0.45f, 0.25f);
        public float stormFogDensity = 0.02f;
        public float fogTransitionSpeed = 1f;

        private WeatherPhysicsUseCase _weatherUseCase;
        private bool _playerInZone = false;
        private float _originalFogDensity;
        private Color _originalFogColor;
        private bool _wasFogEnabled;

        private FogMode _originalFogMode;

        private void Start()
        {
            _wasFogEnabled = RenderSettings.fog;
            _originalFogMode = RenderSettings.fogMode;
            _originalFogDensity = RenderSettings.fogDensity;
            _originalFogColor = RenderSettings.fogColor;
        }

        private void Update()
        {
            if (!overrideFog) return;

            if (_playerInZone)
            {
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                // Move quickly at the beginning so it's not "stuck", then smooth out
                RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, stormFogDensity, Time.deltaTime * fogTransitionSpeed * 2f);
                RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, stormFogColor, Time.deltaTime * fogTransitionSpeed * 2f);
            }
            else
            {
                RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, _originalFogDensity, Time.deltaTime * fogTransitionSpeed * 2f);
                
                // Only lerp color back if there was actual fog originally to prevent it from turning weird white
                if (_wasFogEnabled && _originalFogDensity > 0.001f)
                {
                    RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, _originalFogColor, Time.deltaTime * fogTransitionSpeed * 2f);
                }

                if (!_wasFogEnabled && Mathf.Abs(RenderSettings.fogDensity - _originalFogDensity) < 0.0005f)
                {
                    RenderSettings.fog = false;
                    RenderSettings.fogMode = _originalFogMode;
                }
            }
        }

        private void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            _weatherUseCase = new WeatherPhysicsUseCase(weatherConfig);
        }

        private void OnTriggerStay(Collider other)
        {
            PlaneController plane = other.GetComponentInParent<PlaneController>();
            
            if (plane != null)
            {
                _weatherUseCase.ApplyWeather(plane, plane.transform.position, Time.time);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            PlaneController plane = other.GetComponentInParent<PlaneController>();

            if (plane != null)
            {
                plane.RegisterWeatherZoneEnter();
                _playerInZone = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            PlaneController plane = other.GetComponentInParent<PlaneController>();

            if (plane != null)
            {
                plane.RegisterWeatherZoneExit();
                _playerInZone = false;
            }
        }
    }
}
