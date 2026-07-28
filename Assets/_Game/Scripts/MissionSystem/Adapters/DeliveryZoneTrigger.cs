using UnityEngine;
using UnityEngine.Events;
using FlightSystem.Adapters;

namespace MissionSystem.Adapters
{
    public enum ZoneState
    {
        Inactive,
        Active,
        Delivered
    }

    [RequireComponent(typeof(Collider))]
    public class DeliveryZoneTrigger : MonoBehaviour
    {
        [Header("Configuración Visual")]
        public Renderer zoneRenderer;
        public Color activeColor = Color.yellow;
        public Color inactiveColor = Color.gray;
        public Color deliveredColor = Color.green;

        [Tooltip("Arrastra aquí el script BlinkingEmission del FARO que corresponde a esta zona")]
        public AeroByte.WeatherSystem.Adapters.BlinkingEmission faroAsociado;

        [Header("Modo Aterrizaje / Meta Final")]
        [Tooltip("Si se activa, no pedirá soltar carga con la barra espaciadora. La misión se completará en cuanto el avión aterrice aquí y baje el motor (Throttle) a 0.")]
        public bool isLandingGoal = false;
        [Tooltip("Velocidad máxima (m/s) permitida al tocar tierra para completar la misión.")]
        public float maxLandingSpeed = 15f;
        [Tooltip("Si está marcado, pedirá que el acelerador (throttle) esté en 0 o casi 0 para completar.")]
        public bool requireZeroThrottle = true;

        [Header("Eventos de Aterrizaje")]
        public UnityEvent OnLandingCompleted;

        public ZoneState CurrentState { get; private set; } = ZoneState.Inactive;
        private bool _promptedThrottle = false;

        public void SetState(ZoneState newState)
        {
            CurrentState = newState;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (zoneRenderer == null) return;

            switch (CurrentState)
            {
                case ZoneState.Active:
                    if (zoneRenderer != null)
                    {
                        zoneRenderer.material.SetColor("_EmissionColor", activeColor);
                        zoneRenderer.material.color = activeColor;
                    }
                    if (faroAsociado != null) faroAsociado.SetBlinkColor(activeColor);
                    break;
                case ZoneState.Inactive:
                    if (zoneRenderer != null)
                    {
                        zoneRenderer.material.SetColor("_EmissionColor", inactiveColor);
                        zoneRenderer.material.color = inactiveColor;
                    }
                    if (faroAsociado != null) faroAsociado.SetBlinkColor(inactiveColor);
                    break;
                case ZoneState.Delivered:
                    if (zoneRenderer != null)
                    {
                        zoneRenderer.material.SetColor("_EmissionColor", deliveredColor);
                        zoneRenderer.material.color = deliveredColor;
                    }
                    if (faroAsociado != null) faroAsociado.SetBlinkColor(deliveredColor);
                    break;
            }
        }
        private void Start()
        {
            // Ensure collider is marked as a trigger
            Collider col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
                Debug.LogWarning($"[DeliveryZoneTrigger] Collider on {gameObject.name} was not marked as Trigger. Setting it to Trigger automatically.", this);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Search in parent as colliders can be in child GameObjects of the plane structure
            var deliveryController = other.GetComponentInParent<PlaneDeliveryController>();
            if (deliveryController != null)
            {
                // Only allow delivery if this zone is active
                if (AeroByteDeliveryManager.Instance != null && AeroByteDeliveryManager.Instance.GetCurrentActiveZone() != this)
                {
                    return; // Ignore if it's not the active zone
                }

                deliveryController.SetCurrentZone(this);
                deliveryController.SetInDeliveryZone(true);
                Debug.Log($"[DeliveryZoneTrigger] Plane entered the active delivery zone: {gameObject.name}", this);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!isLandingGoal || CurrentState != ZoneState.Active) return;

            PlaneController plane = other.GetComponentInParent<PlaneController>();
            if (plane == null) return;

            float speed = plane.Body != null ? plane.Body.linearVelocity.magnitude : 0f;
            if (speed > maxLandingSpeed) return; // Aún va muy rápido

            if (requireZeroThrottle)
            {
                float throttle = plane.State != null ? plane.State.throttle : 0f;
                if (throttle > 0.05f)
                {
                    if (!_promptedThrottle)
                    {
                        Debug.Log("[DeliveryZoneTrigger] El avión está en la meta. ¡Baja el acelerador (throttle) a 0 para apagar el motor y terminar la misión!");
                        _promptedThrottle = true;
                    }
                    return;
                }
            }

            Debug.Log("[DeliveryZoneTrigger] ¡ATERRIZAJE Y APAGADO DE MOTOR EN LA META EXITOSO! Misión completada.");
            if (AeroByteDeliveryManager.Instance != null)
            {
                AeroByteDeliveryManager.Instance.RegisterDeliveryComplete(this);
            }
            OnLandingCompleted?.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            var deliveryController = other.GetComponentInParent<PlaneDeliveryController>();
            if (deliveryController != null)
            {
                if (deliveryController.CurrentZone == this)
                {
                    deliveryController.SetCurrentZone(null);
                    deliveryController.SetInDeliveryZone(false);
                    Debug.Log($"[DeliveryZoneTrigger] Plane exited the delivery zone: {gameObject.name}", this);
                }
            }
            _promptedThrottle = false;
        }
    }
}
