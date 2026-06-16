using AeroByte.CheckpointSystem.Domain.Entities;
using AeroByte.CheckpointSystem.Domain.Interfaces;
using UnityEngine;

namespace AeroByte.CheckpointSystem.UseCases
{
    public class ReachCheckpointUseCase
    {
        private readonly CheckpointSequence _sequence;
        private readonly ICheckpointPresenter _presenter;
        private readonly Color[] _checkpointColors;
        private readonly Color _defaultColor;

        public ReachCheckpointUseCase(CheckpointSequence sequence, ICheckpointPresenter presenter, Color[] checkpointColors, Color defaultColor)
        {
            _sequence = sequence;
            _presenter = presenter;
            _checkpointColors = checkpointColors;
            _defaultColor = defaultColor;
        }

        public void Execute(int checkpointIndex)
        {
            // Intentar avanzar la secuencia lógica
            if (_sequence.ReachCheckpoint(checkpointIndex))
            {
                Debug.Log($"[ReachCheckpointUseCase] Checkpoint {checkpointIndex} alcanzado con éxito.");

                // 1. Efectos visuales/sonoros de feedback para el checkpoint actual
                _presenter.PlayFeedbackEffects(checkpointIndex);

                // 2. Desvanecer de forma suave el checkpoint que se acaba de tocar
                _presenter.DeactivateCheckpointVisual(checkpointIndex, true);

                // 3. Activar el siguiente checkpoint
                if (!_sequence.IsCompleted)
                {
                    int nextIndex = _sequence.ActiveIndex;
                    Color color = GetColorForIndex(nextIndex);
                    _presenter.ActivateCheckpointVisual(nextIndex, color);
                    Debug.Log($"[ReachCheckpointUseCase] Siguiente checkpoint {nextIndex} activado.");
                }
                else
                {
                    _presenter.CompleteSequenceVisual();
                    Debug.Log("[ReachCheckpointUseCase] ¡Secuencia de checkpoints completada!");
                }
            }
            else
            {
                Debug.LogWarning($"[ReachCheckpointUseCase] Intento de tocar checkpoint {checkpointIndex} rechazado. Checkpoint activo actual: {_sequence.ActiveIndex}");
            }
        }

        private Color GetColorForIndex(int index)
        {
            if (_checkpointColors != null && index < _checkpointColors.Length)
            {
                return _checkpointColors[index];
            }
            return _defaultColor;
        }
    }
}
