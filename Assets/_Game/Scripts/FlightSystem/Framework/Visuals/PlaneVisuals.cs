using UnityEngine;
using FlightSystem.Adapters;
using FlightSystem.Domain.Entities;

namespace FlightSystem.Framework.Visuals
{
    [RequireComponent(typeof(PlaneController))]
    public class PlaneVisuals : MonoBehaviour
    {
        public Transform Propeller;
        public Transform LeftElevator;
        public Transform RightElevator;
        public Transform LeftFlaps;
        public Transform RightFlaps;
        public Transform LeftSpoiler;
        public Transform RightSpoiler;
        public Transform Rudder;
        public Transform AirBrake1;
        public Transform AirBrake2;
        public Transform AirBrake3;
        public Transform AirBrake4;
        public GameObject[] planeLights; // Focos y Conos de luz
        public TrailRenderer[] wingTrails; // Cintas aerodinámicas de las alas
        
        public float maxElevatorDeflection = 25f;
        public float maxFlapsDeflection = 35f;
        public float maxSpoilersDeflection = 35f;
        public float maxRudderDeflection = 35f;

        private Quaternion leftFlapStartRotation;
        private Quaternion rightFlapStartRotation;
        private Quaternion leftSpoilerStartRotation;
        private Quaternion rightSpoilerStartRotation;
        private Quaternion airBrake1StartRotation;
        private Quaternion airBrake2StartRotation;  
        private Quaternion airBrake3StartRotation;
        private Quaternion airBrake4StartRotation;

        private float lerpSpeed = 5f;
        private float currentElevatorAngle = 0f;
        private float currentFlapAngle = 0f;
        private float currentSpoilerAngle = 0f;
        private float currentAirBrakeAngle = 0f;

        private PlaneController _controller;

        private void Awake()
        {
            _controller = GetComponent<PlaneController>();

            if (LeftFlaps != null) leftFlapStartRotation = LeftFlaps.localRotation;
            if (RightFlaps != null) rightFlapStartRotation = RightFlaps.localRotation;
            
            if (LeftSpoiler != null) leftSpoilerStartRotation = LeftSpoiler.localRotation;
            if (RightSpoiler != null) rightSpoilerStartRotation = RightSpoiler.localRotation;
            
            if (AirBrake1 != null) airBrake1StartRotation = AirBrake1.localRotation;
            if (AirBrake2 != null) airBrake2StartRotation = AirBrake2.localRotation;
            if (AirBrake3 != null) airBrake3StartRotation = AirBrake3.localRotation;
            if (AirBrake4 != null) airBrake4StartRotation = AirBrake4.localRotation;
        }

        private void Update()
        {
            PlaneState state = _controller.GetState();
            if (state == null) return;

            UpdateLightsVisual(state);
            UpdateTrailsVisual(state);
            UpdatePropellerVisual(state);
            UpdateElevatorVisual(state);
            UpdateFlapsVisual(state);
            UpdateSpoilersVisual(state);
            UpdateRudderVisual(state);
            UpdateAirBrakeVisual(state);
        }

        private void UpdateTrailsVisual(PlaneState state)
        {
            if (wingTrails == null || wingTrails.Length == 0) return;

            // Las estelas aparecen si el avión NO está en el suelo y va a cierta velocidad
            bool isFlying = !state.isGrounded && state.velocity.magnitude > 20f;

            foreach (var trail in wingTrails)
            {
                if (trail == null) continue;

                if (isFlying && !trail.emitting)
                {
                    trail.emitting = true;
                }
                else if (!isFlying && trail.emitting)
                {
                    trail.emitting = false;
                }
            }
        }

        private void UpdateLightsVisual(PlaneState state)
        {
            if (planeLights == null || planeLights.Length == 0) return;

            foreach (var light in planeLights)
            {
                if (light != null && light.activeSelf != state.lightsOn)
                {
                    light.SetActive(state.lightsOn);
                }
            }
        }

        private void UpdatePropellerVisual(PlaneState state)
        {
            if (Propeller != null)
            {
                float propSpeed = (state.throttle * 2000f) + 250f;
                Propeller.Rotate(Vector3.up * state.velocity.magnitude * 40f);
            }
        }

        private void UpdateElevatorVisual(PlaneState state)
        {
            if (LeftElevator != null && RightElevator != null)
            {
                float targetAngle = state.controlInput.x * maxElevatorDeflection * 1.2f; // Ángulo mayor (20% más)
                currentElevatorAngle = Mathf.Lerp(currentElevatorAngle, targetAngle, Time.fixedDeltaTime * lerpSpeed);
                LeftElevator.localRotation = Quaternion.Euler(-currentElevatorAngle, 0f, 0f);
                RightElevator.localRotation = Quaternion.Euler(-currentElevatorAngle, 0f, 0f);
            }
        }

        private void UpdateFlapsVisual(PlaneState state)
        {
            if (LeftFlaps != null && RightFlaps != null)
            {
                float targetAngle = state.flapsDeployed ? maxFlapsDeflection * 1.2f : 0f; 
                currentFlapAngle = Mathf.Lerp(currentFlapAngle, targetAngle, Time.fixedDeltaTime * lerpSpeed);
                LeftFlaps.localRotation = leftFlapStartRotation * Quaternion.Euler(0f, currentFlapAngle, 0f);
                RightFlaps.localRotation = rightFlapStartRotation * Quaternion.Euler(0f, -currentFlapAngle, 0f);
            }
        }

        private void UpdateSpoilersVisual(PlaneState state)
        {
            if (LeftSpoiler != null && RightSpoiler != null)
            {
                float targetAngle = state.controlInput.z * maxSpoilersDeflection * 1.2f; // Ángulo mayor (20% más)
                currentSpoilerAngle = Mathf.Lerp(currentSpoilerAngle, targetAngle, Time.fixedDeltaTime * lerpSpeed);
                LeftSpoiler.localRotation = leftSpoilerStartRotation * Quaternion.Euler(0f, currentSpoilerAngle, 0f);
                RightSpoiler.localRotation = rightSpoilerStartRotation * Quaternion.Euler(0f, currentSpoilerAngle, 0f);
            }
        }

        private void UpdateRudderVisual(PlaneState state) 
        {
            if(Rudder != null) {
                float angle = state.controlInput.y * maxRudderDeflection;
                Rudder.localRotation = Quaternion.Euler(0f, angle, 0f);
            }
        }

        private void UpdateAirBrakeVisual(PlaneState state){
            if (AirBrake1 != null && AirBrake2 != null && AirBrake3 != null && AirBrake4 != null) {
                float targetAngle = state.airbrakeDeployed ? maxFlapsDeflection * 1.5f : 0f; // Ángulo mayor (20% más)
                currentAirBrakeAngle = Mathf.Lerp(currentAirBrakeAngle, targetAngle, Time.fixedDeltaTime * lerpSpeed);
                AirBrake1.localRotation = airBrake1StartRotation * Quaternion.Euler(0f, currentAirBrakeAngle, 0f);
                AirBrake2.localRotation = airBrake2StartRotation * Quaternion.Euler(0f, -currentAirBrakeAngle, 0f);
                AirBrake3.localRotation = airBrake3StartRotation * Quaternion.Euler(0f, currentAirBrakeAngle, 0f);
                AirBrake4.localRotation = airBrake4StartRotation * Quaternion.Euler(0f, -currentAirBrakeAngle, 0f);
            }
        }
    }
}