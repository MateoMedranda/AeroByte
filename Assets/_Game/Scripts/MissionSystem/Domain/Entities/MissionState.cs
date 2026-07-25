using System;

namespace MissionSystem.Domain.Entities
{
    public class MissionState
    {
        public bool IsInDeliveryZone { get; private set; }
        public bool IsDeliveryCompleted { get; private set; }

        public event Action<bool> OnInDeliveryZoneChanged;
        public event Action<bool> OnDeliveryCompletedChanged;

        public void SetInDeliveryZone(bool inZone)
        {
            if (IsInDeliveryZone != inZone)
            {
                IsInDeliveryZone = inZone;
                OnInDeliveryZoneChanged?.Invoke(IsInDeliveryZone);
            }
        }

        public void CompleteDelivery()
        {
            if (!IsDeliveryCompleted)
            {
                IsDeliveryCompleted = true;
                OnDeliveryCompletedChanged?.Invoke(IsDeliveryCompleted);
            }
        }

        public void ResetDelivery()
        {
            IsDeliveryCompleted = false;
        }
    }
}
