using UnityEngine;

namespace MissionSystem.Domain.Interfaces
{
    public interface IDeliveryPresenter
    {
        void SpawnCargoBox(Vector3 spawnPosition, Quaternion spawnRotation, Vector3 initialVelocity);
        void UpdateIndicatorSignal(bool completed);
    }
}
