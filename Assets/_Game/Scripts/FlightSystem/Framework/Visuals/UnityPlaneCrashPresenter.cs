using UnityEngine;
using FlightSystem.Domain.Interfaces;

namespace FlightSystem.Framework.Visuals
{
    [RequireComponent(typeof(Rigidbody))]
    public class UnityPlaneCrashPresenter : MonoBehaviour, IPlaneCrashPresenter
    {
        [Header("Efectos Visuales y Sonido")]
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private GameObject visualModelContainer; // Contenedor del modelo 3D para desactivar
        [Tooltip("Archivo de audio (.wav / .mp3) de la explosión. ¡Puedes arrastrar tu archivo .wav directamente aquí!")]
        [SerializeField] private AudioClip explosionSoundClip;
        [Tooltip("Volumen del sonido de explosión.")]
        [Range(0f, 1f)]
        [SerializeField] private float explosionVolume = 1f;

        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void PresentCrash()
        {
            Debug.Log("[UnityPlaneCrashPresenter] ¡EL AVION HA EXPLOTADO!");

            // 1. Instanciar explosión visual
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, transform.rotation);
            }
            else
            {
                Debug.LogWarning("No se asignó Prefab de explosión en el Presenter.");
            }

            // 2. Reproducir sonido de explosión (.wav / .mp3)
            if (explosionSoundClip != null)
            {
                GameObject audioObj = new GameObject("ExplosionAudio_" + explosionSoundClip.name);
                audioObj.transform.position = transform.position;
                AudioSource src = audioObj.AddComponent<AudioSource>();
                src.clip = explosionSoundClip;
                src.volume = explosionVolume;
                src.spatialBlend = 0.3f; // Mayormente 2D para que se escuche fuerte y claro desde cualquier cámara
                src.Play();
                Destroy(audioObj, explosionSoundClip.length + 0.5f);
            }

            // 3. Desactivar el modelo visual
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

            // 4. Apagar físicas del avión
            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.isKinematic = true;
            }

            // 5. Desactivar scripts auxiliares para evitar que sigan corriendo
            var visuals = GetComponent<PlaneVisuals>();
            if (visuals != null) visuals.enabled = false;
        }
    }
}
