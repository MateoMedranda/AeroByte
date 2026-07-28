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

        [Header("Partículas, Efectos y Sonidos en el Avión")]
        [Tooltip("Arrastra aquí los sistemas de partículas (hijos del avión) para lluvia, arena, etc. Se activan SOLO al entrar a la zona y se apagarán al salir.")]
        public ParticleSystem[] stormParticles;
        [Tooltip("GameObjects completos de efectos que quieras encender dentro de la tormenta y apagar al salir.")]
        public GameObject[] stormEffectObjects;
        [Tooltip("Archivos de audio (.wav / .mp3) para sonido de lluvia o tormenta. ¡Puedes arrastrar tu archivo .wav directamente aquí!")]
        public AudioClip[] stormAudioClips;
        [Tooltip("Volumen del sonido de la tormenta.")]
        [Range(0f, 1f)]
        public float stormAudioVolume = 0.7f;
        [Tooltip("Fuentes de audio (AudioSource) para sonido de lluvia, viento o tormenta. Se reproducirán al entrar y se detendrán al salir.")]
        public AudioSource[] stormAudioSources;

        private WeatherPhysicsUseCase _weatherUseCase;
        private bool _playerInZone = false;
        private float _originalFogDensity;
        private Color _originalFogColor;
        private bool _wasFogEnabled;

        private FogMode _originalFogMode;
        private AudioSource[] _createdAudioSources;

        private void Start()
        {
            _wasFogEnabled = RenderSettings.fog;
            _originalFogMode = RenderSettings.fogMode;
            _originalFogDensity = RenderSettings.fogDensity;
            _originalFogColor = RenderSettings.fogColor;

            // Crear automáticamente reproductores de audio para los archivos .wav asignados
            if (stormAudioClips != null && stormAudioClips.Length > 0)
            {
                _createdAudioSources = new AudioSource[stormAudioClips.Length];
                for (int i = 0; i < stormAudioClips.Length; i++)
                {
                    if (stormAudioClips[i] != null)
                    {
                        GameObject audioObj = new GameObject("StormAudio_" + stormAudioClips[i].name);
                        audioObj.transform.SetParent(this.transform);
                        AudioSource src = audioObj.AddComponent<AudioSource>();
                        src.clip = stormAudioClips[i];
                        src.loop = true;
                        src.volume = stormAudioVolume;
                        src.playOnAwake = false;
                        src.spatialBlend = 0f; // Sonido 2D envolvente en toda la zona
                        _createdAudioSources[i] = src;
                    }
                }
            }

            // Apagar las partículas, efectos y sonidos por defecto al iniciar el juego
            SetStormEffectsActive(false);
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
                SetStormEffectsActive(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            PlaneController plane = other.GetComponentInParent<PlaneController>();

            if (plane != null)
            {
                plane.RegisterWeatherZoneExit();
                _playerInZone = false;
                SetStormEffectsActive(false);
            }
        }

        private void SetStormEffectsActive(bool active)
        {
            if (stormParticles != null)
            {
                foreach (var ps in stormParticles)
                {
                    if (ps != null)
                    {
                        if (active)
                        {
                            ps.gameObject.SetActive(true);
                            ps.Play();
                        }
                        else
                        {
                            ps.Stop();
                            ps.gameObject.SetActive(false);
                        }
                    }
                }
            }

            if (stormEffectObjects != null)
            {
                foreach (var obj in stormEffectObjects)
                {
                    if (obj != null)
                    {
                        obj.SetActive(active);
                    }
                }
            }

            if (stormAudioSources != null)
            {
                foreach (var audio in stormAudioSources)
                {
                    if (audio != null)
                    {
                        if (active)
                        {
                            audio.gameObject.SetActive(true);
                            if (!audio.isPlaying) audio.Play();
                        }
                        else
                        {
                            if (audio.isPlaying) audio.Stop();
                            audio.gameObject.SetActive(false);
                        }
                    }
                }
            }

            if (_createdAudioSources != null)
            {
                foreach (var audio in _createdAudioSources)
                {
                    if (audio != null)
                    {
                        if (active)
                        {
                            if (!audio.isPlaying) audio.Play();
                        }
                        else
                        {
                            if (audio.isPlaying) audio.Stop();
                        }
                    }
                }
            }
        }
    }
}
