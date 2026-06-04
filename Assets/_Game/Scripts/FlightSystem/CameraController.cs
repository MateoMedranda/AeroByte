using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("An array of transforms representing the different camera positions and orientations.")]
    [SerializeField] private Transform[] cameraPositions;
    [Tooltip("The speed at which the camera transitions between positions.")]
    [SerializeField] private float speed;

    private int index = 0; // Empezamos en 0 (la primera cámara)

    private void Update()
    {
        if (cameraPositions == null || cameraPositions.Length == 0) return;

        // Transición suave de posición y rotación en el Update
        transform.position = Vector3.Lerp(transform.position, cameraPositions[index].position, speed * Time.deltaTime);
        transform.forward = cameraPositions[index].forward; // Esto hará que la cámara mire en la dirección del transform objetivo
        //transform.rotation = Quaternion.Lerp(transform.rotation, cameraPositions[index].rotation, speed * Time.deltaTime);
    }

    // Este método será llamado desde el Input Manager
    public void ToggleCamera()
    {
        if (cameraPositions == null || cameraPositions.Length == 0) return;
        Debug.Log("Cambiando cámara...");
        index = (index + 1) % cameraPositions.Length;
    }
}