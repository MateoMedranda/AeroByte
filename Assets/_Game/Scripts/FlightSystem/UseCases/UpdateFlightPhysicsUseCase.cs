using UnityEngine;
using FlightSystem.Domain.Entities;
using FlightSystem.Domain.Interfaces;

namespace FlightSystem.UseCases
{
    public class FlightPhysicsUseCase
    {
        private readonly IPhysicsGateway _physics;
        private readonly IPlaneStats _stats;

        public FlightPhysicsUseCase(IPhysicsGateway physics, IPlaneStats stats)
        {
            _physics = physics;
            _stats = stats;
        }

        public void Execute(PlaneState state, float dt)
        {
            state.UpdateThrottle(dt, _stats.ThrottleSpeed);
            state.UpdateFlaps(_stats.FlapsRetractSpeed);
            UpdateThrust(state);
            UpdateLift(state);
            UpdateSteering(state, dt);
            UpdateDrag(state);
            UpdateAngularDrag(state);
            UpdateGroundPhysics(state,dt);
        }

        private void UpdateDrag(PlaneState state) 
        {
            var lv = state.localVelocity;
            var lv2 = lv.sqrMagnitude;  

            float airbrakeDrag = state.airbrakeDeployed ? _stats.AirbrakeDrag : 0;
            float flapsDrag = state.flapsDeployed ? _stats.FlapsDrag : 0;

            var coefficient = Utilities.Scale6(
                lv.normalized,
                _stats.DragRight.Evaluate(Mathf.Abs(lv.x)), _stats.DragLeft.Evaluate(Mathf.Abs(lv.x)),
                _stats.DragTop.Evaluate(Mathf.Abs(lv.y)), _stats.DragBottom.Evaluate(Mathf.Abs(lv.y)),
                _stats.DragForward.Evaluate(Mathf.Abs(lv.z)) + airbrakeDrag + flapsDrag,   
                _stats.DragBack.Evaluate(Mathf.Abs(lv.z))
            );

            var drag = coefficient.magnitude * lv2 * -lv.normalized; 
            _physics.ApplyRelativeForce(drag);
        }

        private void UpdateAngularDrag(PlaneState state) 
        {
            var av = state.localAngularVelocity;
            var drag = av.sqrMagnitude * -av.normalized;    
            _physics.ApplyRelativeTorque(Vector3.Scale(drag, _stats.AngularDrag), ForceMode.Acceleration); 
        }

        private void UpdateGroundPhysics(PlaneState state, float dt) 
        {
            if (!state.isGrounded) return;
            
            Vector3 localVel = state.localVelocity;
            
            localVel.x = Mathf.Lerp(localVel.x, 0, dt * 10f); // 10f asegura que no derrape.
            
            if (state.airbrakeDeployed || state.throttle < 0) {
                if (localVel.z > 0.1f) {
                    localVel.z = Mathf.Max(0, localVel.z - (_stats.GroundBrakeDeceleration * dt));
                }
            }
            
            _physics.ApplyTransformDirection(localVel);
        }

        private void UpdateThrust(PlaneState state)
        {
            Vector3 thrustForce = state.throttle * _stats.MaxThrust * Vector3.forward;
            _physics.ApplyRelativeForce(thrustForce);
        }

        private void UpdateLift(PlaneState state)
        {
            if (state.localVelocity.sqrMagnitude < 1f) return;

            float flapsLiftPower = state.flapsDeployed ? _stats.FlapsLiftPower : 0;
            float flapsAOABias = state.flapsDeployed ? _stats.FlapsAOABias : 0;

            Vector3 liftForce = CalculateLift(
                state.localVelocity,
                state.angleOfAttack + (flapsAOABias * Mathf.Deg2Rad), 
                Vector3.right,
                _stats.LiftPower + flapsLiftPower,
                _stats.LiftAOACurve,
                _stats.InducedDragCurve
            );

            Vector3 yawForce = CalculateLift(
                state.localVelocity,
                state.angleOfAttackYaw, 
                Vector3.up, 
                _stats.RudderPower, 
                _stats.RudderAOACurve, 
                _stats.RudderInducedDragCurve
            );

            _physics.ApplyRelativeForce(liftForce);
            _physics.ApplyRelativeForce(yawForce);
        }

        private Vector3 CalculateLift(Vector3 localVelocity, float angleOfAttack, Vector3 rightAxis, float liftPower, AnimationCurve aoaCurve, AnimationCurve inducedDragCurve) 
        {
            var liftVelocity = Vector3.ProjectOnPlane(localVelocity, rightAxis);    
            var v2 = liftVelocity.sqrMagnitude;                                     

            var liftCoefficient = aoaCurve.Evaluate(angleOfAttack * Mathf.Rad2Deg);
            var liftForce = v2 * liftCoefficient * liftPower;

            var liftDirection = Vector3.Cross(liftVelocity.normalized, rightAxis);
            var lift = liftDirection * liftForce;

            var dragForce = liftCoefficient * liftCoefficient;
            var dragDirection = -liftVelocity.normalized;
            var inducedDrag = dragDirection * v2 * dragForce * _stats.InducedDrag * inducedDragCurve.Evaluate(Mathf.Max(0, localVelocity.z));

            return lift + inducedDrag;
        }

