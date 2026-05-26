using UnityEngine;
using UnityEngine.InputSystem;
using FlightSystem.Adapters;

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
        
        // ¡OJO! Tendrás que crear la acción 'ToggleLights' en tu archivo AvionControles.inputactions
        controles.Vuelo.ToggleLights.performed += OnLightsInput;
    }

    private void OnDisable()
    {
        controles.Vuelo.ToggleFlaps.performed -= OnFlapsInput;
        controles.Vuelo.ToggleCamera.performed -= CameraToggle;
        controles.Vuelo.ToggleLights.performed -= OnLightsInput;
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

    public void CameraToggle(InputAction.CallbackContext context) {
        if(cameraController == null) return;
        if(context.phase == InputActionPhase.Performed) {
            cameraController.ToggleCamera();
        }
    }
}