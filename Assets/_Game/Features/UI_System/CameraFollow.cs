using UnityEngine;

namespace AeroByte.UI_System 
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Configuración de Seguimiento")]
        [SerializeField] private Transform target; 
        
        [Tooltip("Posición relativa de la cámara (X, Y, Z)")]
        [SerializeField] private Vector3 offset = new Vector3(0, 5, -10); 
        
        [Tooltip("Tiempo en segundos que tarda la cámara en alcanzar al avión. Menor = más rígido.")]
        [SerializeField] private float smoothTime = 0.05f; 

        // SmoothDamp necesita esta variable para guardar la velocidad actual de la cámara en la memoria RAM
        private Vector3 cameraVelocity = Vector3.zero;

        private void LateUpdate()
        {
            if (target == null) return;

            // 1. Calculamos la posición ideal (detrás del avión)
            Vector3 desiredPosition = target.position + target.TransformDirection(offset);
            
            // 2. Aplicamos la matemática avanzada de SmoothDamp
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref cameraVelocity, smoothTime);
            
            // 3. Miramos fijamente al objetivo
            transform.LookAt(target);
        }
        // Cambiamos LateUpdate por FixedUpdate para que corra al mismo ritmo exacto que el Rigidbody del avión
        private void FixedUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + target.TransformDirection(offset);
            
            // OJO: Como estamos en FixedUpdate, SmoothDamp funciona mejor si usamos Time.fixedDeltaTime, 
            // pero internamente SmoothDamp ya lee Time.deltaTime. Sin embargo, en un entorno puramente físico, 
            // a veces es mejor dejarlo así o pasar la matemática manual. Para SmoothDamp, Unity recomienda dejarlo normal.
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref cameraVelocity, smoothTime);
            
            transform.LookAt(target);
        }
    }
}