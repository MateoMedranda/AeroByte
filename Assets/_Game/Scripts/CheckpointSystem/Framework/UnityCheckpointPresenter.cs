using System.Collections;
using System.Collections.Generic;
using AeroByte.CheckpointSystem.Domain.Interfaces;
using AeroByte.CheckpointSystem.Framework.Visuals;
using UnityEngine;

namespace AeroByte.CheckpointSystem.Framework
{
    public class UnityCheckpointPresenter : MonoBehaviour, ICheckpointPresenter
    {
        [Header("Efectos Visuales")]
        [Tooltip("Prefab de partículas a instanciar al pasar por un checkpoint")]
        [SerializeField] private GameObject feedbackParticlesPrefab;

        [Tooltip("Efecto de sonido opcional al pasar por un checkpoint")]
        [SerializeField] private AudioClip reachSound;

        [Tooltip("Duración de la animación de desvanecimiento suave (fade out)")]
        [SerializeField] private float fadeDuration = 0.5f;

        private List<GameObject> _checkpointObjects = new List<GameObject>();
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null && reachSound != null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            if (_audioSource != null)
            {
                _audioSource.playOnAwake = false;
                _audioSource.spatialBlend = 0f;
            }
        }

        public void Initialize(List<GameObject> checkpointObjects)
        {
            _checkpointObjects = checkpointObjects;
            
            // Ocultar todos los checkpoints inicialmente
            for (int i = 0; i < _checkpointObjects.Count; i++)
            {
                if (_checkpointObjects[i] != null)
                {
                    _checkpointObjects[i].SetActive(false);
                }
            }
        }

        public void ActivateCheckpointVisual(int index, Color color)
        {
            if (index < 0 || index >= _checkpointObjects.Count) return;

            GameObject cpObj = _checkpointObjects[index];
            if (cpObj != null)
            {
                cpObj.SetActive(true);
                
                // Configurar color y reiniciar opacidad al máximo
                DashedCircleRenderer renderer = cpObj.GetComponent<DashedCircleRenderer>();
                if (renderer != null)
                {
                    renderer.SetColor(color);
                    renderer.SetAlpha(1f);
                }
            }
        }

        public void DeactivateCheckpointVisual(int index, bool smooth)
        {
            if (index < 0 || index >= _checkpointObjects.Count) return;

            GameObject cpObj = _checkpointObjects[index];
            if (cpObj != null)
            {
                if (smooth)
                {
                    StartCoroutine(FadeOutAndDeactivate(cpObj));
                }
                else
                {
                    cpObj.SetActive(false);
                }
            }
        }

        private IEnumerator FadeOutAndDeactivate(GameObject checkpointObj)
        {
            DashedCircleRenderer renderer = checkpointObj.GetComponent<DashedCircleRenderer>();
            if (renderer != null)
            {
                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                    renderer.SetAlpha(alpha);
                    yield return null;
                }
            }
            checkpointObj.SetActive(false);
        }

        public void PlayFeedbackEffects(int index)
        {
            if (index < 0 || index >= _checkpointObjects.Count) return;

            GameObject cpObj = _checkpointObjects[index];
            if (cpObj != null)
            {
                // Instanciar partículas en la posición del checkpoint
                if (feedbackParticlesPrefab != null)
                {
                    Instantiate(feedbackParticlesPrefab, cpObj.transform.position, cpObj.transform.rotation);
                }

                // Reproducir sonido
                if (_audioSource != null && reachSound != null)
                {
                    _audioSource.PlayOneShot(reachSound);
                }
            }
        }

        public void CompleteSequenceVisual()
        {
            Debug.Log("[UnityCheckpointPresenter] Secuencia de checkpoints finalizada. Todos los checkpoints completados.");
        }
    }
}
