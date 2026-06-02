using UnityEngine;
using FlightSystem.Domain.Interfaces;

namespace AeroByte.FlightSystem.Framework.Config
{
    [CreateAssetMenu(fileName = "PlaneStats", menuName = "FlightSystem/Plane Stats Config", order = 0)]
    public class PlaneStatsConfig : ScriptableObject, IPlaneStats 
    {
        [Header("Velocidades y Límites")]
        [SerializeField] private float maxThrust = 15000f;
        [SerializeField] private float throttleSpeed = 0.5f;
        [SerializeField] private float flapsRetractSpeed = 80f;
        [SerializeField] private float gLimit = 9f;
        [SerializeField] private float gLimitPitch = 9f;

        [Header("Curvas y Aerodinámica")]
        [SerializeField] private float liftPower = 50f;
        [SerializeField] private AnimationCurve liftAOACurve;
        [SerializeField] private float rudderPower = 10f;
        [SerializeField] private AnimationCurve rudderAOACurve;
        [SerializeField] private AnimationCurve rudderInducedDragCurve;
        [SerializeField] private float inducedDrag = 1f;
        [SerializeField] private AnimationCurve inducedDragCurve;

        [Header("Flaps y Maniobrabilidad")]
        [SerializeField] private float flapsLiftPower = 20f;
        [SerializeField] private float flapsAOABias = 10f;
        [SerializeField] private float flapsDrag = 5f;
        [SerializeField] private Vector3 turnSpeed = new Vector3(30f, 15f, 60f);
        [SerializeField] private Vector3 turnAcceleration = new Vector3(60f, 30f, 120f);
        [SerializeField] private AnimationCurve steeringCurve;

        [Header("Drag Dinámico")]
        [SerializeField] private AnimationCurve dragForward;
        [SerializeField] private AnimationCurve dragBack;
        [SerializeField] private AnimationCurve dragLeft;
        [SerializeField] private AnimationCurve dragRight;
        [SerializeField] private AnimationCurve dragTop;
        [SerializeField] private AnimationCurve dragBottom;
        [SerializeField] private Vector3 angularDrag = new Vector3(1, 1, 1);
        [SerializeField] private float airbrakeDrag = 50f;

        [Header("Tren de Aterrizaje")]
        [SerializeField] private bool hasRetractableGear = true;
        [SerializeField] private float landingGearDrag = 15f;

        [Header("Estabilidad y Pérdida (Stall)")]
        [SerializeField] private float pitchStability = 5f;
        [SerializeField] private float yawStability = 15f;
        [SerializeField] private float stallAngle = 18f;
        [SerializeField] private float stallTorque = 3000f;

        [Header("Físicas de Suelo")]
        [SerializeField] private float groundSteeringSpeed = 40f;
        [SerializeField] private float groundBrakeDeceleration = 15f;

        // Implementación de la interfaz IPlaneStats
        public float MaxThrust => maxThrust;
        public float ThrottleSpeed => throttleSpeed;
        public float FlapsRetractSpeed => flapsRetractSpeed;
        public float GLimit => gLimit;
        public float GLimitPitch => gLimitPitch;
        
        public float LiftPower => liftPower;
        public AnimationCurve LiftAOACurve => liftAOACurve;
        public float RudderPower => rudderPower;
        public AnimationCurve RudderAOACurve => rudderAOACurve;
        public AnimationCurve RudderInducedDragCurve => rudderInducedDragCurve;
        public float InducedDrag => inducedDrag;
        public AnimationCurve InducedDragCurve => inducedDragCurve;
        
        public float FlapsLiftPower => flapsLiftPower;
        public float FlapsAOABias => flapsAOABias;
        public float FlapsDrag => flapsDrag;
        
        public Vector3 TurnSpeed => turnSpeed;
        public Vector3 TurnAcceleration => turnAcceleration;
        public AnimationCurve SteeringCurve => steeringCurve;

        public AnimationCurve DragForward => dragForward;
        public AnimationCurve DragBack => dragBack;
        public AnimationCurve DragLeft => dragLeft;
        public AnimationCurve DragRight => dragRight;
        public AnimationCurve DragTop => dragTop;
        public AnimationCurve DragBottom => dragBottom;
        public Vector3 AngularDrag => angularDrag;
        public float AirbrakeDrag => airbrakeDrag;
        public bool HasRetractableGear => hasRetractableGear;
        public float LandingGearDrag => landingGearDrag;

        public float PitchStability => pitchStability;
        public float YawStability => yawStability;
        public float StallAngle => stallAngle;
        public float StallTorque => stallTorque;

        public float GroundSteeringSpeed => groundSteeringSpeed;
        public float GroundBrakeDeceleration => groundBrakeDeceleration;
    }
}