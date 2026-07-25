using UnityEngine;
using FlightSystem.Adapters;

namespace MissionSystem.Adapters
{
    public class OutOfBoundsManager : MonoBehaviour
    {
        public static OutOfBoundsManager Instance { get; private set; }

        [Header("Settings")]
        [Tooltip("Tiempo en segundos que el jugador tiene para volver al área jugable")]
        public float returnTime = 10f;

        public bool IsOOB { get; private set; }
        public float CurrentTimer { get; private set; }

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

        private void Update()
        {
            if (IsOOB)
            {
                CurrentTimer -= Time.deltaTime;

                if (CurrentTimer <= 0f)
                {
                    CurrentTimer = 0f;
                    TriggerExplosion();
                }
            }
        }

        public void EnterOOB()
        {
            if (!IsOOB)
            {
                IsOOB = true;
                CurrentTimer = returnTime;
                Debug.Log("[OutOfBoundsManager] ¡Jugador fuera de los límites! Iniciando contador.");
            }
        }

        public void ExitOOB()
        {
            if (IsOOB)
            {
                IsOOB = false;
                CurrentTimer = returnTime;
                Debug.Log("[OutOfBoundsManager] Jugador regresó a la zona segura.");
            }
        }

        private void TriggerExplosion()
        {
            // Evitar llamadas múltiples
            IsOOB = false; 

            var plane = FindFirstObjectByType<PlaneController>();
            if (plane != null)
            {
                Debug.Log("[OutOfBoundsManager] ¡Tiempo agotado! Destruyendo el avión.");
                plane.ForceCrash();
            }
            else
            {
                Debug.LogWarning("[OutOfBoundsManager] Tiempo agotado pero no se encontró un PlaneController en la escena.");
            }
        }
    }
}
