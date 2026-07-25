using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("An array of transforms representing the different camera positions and orientations.")]
    [SerializeField] private Transform[] cameraPositions;
    
    [Tooltip("Marca la casilla si la cámara en ese índice es de primera persona (cabina).")]
    public bool[] isFirstPersonCamera;

    [Tooltip("The speed at which the camera transitions between positions.")]
    [SerializeField] private float speed;
    
    [Tooltip("Sensibilidad al mirar con el mouse.")]
    public float mouseSensitivity = 0.2f;

    private int index = 0; // Empezamos en 0 (la primera cámara)
    
    private float currentYaw = 0f;
    private float currentPitch = 0f;
    private Vector2 lookInput;
    private bool isLooking;

    public int CurrentIndex => index;
    public int CameraCount => cameraPositions == null ? 0 : cameraPositions.Length;
    public string CurrentCameraName => cameraPositions == null || cameraPositions.Length == 0 ? "N/A" : cameraPositions[index].name;

    public void SetLookInput(Vector2 delta, bool isLooking) {
        this.lookInput = delta;
        this.isLooking = isLooking;
    }

    public void ResetCameraView() {
        currentYaw = 0f;
        currentPitch = 0f;
    }

    private void Update()
    {
        if (cameraPositions == null || cameraPositions.Length == 0) return;

        bool isFirstPerson = false;
        if (isFirstPersonCamera != null && index < isFirstPersonCamera.Length) {
            isFirstPerson = isFirstPersonCamera[index];
        }

        if (isLooking) {
            currentYaw += lookInput.x * mouseSensitivity;
            currentPitch -= lookInput.y * mouseSensitivity;
            currentPitch = Mathf.Clamp(currentPitch, -89f, 89f);
        } else {
            // Retorno suave al centro si no se está mirando
            currentYaw = Mathf.Lerp(currentYaw, 0f, speed * Time.deltaTime);
            currentPitch = Mathf.Lerp(currentPitch, 0f, speed * Time.deltaTime);
        }

        Transform targetCam = cameraPositions[index];

        if (isFirstPerson) {
            // First Person: Mueve la cabeza desde un punto fijo
            transform.position = Vector3.Lerp(transform.position, targetCam.position, speed * Time.deltaTime);
            
            Quaternion lookRot = Quaternion.Euler(currentPitch, currentYaw, 0f);
            Quaternion targetRotation = targetCam.rotation * lookRot;
            
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);
        } else {
            // Third Person: Orbita alrededor del avión (el parent de la cámara)
            Transform pivot = targetCam.parent; 
            if (pivot == null) pivot = targetCam; // Fallback
            
            Quaternion lookRot = Quaternion.Euler(currentPitch, currentYaw, 0f);
            Quaternion orbitRotation = pivot.rotation * lookRot;
            
            // Calculamos el offset real en el mundo y le quitamos la rotación del avión
            // Esto preserva perfectamente la escala real de Unity sin importar el tamaño del avión.
            Vector3 worldOffset = targetCam.position - pivot.position;
            Vector3 unrotatedOffset = Quaternion.Inverse(pivot.rotation) * worldOffset;
            
            Vector3 targetPosition = pivot.position + orbitRotation * unrotatedOffset;
            Quaternion targetRotation = orbitRotation * targetCam.localRotation;
            
            transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);
        }
    }

    // Este método será llamado desde el Input Manager
    public void ToggleCamera()
    {
        if (cameraPositions == null || cameraPositions.Length == 0) return;
        Debug.Log("Cambiando cámara...");
        index = (index + 1) % cameraPositions.Length;
        ResetCameraView();
    }
}
