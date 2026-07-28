using UnityEngine;
using FlightSystem.Adapters;

namespace MissionSystem.Adapters
{
    public class AnimalCargoManager : MonoBehaviour
    {
        public static AnimalCargoManager Instance { get; private set; }

        [Header("Reglas de Altitud")]
        [Tooltip("Altitud mínima permitida antes de que el animal se asuste.")]
        public float minAltitude = 200f;
        [Tooltip("Altitud máxima permitida antes de que el animal se asuste por la presión/frío.")]
        public float maxAltitude = 1500f;

        [Header("Tolerancia")]
        [Tooltip("Cuántos segundos puede estar fuera de rango antes de perder la misión.")]
        public float stressToleranceTime = 5f;

        [Header("Despegue")]
        [Tooltip("Si está activo, el animal no se estresará por baja altitud hasta que el avión alcance la altitud mínima por primera vez o se agote el tiempo de gracia.")]
        public bool ignoreMinAltitudeDuringTakeoff = true;
        [Tooltip("Tiempo máximo en segundos que el jugador tiene para despegar y subir antes de que la alarma se active obligatoriamente.")]
        public float maxTakeoffTime = 20f;

        [Header("Alerta Visual (.png / Sprite)")]
        [Tooltip("¡Arrastra aquí directamente tu archivo .PNG de imagen del animalito asustado! No necesitas cambiarle nada en Unity.")]
        public Texture2D scaredAnimalTexture;
        [Tooltip("También puedes asignar un Sprite si ya lo tenías configurado.")]
        public Sprite scaredAnimalIcon;

        private Sprite _generatedSprite;

        public Sprite GetAlertSprite()
        {
            if (scaredAnimalIcon != null) return scaredAnimalIcon;
            if (scaredAnimalTexture != null)
            {
                if (_generatedSprite == null)
                {
                    _generatedSprite = Sprite.Create(
                        scaredAnimalTexture,
                        new Rect(0, 0, scaredAnimalTexture.width, scaredAnimalTexture.height),
                        new Vector2(0.5f, 0.5f)
                    );
                }
                return _generatedSprite;
            }
            return null;
        }

        public bool IsAnimalStressed { get; private set; }
        public float CurrentStressTimer { get; private set; }
        
        private PlaneController _plane;
        private bool _missionFailed = false;
        private bool _hasReachedSafeAltitude = false;
        private float _takeoffTimer;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            _plane = FindFirstObjectByType<PlaneController>();
            CurrentStressTimer = stressToleranceTime;
            _takeoffTimer = maxTakeoffTime;
        }

        private void Update()
        {
            if (_missionFailed || _plane == null) return;

            // Si la misión de entrega ya se completó, desactivamos las alarmas para siempre
            if (AeroByteDeliveryManager.Instance != null && 
                AeroByteDeliveryManager.Instance.CurrentZoneIndex >= AeroByteDeliveryManager.Instance.TotalZones)
            {
                if (IsAnimalStressed) CalmAnimal();
                return;
            }

            // Si estamos dentro de la Zona de Entrega o de un LandingGoalTrigger para aterrizar, no penalizar por baja altura
            var deliveryCtrl = _plane.GetComponent<PlaneDeliveryController>();
            if ((deliveryCtrl != null && deliveryCtrl.CurrentZone != null) || LandingGoalTrigger.IsPlaneInAnyLandingZone)
            {
                if (IsAnimalStressed) CalmAnimal();
                return;
            }

            // Obtener la altura real del avión (usando la Y global)
            float currentAltitude = _plane.transform.position.y;

            // Fase de despegue: no penalizar por baja altura hasta llegar a minAltitude o agotar el tiempo máximo de despegue
            if (ignoreMinAltitudeDuringTakeoff && !_hasReachedSafeAltitude)
            {
                _takeoffTimer -= Time.deltaTime;

                if (currentAltitude >= minAltitude)
                {
                    _hasReachedSafeAltitude = true;
                    Debug.Log("[AnimalCargoManager] Altitud mínima alcanzada. ¡Se activa la alarma de altitud mínima para el resto del vuelo!");
                }
                else if (_takeoffTimer <= 0f)
                {
                    _hasReachedSafeAltitude = true;
                    Debug.LogWarning("[AnimalCargoManager] Tiempo de despegue agotado. ¡Se activa obligatoriamente la restricción de altitud mínima!");
                }
                else
                {
                    // Durante el despegue solo vigilamos si excede la altitud máxima
                    if (currentAltitude > maxAltitude)
                    {
                        TriggerStress();
                    }
                    else if (IsAnimalStressed)
                    {
                        CalmAnimal();
                    }
                    return;
                }
            }

            if (currentAltitude < minAltitude || currentAltitude > maxAltitude)
            {
                TriggerStress();
            }
            else
            {
                if (IsAnimalStressed)
                {
                    CalmAnimal();
                }
            }
        }

        private void TriggerStress()
        {
            IsAnimalStressed = true;
            CurrentStressTimer -= Time.deltaTime;

            if (CurrentStressTimer <= 0f)
            {
                CurrentStressTimer = 0f;
                FailMission();
            }
        }

        private void CalmAnimal()
        {
            IsAnimalStressed = false;
            CurrentStressTimer = stressToleranceTime;
        }

        private void FailMission()
        {
            _missionFailed = true;
            IsAnimalStressed = false;
            
            Debug.Log("[AnimalCargoManager] El animal ha sucumbido al estrés. Misión fallida.");
            
            if (_plane != null)
            {
                // Provocar el choque del avión como penalización
                _plane.ForceCrash();
            }
        }
    }
}
