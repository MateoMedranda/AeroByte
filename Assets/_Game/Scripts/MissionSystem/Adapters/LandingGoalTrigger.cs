using UnityEngine;
using UnityEngine.Events;
using FlightSystem.Adapters;

namespace MissionSystem.Adapters
{
    [RequireComponent(typeof(BoxCollider))]
    public class LandingGoalTrigger : MonoBehaviour
    {
        public static bool IsPlaneInAnyLandingZone { get; private set; }

        [Header("Configuración de Aterrizaje")]
        [Tooltip("Velocidad máxima en m/s permitida para dar el aterrizaje por válido.")]
        public float maxLandingSpeed = 15f;
        [Tooltip("Si está marcado, obligará a que el acelerador (throttle) esté en 0 o casi 0 para completar.")]
        public bool requireZeroThrottle = true;

        [Header("Eventos de Misión")]
        public UnityEvent OnLandingCompleted;

        private bool _completed = false;
        private bool _promptedThrottle = false;

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnDestroy()
        {
            IsPlaneInAnyLandingZone = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            PlaneController plane = other.GetComponentInParent<PlaneController>();
            if (plane != null)
            {
                IsPlaneInAnyLandingZone = true;
                Debug.Log("[LandingGoalTrigger] Avión en zona de aterrizaje. Alarma de altitud del animal desactivada.", this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            PlaneController plane = other.GetComponentInParent<PlaneController>();
            if (plane != null)
            {
                IsPlaneInAnyLandingZone = false;
                _promptedThrottle = false;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (_completed) return;

            PlaneController plane = other.GetComponentInParent<PlaneController>();
            if (plane == null) return;

            // Verificar velocidad del avión
            float speed = plane.Body != null ? plane.Body.linearVelocity.magnitude : 0f;
            if (speed > maxLandingSpeed)
            {
                return; // Aún va muy rápido
            }

            // Verificar si el motor (throttle) está en 0
            if (requireZeroThrottle)
            {
                float throttle = plane.State != null ? plane.State.throttle : 0f;
                if (throttle > 0.05f)
                {
                    if (!_promptedThrottle)
                    {
                        Debug.Log("[LandingGoalTrigger] El avión ha llegado a la meta. ¡Baja la aceleración (throttle) a 0 para apagar el motor y completar la misión!");
                        _promptedThrottle = true;
                    }
                    return;
                }
            }

            CompleteMission(plane);
        }

        private void CompleteMission(PlaneController plane)
        {
            if (_completed) return;
            _completed = true;

            Debug.Log("[LandingGoalTrigger] ¡ATERRIZAJE Y APAGADO DE MOTOR EXITOSO! Misión completada.");

            // Si hay un AeroByteDeliveryManager en la escena, completamos la zona actual para activar UI o victoria
            if (AeroByteDeliveryManager.Instance != null)
            {
                var activeZone = AeroByteDeliveryManager.Instance.GetCurrentActiveZone();
                if (activeZone != null)
                {
                    AeroByteDeliveryManager.Instance.RegisterDeliveryComplete(activeZone);
                }
            }

            OnLandingCompleted?.Invoke();
        }
    }
}
