using UnityEngine;
using MissionSystem.Domain.Entities;
using MissionSystem.Domain.Interfaces;
using MissionSystem.UseCases;
using MissionSystem.Framework;

namespace MissionSystem.Adapters
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlaneDeliveryController : MonoBehaviour
    {
        [Header("Presentador de Entrega (Opcional)")]
        [Tooltip("Si se deja vacío, buscará automáticamente un componente UnityDeliveryPresenter en la escena.")]
        [SerializeField] private GameObject deliveryPresenterObject;

        private MissionState _state;
        private DropCargoUseCase _useCase;
        private IDeliveryPresenter _presenter;
        private Rigidbody _rb;

        public DeliveryZoneTrigger CurrentZone { get; private set; }
        public AttackZoneTrigger CurrentAttackZone { get; private set; }

        public void SetCurrentZone(DeliveryZoneTrigger zone)
        {
            CurrentZone = zone;
        }

        public void SetCurrentAttackZone(AttackZoneTrigger zone)
        {
            CurrentAttackZone = zone;
            if (zone != null)
            {
                _state.SetInDeliveryZone(true); // Permite que el avión habilite soltar carga/bomba en zona de ataque
            }
            else if (CurrentZone == null)
            {
                _state.SetInDeliveryZone(false);
            }
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();

            // Try to resolve presenter reference
            if (deliveryPresenterObject != null)
            {
                _presenter = deliveryPresenterObject.GetComponent<IDeliveryPresenter>();
            }
            
            if (_presenter == null)
            {
                _presenter = GetComponentInChildren<IDeliveryPresenter>();
            }

            if (_presenter == null)
            {
                // Resilient fallback: search the whole scene for the presenter
                _presenter = Object.FindFirstObjectByType<UnityDeliveryPresenter>();
            }

            if (_presenter == null)
            {
                Debug.LogError("[PlaneDeliveryController] ERROR: No se encontró ningún componente que implemente IDeliveryPresenter en el avión, en sus hijos o en la escena. Asegúrate de añadir el script UnityDeliveryPresenter a la escena.", this);
            }
            else
            {
                Debug.Log("[PlaneDeliveryController] Presentador de entrega (IDeliveryPresenter) resuelto y conectado correctamente.", this);
            }

            // Initialize Clean Architecture Core
            _state = new MissionState();
            _useCase = new DropCargoUseCase(_state, _presenter);
        }

        public void SetInDeliveryZone(bool inZone)
        {
            _state.SetInDeliveryZone(inZone);
            Debug.Log($"[PlaneDeliveryController] Estado de zona actualizado. ¿En zona de entrega?: {inZone}");
        }

        public void TryDropCargo()
        {
            Debug.Log("[PlaneDeliveryController] Solicitud de soltar carga recibida. Procesando...");
            
            Vector3 velocity = _rb != null ? _rb.linearVelocity : Vector3.zero;
            
            bool success = _useCase.Execute(transform.position, transform.rotation, velocity);
            if (success)
            {
                Debug.Log("[PlaneDeliveryController] La carga ha sido soltada exitosamente.");
                
                if (AeroByteDeliveryManager.Instance != null && CurrentZone != null)
                {
                    AeroByteDeliveryManager.Instance.RegisterDeliveryComplete(CurrentZone);
                }

                if (AeroByteAttackManager.Instance != null && CurrentAttackZone != null)
                {
                    AeroByteAttackManager.Instance.RegisterAttackComplete(CurrentAttackZone);
                }

                // Reset delivery state to allow dropping in the next zone
                _state.ResetDelivery();
            }
            else
            {
                Debug.LogWarning("[PlaneDeliveryController] No se pudo soltar la carga. Revisa los logs del caso de uso (DropCargoUseCase) para ver el motivo.");
            }
        }
    }
}
