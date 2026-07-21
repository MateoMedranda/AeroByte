using UnityEngine;
using MissionSystem.Domain.Interfaces;
using AeroByte.WeatherSystem.Adapters;

namespace MissionSystem.Framework
{
    public class UnityDeliveryPresenter : MonoBehaviour, IDeliveryPresenter
    {
        //
        [Header("Referencias de Señalización")]
        [SerializeField] private BlinkingEmission blinkingEmission;
        [SerializeField, ColorUsage(true, true)] private Color pendingColor = Color.red;
        [SerializeField, ColorUsage(true, true)] private Color completedColor = Color.green;

        [Header("Configuración de Carga")]
        [SerializeField] private GameObject cargoBoxPrefab;
        [SerializeField] private Transform dropPoint;
        [SerializeField] private Vector3 boxScale = Vector3.one;

        private void Start()
        {
            if (blinkingEmission == null)
            {
                // Auto-discover if not assigned in inspector
                blinkingEmission = FindFirstObjectByType<BlinkingEmission>();
            }

            // Initialize to pending color at start
            UpdateIndicatorSignal(false);
        }

        public void SpawnCargoBox(Vector3 spawnPosition, Quaternion spawnRotation, Vector3 initialVelocity)
        {
            // If dropPoint is assigned, use it. Otherwise offset downward from the plane's position
            Vector3 targetSpawnPos = dropPoint != null ? dropPoint.position : (spawnPosition - (spawnRotation * Vector3.up * 1.5f));
            
            GameObject boxInstance = null;

            if (cargoBoxPrefab != null)
            {
                Debug.Log($"[UnityDeliveryPresenter] Instanciando prefab configurado: {cargoBoxPrefab.name}");
                boxInstance = Instantiate(cargoBoxPrefab, targetSpawnPos, spawnRotation);
            }
            else
            {
                Debug.LogWarning("[UnityDeliveryPresenter] No se asignó ningún Prefab. Creando un cubo primitivo de respaldo.");
                
                // Fallback: Create a primitive cube to represent the cargo box
                boxInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                boxInstance.name = "CargoBox_Fallback_Cube";
                boxInstance.transform.position = targetSpawnPos;
                boxInstance.transform.rotation = spawnRotation;
                boxInstance.transform.localScale = boxScale;
                
                // Color it brown like a cargo crate
                Renderer renderer = boxInstance.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(0.6f, 0.4f, 0.2f); // Cargo box brown
                }
            }

            if (boxInstance != null)
            {
                // Ensure the box has a Rigidbody for physics simulation
                Rigidbody boxRb = boxInstance.GetComponent<Rigidbody>();
                if (boxRb == null)
                {
                    boxRb = boxInstance.AddComponent<Rigidbody>();
                }

                // Give it the same starting speed as the plane for natural projectile motion
                boxRb.linearVelocity = initialVelocity;
                boxRb.mass = 15f; 
                boxRb.interpolation = RigidbodyInterpolation.Interpolate;

                Debug.Log($"[UnityDeliveryPresenter] Caja soltada con éxito en {targetSpawnPos} con velocidad inicial {initialVelocity}");
            }
            else
            {
                Debug.LogError("[UnityDeliveryPresenter] ERROR crítico: No se pudo crear la instancia del objeto a soltar.");
            }
        }

        public void UpdateIndicatorSignal(bool completed)
        {
            if (blinkingEmission != null)
            {
                Color targetColor = completed ? completedColor : pendingColor;
                blinkingEmission.SetBlinkColor(targetColor);
                Debug.Log($"[UnityDeliveryPresenter] Señal luminosa actualizada. ¿Entregado?: {completed} (Color asignado: {targetColor})");
            }
            else
            {
                Debug.LogWarning("[UnityDeliveryPresenter] No se pudo actualizar el color: BlinkingEmission es nulo.");
            }
        }
    }
}
