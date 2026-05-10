using UnityEngine;
using UnityEngine.InputSystem;

// Esto asegura que si pones este script, Unity añadirá automáticamente el script Plane
[RequireComponent(typeof(Plane))] 
public class PlaneInputManager : MonoBehaviour
{
    private Plane avion;
    private AvionControles controles;

    private void Awake()
    {
        // Conectamos con el motor de físicas del avión
        avion = GetComponent<Plane>();
        
        // Inicializamos tus controles
        controles = new AvionControles();
    }

    private void OnEnable()
    {
        controles.Enable();
    }

    private void OnDisable()
    {
        controles.Disable();
    }

    private void Update()
    {
        // 1. Leer los valores de tu Input System
        Vector2 pitchRoll = controles.Vuelo.PitchRoll.ReadValue<Vector2>();
        float yaw = controles.Vuelo.Yaw.ReadValue<float>();
        float throttle = controles.Vuelo.Throttle.ReadValue<float>(); // Debe ir de -1 a 1

        // 2. Empaquetar los controles en un Vector3
        // X = Pitch (Nariz arriba/abajo)
        // Y = Yaw (Timón izquierda/derecha)
        // Z = Roll (Girar sobre sí mismo)
        // Nota: Le ponemos un signo negativo al Roll (-pitchRoll.x) para que se sienta natural al jugar.
        Vector3 controlInput = new Vector3(pitchRoll.y, yaw, -pitchRoll.x);

        // 3. Enviar las órdenes al avión
        avion.SetControlInput(controlInput);
        
        // El script Plane espera un acelerador de -1 (freno de aire) a 1 (aceleración máxima)
        avion.SetThrottleInput(throttle);

        // Ejemplo para los Flaps (puedes crear un botón 'ToggleFlaps' en tu Input System)
        // if (Keyboard.current.fKey.wasPressedThisFrame) avion.ToggleFlaps();
    }
}