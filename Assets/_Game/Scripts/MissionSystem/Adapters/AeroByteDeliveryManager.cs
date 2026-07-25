using System;
using System.Collections.Generic;
using UnityEngine;

// Script for managing the delivery mission sequence.
namespace MissionSystem.Adapters
{
    public class AeroByteDeliveryManager : MonoBehaviour
    {
        public static AeroByteDeliveryManager Instance { get; private set; }

        [Header("Zonas de Entrega")]
        [Tooltip("Arrastra aquí los objetos con el script DeliveryZoneTrigger en el orden que quieres que se entreguen.")]
        public List<DeliveryZoneTrigger> deliveryZones = new List<DeliveryZoneTrigger>();

        public int CurrentZoneIndex { get; private set; } = 0;
        public int TotalZones => deliveryZones.Count;

        public event Action<int, int> OnMissionProgressChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Initialize zones
            for (int i = 0; i < deliveryZones.Count; i++)
            {
                if (deliveryZones[i] != null)
                {
                    // Active zone gets active color, others get inactive
                    deliveryZones[i].SetState(i == 0 ? ZoneState.Active : ZoneState.Inactive);
                }
            }

            OnMissionProgressChanged?.Invoke(CurrentZoneIndex, TotalZones);
        }

        public DeliveryZoneTrigger GetCurrentActiveZone()
        {
            if (CurrentZoneIndex < deliveryZones.Count)
            {
                return deliveryZones[CurrentZoneIndex];
            }
            return null;
        }

        public void RegisterDeliveryComplete(DeliveryZoneTrigger zone)
        {
            if (GetCurrentActiveZone() == zone)
            {
                Debug.Log($"[AeroByteDeliveryManager] Zona {CurrentZoneIndex + 1} completada.");
                
                // Set current to delivered
                zone.SetState(ZoneState.Delivered);
                
                // Move to next
                CurrentZoneIndex++;
                
                // Activate next if exists
                if (CurrentZoneIndex < deliveryZones.Count)
                {
                    deliveryZones[CurrentZoneIndex].SetState(ZoneState.Active);
                }
                else
                {
                    Debug.Log("[AeroByteDeliveryManager] ¡Misión Completada!");
                }

                OnMissionProgressChanged?.Invoke(CurrentZoneIndex, TotalZones);
            }
        }
    }
}
