using UnityEngine;
using FlightSystem.Domain.Interfaces;

namespace FlightSystem.Framework.Visuals
{
    [RequireComponent(typeof(Rigidbody))]
    public class UnityPlaneCrashPresenter : MonoBehaviour, IPlaneCrashPresenter
    {
        [Header("Efectos Visuales")]
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private GameObject visualModelContainer; // Contenedor del modelo 3D para desactivar

        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void PresentCrash()
        {
            Debug.Log("[UnityPlaneCrashPresenter] ¡EL AVION HA EXPLOTADO!");

            // 1. Instanciar explosión
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, transform.rotation);
            }
            else
            {
                Debug.LogWarning("No se asignó Prefab de explosión en el Presenter.");
            }

            // 2. Desactivar el modelo visual
            if (visualModelContainer != null)
            {
                visualModelContainer.SetActive(false);
            }
            else
            {
                // Fallback: Desactivar Renderers recursivamente
                foreach (var r in GetComponentsInChildren<Renderer>())
                {
                    r.enabled = false;
                }
            }

            // 3. Apagar físicas del avión
            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.isKinematic = true;
            }

            // 4. Desactivar scripts auxiliares para evitar que sigan corriendo
            var visuals = GetComponent<PlaneVisuals>();
            if (visuals != null) visuals.enabled = false;
        }
    }
}
