using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MissionSystem.Adapters
{
    public class AeroByteAttackManager : MonoBehaviour
    {
        public static AeroByteAttackManager Instance { get; private set; }

        [Header("Zonas de Ataque (Bombardeo)")]
        [Tooltip("Lista en orden de los objetos AttackZoneTrigger en la escena que deben ser destruidos.")]
        public List<AttackZoneTrigger> attackZones = new List<AttackZoneTrigger>();

        [Header("Eventos de Misión")]
        public UnityEvent<int, int> OnZoneDestroyed; // current, total
        public UnityEvent OnMissionComplete;

        public int CurrentZoneIndex { get; private set; } = 0;
        public int TotalZones => attackZones.Count;
        public int DestroyedZones => CurrentZoneIndex;
        public bool IsMissionComplete => CurrentZoneIndex >= TotalZones;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            for (int i = 0; i < attackZones.Count; i++)
            {
                if (attackZones[i] != null)
                {
                    attackZones[i].SetState(i == 0, false);
                }
            }
            Debug.Log($"[AeroByteAttackManager] Misión de Ataque inicializada. Total de zonas: {TotalZones}");
        }

        public AttackZoneTrigger GetCurrentActiveZone()
        {
            if (CurrentZoneIndex < attackZones.Count)
            {
                return attackZones[CurrentZoneIndex];
            }
            return null;
        }

        public Transform GetCurrentActiveZoneTransform()
        {
            var zone = GetCurrentActiveZone();
            return zone != null ? zone.transform : null;
        }

        public void RegisterAttackComplete(AttackZoneTrigger zone)
        {
            if (zone == null || zone != GetCurrentActiveZone()) return;

            Debug.Log($"[AeroByteAttackManager] ¡Zona {CurrentZoneIndex + 1}/{TotalZones} destruida con éxito!");
            zone.ExplodeZone();

            CurrentZoneIndex++;
            OnZoneDestroyed?.Invoke(CurrentZoneIndex, TotalZones);

            if (CurrentZoneIndex < attackZones.Count)
            {
                if (attackZones[CurrentZoneIndex] != null)
                {
                    attackZones[CurrentZoneIndex].SetState(true, false);
                }
            }
            else
            {
                Debug.Log("[AeroByteAttackManager] ¡TODAS LAS ZONAS DE ATAQUE HAN SIDO DESTRUIDAS! Misión Completada.");
                OnMissionComplete?.Invoke();
            }
        }
    }
}
