using UnityEngine;
using FlightSystem.Domain.Entities;
using FlightSystem.Domain.Interfaces;
using FlightSystem.UseCases;
using AeroByte.FlightSystem.Framework.Config; 

namespace FlightSystem.Adapters
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlaneController : MonoBehaviour, IPhysicsGateway
    {
        [Header("Configuración (¡Usa el ScriptableObject!)")]
        public PlaneStatsConfig statsConfig; 
        
        [Header("Sensores del Avión")]
        public LayerMask groundMask;
        public float groundCheckDistance = 2f;

        private Rigidbody _rb;
        private PlaneState _planeState;
        private FlightPhysicsUseCase _physicsUseCase;
        private Vector3 _lastVelocity;
        private int _weatherZoneContacts;

        public Rigidbody Body => _rb;
        public PlaneState State => _planeState;
        public bool IsInWeatherZone => _weatherZoneContacts > 0;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _planeState = new PlaneState(0f); 
            _physicsUseCase = new FlightPhysicsUseCase(this, statsConfig);
        }

        public void OnControlInput(Vector3 input) => _planeState.SetControlInput(input);
        public void OnThrottleInput(float input) => _planeState.SetThrottleInput(input);
        public void OnToggleFlaps()
        {
            if (statsConfig == null) return;
            _planeState.ToggleFlaps(statsConfig.FlapsRetractSpeed);
        }
        public void OnToggleLights() => _planeState.ToggleLights();

        public void RegisterWeatherZoneEnter()
        {
            _weatherZoneContacts++;
        }

        public void RegisterWeatherZoneExit()
        {
            _weatherZoneContacts = Mathf.Max(0, _weatherZoneContacts - 1);
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;

            _planeState.SetGroundedState(Physics.Raycast(transform.position, -transform.up, groundCheckDistance, groundMask));
            CalculatePlaneState(dt);
            _physicsUseCase.Execute(_planeState, dt);
        }

        private void CalculatePlaneState(float dt) {
            var quaternion = Quaternion.Inverse(_rb.rotation);
            var localGForce = quaternion * ((_rb.linearVelocity - _lastVelocity) / dt);
            _planeState.SyncPhysicsState(
                _rb.linearVelocity, 
                quaternion * _rb.linearVelocity, 
                quaternion * _rb.angularVelocity,
                localGForce
            );
            _lastVelocity = _rb.linearVelocity;
        }

        public void ApplyRelativeForce(Vector3 force) => _rb.AddRelativeForce(force);
        public void ApplyRelativeTorque(Vector3 torque, ForceMode mode) => _rb.AddRelativeTorque(torque, mode);
        public void ApplyTransformDirection(Vector3 direction) => _rb.linearVelocity = transform.TransformDirection(direction);

        public PlaneState GetState() => _planeState;
    }
}
