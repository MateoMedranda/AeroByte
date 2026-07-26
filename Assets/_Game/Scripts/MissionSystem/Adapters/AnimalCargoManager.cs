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

        public bool IsAnimalStressed { get; private set; }
        public float CurrentStressTimer { get; private set; }
        
        private PlaneController _plane;
        private bool _missionFailed = false;

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
        }

        private void Update()
        {
            if (_missionFailed || _plane == null) return;

            // Obtener la altura real del avión (usando la Y global)
            float currentAltitude = _plane.transform.position.y;

            if (currentAltitude < minAltitude || currentAltitude > maxAltitude)
            {
                // El animal está estresado
                IsAnimalStressed = true;
                CurrentStressTimer -= Time.deltaTime;

                if (CurrentStressTimer <= 0f)
                {
                    CurrentStressTimer = 0f;
                    FailMission();
                }
            }
            else
            {
                // El animal se calma si volvemos a la zona segura
                if (IsAnimalStressed)
                {
                    IsAnimalStressed = false;
                    CurrentStressTimer = stressToleranceTime;
                }
            }
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
