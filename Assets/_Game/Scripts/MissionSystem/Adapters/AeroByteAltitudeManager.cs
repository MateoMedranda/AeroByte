using UnityEngine;
using FlightSystem.Adapters;

namespace MissionSystem.Adapters
{
    public class AeroByteAltitudeManager : MonoBehaviour
    {
        public static AeroByteAltitudeManager Instance { get; private set; }

        [Header("Rango de Altura Segura (metros)")]
        [Tooltip("Altura mínima segura. Volar por debajo activará la defensa antiaérea.")]
        public float minSafeAltitude = 50f;
        [Tooltip("Altura máxima segura. Volar por encima te pondrá en el radar enemigo.")]
        public float maxSafeAltitude = 300f;

        [Header("Tiempo Límite antes del Derribo (segundos)")]
        [Tooltip("Segundos que puedes permanecer fuera del rango seguro antes de que te disparen y explotes.")]
        public float maxDangerTime = 5f;

        [Header("Despegue")]
        [Tooltip("Si está activo, el avión no explotará por baja altitud hasta que alcance la altura mínima por primera vez o se agote el tiempo de gracia.")]
        public bool ignoreMinAltitudeDuringTakeoff = true;
        [Tooltip("Tiempo máximo en segundos que el jugador tiene para despegar y subir antes de que la alarma antiaérea de baja altura se active obligatoriamente.")]
        public float maxTakeoffTime = 20f;

        [Header("Alerta Visual (.png / Sprite)")]
        [Tooltip("¡Arrastra aquí directamente tu archivo .PNG de ícono de alerta (radar/misil/peligro)! No necesitas cambiar nada en Unity.")]
        public Texture2D dangerAlertTexture;
        [Tooltip("O arrastra aquí la imagen (Sprite 2D).")]
        public Sprite dangerAlertIcon;

        [Header("Audio y Explosión")]
        [Tooltip("Sonido de alarma al estar en zona de peligro (Opcional).")]
        public AudioClip alarmSound;
        [Tooltip("Prefab de explosión cuando te derriban (Opcional).")]
        public GameObject killExplosionPrefab;

        public bool IsAltitudeDanger { get; private set; }
        public float CurrentDangerTimer { get; private set; }
        public bool IsKilled { get; private set; }

        private PlaneController _plane;
        private Sprite _cachedSpriteFromTexture;
        private AudioSource _alarmAudioSource;
        private bool _hasReachedSafeAltitude = false;
        private float _takeoffTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            CurrentDangerTimer = maxDangerTime;
            _takeoffTimer = maxTakeoffTime;

            _alarmAudioSource = gameObject.AddComponent<AudioSource>();
            _alarmAudioSource.loop = true;
            _alarmAudioSource.playOnAwake = false;
        }

        private void Start()
        {
            _plane = UnityEngine.Object.FindFirstObjectByType<PlaneController>();
        }

        private void Update()
        {
            if (IsKilled) return;

            if (_plane == null)
            {
                _plane = UnityEngine.Object.FindFirstObjectByType<PlaneController>();
                if (_plane == null) return;
            }

            // Si estamos dentro de la Zona de Entrega o de un LandingGoalTrigger para aterrizar, no penalizar por baja altura
            var deliveryCtrl = _plane.GetComponent<PlaneDeliveryController>();
            if ((deliveryCtrl != null && deliveryCtrl.CurrentZone != null) || LandingGoalTrigger.IsPlaneInAnyLandingZone)
            {
                if (IsAltitudeDanger)
                {
                    IsAltitudeDanger = false;
                    CurrentDangerTimer = maxDangerTime;
                    if (_alarmAudioSource.isPlaying) _alarmAudioSource.Stop();
                }
                return;
            }

            float currentAlt = _plane.transform.position.y;
            bool outOfBoundsAlt = false;

            // Fase de despegue: no penalizar por baja altura hasta llegar a minSafeAltitude o agotar el tiempo máximo de despegue
            if (ignoreMinAltitudeDuringTakeoff && !_hasReachedSafeAltitude)
            {
                _takeoffTimer -= Time.deltaTime;

                if (currentAlt >= minSafeAltitude)
                {
                    _hasReachedSafeAltitude = true;
                    Debug.Log("[AeroByteAltitudeManager] Altitud mínima segura alcanzada tras despegue. ¡Radar activo para límite inferior y superior!");
                }
                else if (_takeoffTimer <= 0f)
                {
                    _hasReachedSafeAltitude = true;
                    Debug.LogWarning("[AeroByteAltitudeManager] Tiempo de gracia de despegue agotado. ¡Se activa la alarma para altitud mínima!");
                }
                else
                {
                    // Durante el despegue solo penaliza si vuela demasiado alto
                    outOfBoundsAlt = currentAlt > maxSafeAltitude;
                }
            }
            else
            {
                outOfBoundsAlt = currentAlt < minSafeAltitude || currentAlt > maxSafeAltitude;
            }

            if (outOfBoundsAlt)
            {
                if (!IsAltitudeDanger)
                {
                    IsAltitudeDanger = true;
                    if (alarmSound != null && !_alarmAudioSource.isPlaying)
                    {
                        _alarmAudioSource.clip = alarmSound;
                        _alarmAudioSource.Play();
                    }
                    Debug.LogWarning($"[AeroByteAltitudeManager] ¡ALERTA RADAR! Fuera de altura segura ({currentAlt:F1}m). Rango seguro: {minSafeAltitude}m - {maxSafeAltitude}m");
                }

                CurrentDangerTimer -= Time.deltaTime;
                if (CurrentDangerTimer <= 0f)
                {
                    CurrentDangerTimer = 0f;
                    TriggerKill();
                }
            }
            else
            {
                if (IsAltitudeDanger)
                {
                    IsAltitudeDanger = false;
                    CurrentDangerTimer = maxDangerTime;
                    if (_alarmAudioSource.isPlaying) _alarmAudioSource.Stop();
                    Debug.Log("[AeroByteAltitudeManager] Avión de vuelta en rango de altura seguro.");
                }
            }
        }

        private void TriggerKill()
        {
            if (IsKilled) return;
            IsKilled = true;
            IsAltitudeDanger = false;
            if (_alarmAudioSource.isPlaying) _alarmAudioSource.Stop();

            Debug.LogError("[AeroByteAltitudeManager] ¡TIEMPO EN ZONA PELIGROSA AGOTADO! El avión fue derribado y ha explotado.");

            if (killExplosionPrefab != null && _plane != null)
            {
                var explosion = Instantiate(killExplosionPrefab, _plane.transform.position, Quaternion.identity);
                Destroy(explosion, 8f);
            }

            if (_plane != null)
            {
                _plane.ForceCrash();
            }
        }

        public Sprite GetAlertSprite()
        {
            if (_cachedSpriteFromTexture != null) return _cachedSpriteFromTexture;

            if (dangerAlertTexture != null)
            {
                _cachedSpriteFromTexture = Sprite.Create(
                    dangerAlertTexture,
                    new Rect(0, 0, dangerAlertTexture.width, dangerAlertTexture.height),
                    new Vector2(0.5f, 0.5f)
                );
                return _cachedSpriteFromTexture;
            }

            return dangerAlertIcon;
        }
    }
}
