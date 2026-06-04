using UnityEngine; 

namespace FlightSystem.Domain.Interfaces
{
    public interface IPlaneStats 
    {
        // Basic properties
        float MaxThrust { get; }
        float ThrottleSpeed { get; }
        float GLimit { get; }
        float GLimitPitch { get; }
        float CrashVelocityThreshold { get; }

        // Steering properties
        Vector3 TurnSpeed { get; }
        Vector3 TurnAcceleration { get; }
        AnimationCurve SteeringCurve { get; }

        // Lift and drag properties
        float LiftPower { get; }
        AnimationCurve LiftAOACurve { get; }
        float InducedDrag { get; }
        AnimationCurve InducedDragCurve { get; }
        float RudderPower { get; }
        AnimationCurve RudderAOACurve { get; }
        AnimationCurve RudderInducedDragCurve { get; }
        float FlapsLiftPower { get; }
        float FlapsAOABias { get; }
        float FlapsDrag { get; }
        float FlapsRetractSpeed { get; }

        // Drag properties
        AnimationCurve DragForward { get; }
        AnimationCurve DragBack { get; }
        AnimationCurve DragLeft { get; }
        AnimationCurve DragRight { get; }
        AnimationCurve DragTop { get; }
        AnimationCurve DragBottom { get; }
        Vector3 AngularDrag { get; }
        float AirbrakeDrag { get; }
        bool HasRetractableGear { get; }
        float LandingGearDrag { get; }

        // Aerodynamic properties
        float PitchStability { get; }
        float YawStability { get; }
        float StallAngle { get; }
        float StallTorque { get; }

        // Ground handling properties 
        float GroundSteeringSpeed { get; }
        float GroundBrakeDeceleration { get; }
    }
}