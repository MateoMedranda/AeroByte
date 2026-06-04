using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AeroByte.WeatherSystem.Adapters
{
    /// <summary>
    /// Utility script to ensure all cameras activated during play mode have HDR and Post-Processing enabled.
    /// This is crucial in URP when cameras are spawned or switched dynamically at runtime.
    /// </summary>
    [DefaultExecutionOrder(-100)] // Run before other scripts
    public class CameraSetupHelper : MonoBehaviour
    {
        private void Awake()
        {
            ConfigureAllCameras();
        }

        private void OnEnable()
        {
            ConfigureAllCameras();
        }

        private void Update()
        {
            // Continuously check to catch any dynamically spawned cameras (e.g. Flight Camera, Orbit Camera)
            ConfigureAllCameras();
        }

        private void ConfigureAllCameras()
        {
            foreach (Camera cam in Camera.allCameras)
            {
                // Force enable HDR
                if (!cam.allowHDR)
                {
                    cam.allowHDR = true;
                }

                // Force enable URP Post-Processing
                var cameraData = cam.GetComponent<UniversalAdditionalCameraData>();
                if (cameraData != null)
                {
                    if (!cameraData.renderPostProcessing)
                    {
                        cameraData.renderPostProcessing = true;
                        Debug.Log($"[CameraSetupHelper] Force-enabled URP Post-Processing & HDR on active camera: {cam.name}");
                    }
                }
                else
                {
                    // If the URP camera data component is missing, add it
                    cameraData = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
                    cameraData.renderPostProcessing = true;
                    Debug.Log($"[CameraSetupHelper] Added UniversalAdditionalCameraData and enabled Post-Processing on: {cam.name}");
                }
            }
        }
    }
}
