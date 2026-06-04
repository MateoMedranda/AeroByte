using UnityEngine; 

namespace FlightSystem.Domain.Entities
{
    public class PlaneState
    {
        public event System.Action<bool> OnLandingGearStateChanged;

        // State properties
        public float throttle { get; private set; }
        public bool flapsDeployed { get; private set; }
        public bool airbrakeDeployed { get; private set; }
        public bool isGrounded { get; private set; }
        public bool lightsOn { get; private set; }
        public bool landingGearDown { get; private set; } = true;
        public bool isCrashed { get; private set; }

        // abstract state properties
        public Vector3 velocity { get; private set; }
        public Vector3 localVelocity { get; private set; }
        public Vector3 localAngularVelocity { get; private set; }
        public Vector3 localGForce { get; private set; }
        public float angleOfAttack { get; private set; }
        public float angleOfAttackYaw { get; private set; }

        // player input properties
        public float throttleInput { get; private set; }
        public Vector3 controlInput { get; private set; }
        public Vector3 effectiveInput { get; private set; }


        // Constructor
        public PlaneState(float initialSpeed)
        {
            velocity = new Vector3(0, 0, initialSpeed);
            throttle = 0f;
            flapsDeployed = false;
        }
        
        public void SetThrottleInput(float input)
        {
            if (isCrashed) return;
            throttleInput = input;
        }
        public void SetEffectiveInput(Vector3 input) => effectiveInput = input;
        public void SetControlInput(Vector3 input)
        {
            if (isCrashed) return;
            controlInput = Vector3.ClampMagnitude(input, 1);
        }
        public void Crash() => isCrashed = true;
        public void SetGroundedState(bool isGrounded) => this.isGrounded = isGrounded;

        public void UpdateThrottle(float dt, float throttleSpeed) {
            throttle += throttleInput * throttleSpeed * dt;
            throttle = Mathf.Clamp01(throttle);
            airbrakeDeployed = throttle <= 0.01f && throttleInput < 0;
        }

        public void UpdateFlaps(float retractSpeed) {
            if (localVelocity.z > retractSpeed) {
                flapsDeployed = false;
            }
        }

        public void ToggleFlaps(float retractSpeed) {
            if (localVelocity.z < retractSpeed) {
                flapsDeployed = !flapsDeployed;
            }
        }

        public void ToggleLights() {
            lightsOn = !lightsOn;
        }

        public void ToggleLandingGear(bool hasRetractableGear) {
            Debug.Log($"[DEBUG] ToggleLandingGear en PlaneState. hasRetractable: {hasRetractableGear}, landingGearDown: {landingGearDown}, isGrounded: {isGrounded}");
            if (!hasRetractableGear) {
                Debug.Log("[DEBUG] Bloqueado: El avión no tiene tren retráctil configurado en sus stats.");
                return;
            }
            if (landingGearDown && isGrounded) {
                Debug.Log("[DEBUG] Bloqueado: No se puede retraer el tren porque el avión detecta que está en tierra (isGrounded = true).");
                return; 
            }
            
            landingGearDown = !landingGearDown;
            Debug.Log($"[DEBUG] Éxito: Tren cambiado a {landingGearDown}. Disparando evento...");
            OnLandingGearStateChanged?.Invoke(landingGearDown);
        }

        public void SyncPhysicsState(Vector3 velocity, Vector3 localVelocity, Vector3 localAngular, Vector3 localGForce)
        {
            this.velocity = velocity;
            this.localVelocity = localVelocity;
            this.localAngularVelocity = localAngular;
            this.localGForce = localGForce;
            
            CalculateAngleOfAttack(); 
        }

        private void CalculateAngleOfAttack()
        {
            if (localVelocity.sqrMagnitude < 0.1f) {
                angleOfAttack = 0;
                angleOfAttackYaw = 0;
                return;
            }
            angleOfAttack = Mathf.Atan2(-localVelocity.y, localVelocity.z);
            angleOfAttackYaw = Mathf.Atan2(localVelocity.x, localVelocity.z);
        }        
    }
}