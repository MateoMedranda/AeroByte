using UnityEngine;

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

        public ZoneState CurrentState { get; private set; } = ZoneState.Inactive;

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
        }
    }
}
