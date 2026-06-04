using System.Collections;
using UnityEngine;

namespace FlightSystem.Framework.Visuals
{
    public class TimeframeExplosion : MonoBehaviour
    {
        [Header("Configuración de Animación")]
        [Tooltip("Fotogramas por segundo (velocidad de la animación)")]
        [SerializeField] private float frameRate = 15f;
        
        [Tooltip("Destruir el objeto cuando termine la animación")]
        [SerializeField] private bool destroyOnComplete = true;

        private Transform[] _frames;

        private void Start()
        {
            int childCount = transform.childCount;
            if (childCount == 0)
            {
                Debug.LogWarning($"[TimeframeExplosion] El objeto {gameObject.name} no tiene sub-objetos (frames) para animar.");
                if (destroyOnComplete) Destroy(gameObject);
                return;
            }

            // Guardar todos los hijos en orden jerárquico
            _frames = new Transform[childCount];
            for (int i = 0; i < childCount; i++)
            {
                _frames[i] = transform.GetChild(i);
                _frames[i].gameObject.SetActive(false);
            }

            StartCoroutine(PlayExplosionCoroutine());
        }

        private IEnumerator PlayExplosionCoroutine()
        {
            float delay = 1f / frameRate;

            for (int i = 0; i < _frames.Length; i++)
            {
                // Desactivar el frame anterior
                if (i > 0)
                {
                    _frames[i - 1].gameObject.SetActive(false);
                }

                // Activar el frame actual
                _frames[i].gameObject.SetActive(true);

                yield return new WaitForSeconds(delay);
            }

            // Ocultar el último frame
            if (_frames.Length > 0)
            {
                _frames[_frames.Length - 1].gameObject.SetActive(false);
            }

            if (destroyOnComplete)
            {
                Destroy(gameObject);
            }
        }
    }
}
