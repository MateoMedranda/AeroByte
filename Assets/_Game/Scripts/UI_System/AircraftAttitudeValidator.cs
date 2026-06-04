using UnityEngine;

namespace AeroByte.UI_System
{
    public static class AircraftAttitudeValidator
    {
        public static void GetPitchRoll(Transform aircraft, out float pitch, out float roll)
        {
            Vector3 forward = aircraft.forward;
            Vector3 flatForward = Vector3.ProjectOnPlane(forward, Vector3.up);

            if (flatForward.sqrMagnitude < 0.0001f)
            {
                flatForward = Vector3.ProjectOnPlane(aircraft.up, Vector3.up);
            }

            if (flatForward.sqrMagnitude < 0.0001f)
            {
                pitch = NormalizeSignedAngle(forward.y >= 0f ? 90f : -90f);
                roll = 0f;
                return;
            }

            Vector3 flatForwardNormalized = flatForward.normalized;
            pitch = NormalizeSignedAngle(Vector3.SignedAngle(flatForwardNormalized, forward.normalized, aircraft.right));

            Vector3 worldUpOnForwardPlane = Vector3.ProjectOnPlane(Vector3.up, forward);
            Vector3 aircraftUpOnForwardPlane = Vector3.ProjectOnPlane(aircraft.up, forward);
            if (worldUpOnForwardPlane.sqrMagnitude < 0.0001f || aircraftUpOnForwardPlane.sqrMagnitude < 0.0001f)
            {
                roll = 0f;
            }
            else
            {
                roll = NormalizeSignedAngle(Vector3.SignedAngle(worldUpOnForwardPlane.normalized, aircraftUpOnForwardPlane.normalized, forward.normalized));
            }
        }

        public static float NormalizeSignedAngle(float angle)
        {
            return Mathf.DeltaAngle(0f, angle);
        }

        public static float NormalizeHeading(float angle)
        {
            float normalized = angle % 360f;
            return normalized < 0f ? normalized + 360f : normalized;
        }
    }
}
