using UnityEngine;
using MissionSystem.Domain.Entities;
using MissionSystem.Domain.Interfaces;

namespace MissionSystem.UseCases
{
    public class DropCargoUseCase
    {
        private readonly MissionState _state;
        private readonly IDeliveryPresenter _presenter;

        public DropCargoUseCase(MissionState state, IDeliveryPresenter presenter)
        {
            _state = state;
            _presenter = presenter;
            
            // Set initial state of indicator light to red (delivery not completed)
            if (_presenter != null)
            {
                _presenter.UpdateIndicatorSignal(false);
            }
            else
            {
                Debug.LogError("[DropCargoUseCase] Presenter is null! Indicator signal cannot be initialized.");
            }
        }

        public bool Execute(Vector3 planePosition, Quaternion planeRotation, Vector3 planeVelocity)
        {
            Debug.Log($"[DropCargoUseCase] Intentando soltar carga. IsInDeliveryZone: {_state.IsInDeliveryZone}, IsDeliveryCompleted: {_state.IsDeliveryCompleted}");

            if (!_state.IsInDeliveryZone)
            {
                Debug.LogWarning("[DropCargoUseCase] No se puede soltar la carga: El avión NO está en la zona de entrega (Trigger).");
                return false;
            }

            if (_state.IsDeliveryCompleted)
            {
                Debug.LogWarning("[DropCargoUseCase] No se puede soltar la carga: La entrega ya ha sido completada anteriormente.");
                return false;
            }

            if (_presenter == null)
            {
                Debug.LogError("[DropCargoUseCase] No se puede soltar la carga: El presentador (IDeliveryPresenter) es nulo.");
                return false;
            }

            // Perform visual spawn of the cargo
            _presenter.SpawnCargoBox(planePosition, planeRotation, planeVelocity);
            
            // Complete delivery status
            _state.CompleteDelivery();
            
            // Update indicator signal to green (delivery completed)
            _presenter.UpdateIndicatorSignal(true);
            
            Debug.Log("[DropCargoUseCase] Carga soltada y estado de entrega completado con éxito.");
            return true;
        }
    }
}
