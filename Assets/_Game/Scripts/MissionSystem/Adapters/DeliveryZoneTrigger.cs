using UnityEngine;

namespace MissionSystem.Adapters
{
    [RequireComponent(typeof(Collider))]
    public class DeliveryZoneTrigger : MonoBehaviour
    {
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
                deliveryController.SetInDeliveryZone(true);
                Debug.Log("[DeliveryZoneTrigger] Plane entered the delivery zone.", this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var deliveryController = other.GetComponentInParent<PlaneDeliveryController>();
            if (deliveryController != null)
            {
                deliveryController.SetInDeliveryZone(false);
                Debug.Log("[DeliveryZoneTrigger] Plane exited the delivery zone.", this);
            }
        }
    }
}
