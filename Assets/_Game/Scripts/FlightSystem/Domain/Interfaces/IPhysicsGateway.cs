using UnityEngine;

namespace FlightSystem.Domain.Interfaces
{
    public interface IPhysicsGateway 
    {
        void ApplyRelativeForce(Vector3 force);
        void ApplyRelativeTorque(Vector3 torque, ForceMode mode);
        void ApplyTransformDirection(Vector3 direction);
    }
}