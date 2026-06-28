using UnityEngine;
using UnityEngine.InputSystem;
using FlightSystem.Adapters;
using MissionSystem.Adapters;
using AeroByte.FlightSystem.Framework.Audio;

[RequireComponent(typeof(PlaneController))] 
public class PlaneInputManager : MonoBehaviour
{
    private PlaneController avion;
    private CameraController cameraController;
    private AvionControles controles;

    private void Awake()
    {
        avion = GetComponent<PlaneController>();
        
        controles = new AvionControles();

        if (cameraController == null)
        {
            cameraController = Object.FindFirstObjectByType<CameraController>();
        }
    }

    private void OnEnable()
    {
        controles.Enable();
        controles.Vuelo.ToggleFlaps.performed += OnFlapsInput;
        controles.Vuelo.ToggleCamera.performed += CameraToggle;
        
        // ¡OJO! Tendrás que crear la acción 'ToggleLights', 'ToggleLandingGear', y 'ToggleMusic' en tu archivo AvionControles.inputactions
        controles.Vuelo.ToggleLights.performed += OnLightsInput;
        controles.Vuelo.ToggleLandingGear.performed += OnLandingGearInput;
        controles.Vuelo.ToggleMusic.performed += OnToggleMusicInput;
    }

    private void OnDisable()
    {
        controles.Vuelo.ToggleFlaps.performed -= OnFlapsInput;
        controles.Vuelo.ToggleCamera.performed -= CameraToggle;
        controles.Vuelo.ToggleLights.performed -= OnLightsInput;
        controles.Vuelo.ToggleLandingGear.performed -= OnLandingGearInput;
        controles.Vuelo.ToggleMusic.performed -= OnToggleMusicInput;
        controles.Disable();
    }

    private void Update()
    {
        Vector2 pitchRoll = controles.Vuelo.PitchRoll.ReadValue<Vector2>();
        float yaw = controles.Vuelo.Yaw.ReadValue<float>();
        float throttle = controles.Vuelo.Throttle.ReadValue<float>(); 
        
        Vector3 controlInput = new Vector3(pitchRoll.y, yaw, -pitchRoll.x);

        avion.OnControlInput(controlInput);
        avion.OnThrottleInput(throttle);

        // Detect M key to drop cargo box
        if (Keyboard.current != null)
        {
            if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                var deliveryController = GetComponent<PlaneDeliveryController>();
                if (deliveryController != null)
                {
                    deliveryController.TryDropCargo();
                }
            }

            // Radio Controls (Fallback / Hardcoded until added to InputActions)
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                var radioManager = GetComponent<RadioManager>();
                if (radioManager != null) radioManager.ToggleMusic();
            }

            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                var radioManager = GetComponent<RadioManager>();
                if (radioManager != null) radioManager.NextTrack();
            }
        }
    }

    public void OnFlapsInput(InputAction.CallbackContext context) {
        if (avion == null) return;

        if (context.phase == InputActionPhase.Performed) {
            avion.OnToggleFlaps();
        }
    }

    public void OnLightsInput(InputAction.CallbackContext context) {
        if (avion == null) return;

        if (context.phase == InputActionPhase.Performed) {
            avion.OnToggleLights();
        }
    }

    public void OnLandingGearInput(InputAction.CallbackContext context) {
        if (avion == null) return;

        if (context.phase == InputActionPhase.Performed) {
            Debug.Log("[DEBUG] Input de teclado (K) detectado en PlaneInputManager.");
            avion.OnToggleLandingGear();
        }
    }

    public void CameraToggle(InputAction.CallbackContext context) {
        if(cameraController == null) return;
        if(context.phase == InputActionPhase.Performed) {
            cameraController.ToggleCamera();
        }
    }

    public void OnToggleMusicInput(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Performed) {
            var radioManager = GetComponent<RadioManager>();
            if (radioManager != null) {
                radioManager.ToggleMusic();
            }
        }
    }
}