using System.Collections;
using UnityEngine;

namespace MissionSystem.Adapters
{
    [RequireComponent(typeof(Collider))]
    public class AttackZoneTrigger : MonoBehaviour
    {
        [Header("Estructura o Edificio Objetivo")]
        [Tooltip("Asigna aquí el GameObject del edificio o estructura que desaparecerá al explotar (Opcional).")]
        public GameObject targetVisual;

        [Header("Efectos de Explosión")]
        [Tooltip("Prefab de partículas de explosión que aparecerá al bombardear la zona.")]
        public GameObject explosionPrefab;
        [Tooltip("Archivo de audio (.wav / .mp3) que sonará al explotar la zona.")]
        public AudioClip explosionSound;
        [Range(0f, 1f)]
        public float explosionVolume = 1f;

        public bool IsDestroyed { get; private set; }
        public bool IsActive { get; private set; }

        private Collider _triggerCollider;
        private Renderer[] _renderers;

        private void Awake()
        {
            _triggerCollider = GetComponent<Collider>();
            if (_triggerCollider != null)
            {
                _triggerCollider.isTrigger = true;
            }
            _renderers = GetComponentsInChildren<Renderer>();
        }

        public void SetState(bool active, bool destroyed)
        {
            IsActive = active;
            IsDestroyed = destroyed;

            if (_triggerCollider != null)
            {
                _triggerCollider.enabled = active && !destroyed;
            }

            // Visual indicator of active zone (glow cyan/red)
            if (_renderers != null)
            {
                foreach (var r in _renderers)
                {
                    if (r != null)
                    {
                        r.enabled = !destroyed;
                        if (active && !destroyed)
                        {
                            r.material.color = new Color(1f, 0.2f, 0.1f, 0.6f);
                            r.material.SetColor("_EmissionColor", new Color(1f, 0.2f, 0.1f) * 2f);
                        }
                        else
                        {
                            r.material.color = new Color(0.3f, 0.3f, 0.3f, 0.3f);
                        }
                    }
                }
            }

            if (targetVisual != null && destroyed)
            {
                targetVisual.SetActive(false);
            }
        }

        public void ExplodeZone()
        {
            if (IsDestroyed) return;
            IsDestroyed = true;
            IsActive = false;

            Debug.Log($"[AttackZoneTrigger] ¡ZONA DE ATAQUE DESTRUIDA EN {name}!");

            // 1. Efecto Visual de Explosión
            if (explosionPrefab != null)
            {
                var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 8f);
            }

            // 2. Audio de Explosión (.wav)
            if (explosionSound != null)
            {
                GameObject audioObj = new GameObject($"AttackZoneAudio_{name}");
                audioObj.transform.position = transform.position;
                AudioSource src = audioObj.AddComponent<AudioSource>();
                src.clip = explosionSound;
                src.volume = explosionVolume;
                src.spatialBlend = 0.5f; // Semi-3D para que suene fuerte
                src.Play();
                Destroy(audioObj, explosionSound.length + 0.5f);
            }

            // 3. Ocultar o destruir la estructura objetivo
            SetState(false, true);
        }

        private void OnTriggerEnter(Collider other)
        {
            var planeCtrl = other.GetComponentInParent<PlaneDeliveryController>();
            if (planeCtrl != null && IsActive && !IsDestroyed)
            {
                planeCtrl.SetCurrentAttackZone(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var planeCtrl = other.GetComponentInParent<PlaneDeliveryController>();
            if (planeCtrl != null && planeCtrl.CurrentAttackZone == this)
            {
                planeCtrl.SetCurrentAttackZone(null);
            }
        }
    }
}
