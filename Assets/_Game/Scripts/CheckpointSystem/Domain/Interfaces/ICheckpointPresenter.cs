using UnityEngine;

namespace AeroByte.CheckpointSystem.Domain.Interfaces
{
    public interface ICheckpointPresenter
    {
        void ActivateCheckpointVisual(int index, Color color);
        void DeactivateCheckpointVisual(int index, bool smooth);
        void PlayFeedbackEffects(int index);
        void CompleteSequenceVisual();
    }
}