        private float CalculateSteering(float dt, float angularVelocity, float targetVelocity, float acceleration) {
            var error = targetVelocity - angularVelocity;
            var accel = acceleration * dt;
            return Mathf.Clamp(error, -accel, accel);
        }

        private void UpdateSteering(PlaneState state, float dt)
        {
            var speed = Mathf.Max(0, state.localVelocity.z);
            var steeringPower = _stats.SteeringCurve.Evaluate(speed);
            var gForceScaling = CalculateGLimiter(state.localVelocity, state.controlInput, _stats.TurnSpeed * Mathf.Deg2Rad * steeringPower);
            var targetAV = Vector3.Scale(state.controlInput, _stats.TurnSpeed * steeringPower * gForceScaling);
            
            targetAV += CalculateAerodynamicStabilityAV(state.localVelocity, _stats.PitchStability, _stats.YawStability);
            targetAV += CalculateStallAV(state.angleOfAttack, _stats.StallAngle, _stats.StallTorque);

            float currentYawAccel = _stats.TurnAcceleration.y * steeringPower;

            if (state.isGrounded) {
                targetAV.y = state.controlInput.y * _stats.GroundSteeringSpeed;
                currentYawAccel = _stats.GroundSteeringSpeed * 10f; 
            }
            var av = state.localAngularVelocity * Mathf.Rad2Deg;
            var correction = new Vector3(
                CalculateSteering(dt, av.x, targetAV.x, _stats.TurnAcceleration.x * steeringPower),
                CalculateSteering(dt, av.y, targetAV.y, currentYawAccel),
                CalculateSteering(dt, av.z, targetAV.z, _stats.TurnAcceleration.z * steeringPower)
            );
            
            _physics.ApplyRelativeTorque(correction * Mathf.Deg2Rad, ForceMode.VelocityChange);
            var correctionInput = new Vector3(
                Mathf.Clamp((targetAV.x - av.x) / _stats.TurnAcceleration.x, -1, 1),
                Mathf.Clamp((targetAV.y - av.y) / _stats.TurnAcceleration.y, -1, 1),
                Mathf.Clamp((targetAV.z - av.z) / _stats.TurnAcceleration.z, -1, 1)
            );

            var effectiveInput = (correctionInput + state.controlInput) * gForceScaling;
            state.SetEffectiveInput(new Vector3(
                Mathf.Clamp(effectiveInput.x, -1, 1),
                Mathf.Clamp(effectiveInput.y, -1, 1),
                Mathf.Clamp(effectiveInput.z, -1, 1)
            ));
        }

        private Vector3 CalculateGForce(Vector3 angularVelocity, Vector3 velocity) {
            return Vector3.Cross(angularVelocity, velocity);
        }

        private Vector3 CalculateGForceLimit(Vector3 input) {
            return Utilities.Scale6(input,
                _stats.GLimit, _stats.GLimitPitch,    
                _stats.GLimit, _stats.GLimit,         
                _stats.GLimit, _stats.GLimit          
            ) * 9.81f;
        }

        private float CalculateGLimiter(Vector3 localVelocity,Vector3 controlInput, Vector3 maxAngularVelocity) {
            if (controlInput.magnitude < 0.01f) {
                return 1;
            }

            var maxInput = controlInput.normalized;
            var limit = CalculateGForceLimit(maxInput);
            var maxGForce = CalculateGForce(Vector3.Scale(maxInput, maxAngularVelocity), localVelocity);

            if (maxGForce.magnitude > limit.magnitude) {
                return limit.magnitude / maxGForce.magnitude;
            }

            return 1;
        }

        private Vector3 CalculateAerodynamicStabilityAV(Vector3 localVelocity, float pitchStability, float yawStability) {
            Vector3 aeroAV = Vector3.zero;
            if (localVelocity.sqrMagnitude > 1f) {
                float sideSlipAngle = Mathf.Atan2(localVelocity.x, localVelocity.z) * Mathf.Rad2Deg;
                float pitchSlipAngle = Mathf.Atan2(-localVelocity.y, localVelocity.z) * Mathf.Rad2Deg;
                
                aeroAV.y = sideSlipAngle * yawStability * 0.1f;
                aeroAV.x = pitchSlipAngle * pitchStability * 0.1f;
            }
            return aeroAV;
        }

        private Vector3 CalculateStallAV(float angleOfAttack, float stallAngle, float stallTorque) {
            Vector3 stallAV = Vector3.zero;
            float currentAoA = angleOfAttack * Mathf.Rad2Deg;
            
            if (currentAoA > stallAngle) {
                float stallSeverity = (currentAoA - stallAngle) / 10f; 
                stallSeverity = Mathf.Clamp01(stallSeverity);
                stallAV.x = stallTorque * stallSeverity * 0.02f; 
            }
            return stallAV;
        }
    }
}