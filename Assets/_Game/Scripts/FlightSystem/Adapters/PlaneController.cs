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

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _planeState = new PlaneState(0f); 
            _physicsUseCase = new FlightPhysicsUseCase(this, statsConfig);
            
            _planeState.OnLandingGearStateChanged += HandleLandingGearStateChanged;
        }

        private void OnDestroy() {
            if (_planeState != null) {
                _planeState.OnLandingGearStateChanged -= HandleLandingGearStateChanged;
            }
        }

        private void HandleLandingGearStateChanged(bool isDown) {
            Debug.Log($"[PlaneController] Tren de aterrizaje {(isDown ? "Desplegado" : "Retraído")}. Activando animación...");
            
            Animator animator = GetComponentInChildren<Animator>();
            if (animator != null) {
                animator.SetBool("GearDown", isDown);
            } else {
                Debug.LogWarning("[PlaneController] No se encontró componente Animator en el avión para el tren de aterrizaje.");
            }
        }

        public void OnControlInput(Vector3 input) => _planeState.SetControlInput(input);
        public void OnThrottleInput(float input) => _planeState.SetThrottleInput(input);
        public void OnToggleFlaps() => _planeState.ToggleFlaps(statsConfig.FlapsRetractSpeed);
        public void OnToggleLights() => _planeState.ToggleLights();
        
        public void OnToggleLandingGear() {
            Debug.Log($"[DEBUG] OnToggleLandingGear llamado en PlaneController. Enviando a PlaneState... HasRetractableGear: {statsConfig.HasRetractableGear}, isGrounded actual: {_planeState.isGrounded}");
            _planeState.ToggleLandingGear(statsConfig.HasRetractableGear);
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