using System;
using System.Collections.Generic;

namespace AeroByte.CheckpointSystem.Domain.Entities
{
    public class CheckpointSequence
    {
        public List<CheckpointState> Checkpoints { get; private set; }
        public int ActiveIndex { get; private set; }
        public bool IsCompleted { get; private set; }

        public event Action<int> OnActiveCheckpointChanged;
        public event Action OnCheckpointSequenceCompleted;

        public CheckpointSequence(int totalCheckpoints)
        {
            Checkpoints = new List<CheckpointState>();
            for (int i = 0; i < totalCheckpoints; i++)
            {
                Checkpoints.Add(new CheckpointState(i));
            }
            ActiveIndex = 0;
            IsCompleted = false;
        }

        public bool ReachCheckpoint(int index)
        {
            if (IsCompleted) return false;
            if (index != ActiveIndex) return false;

            Checkpoints[index].Reach();

            if (ActiveIndex < Checkpoints.Count - 1)
            {
                ActiveIndex++;
                OnActiveCheckpointChanged?.Invoke(ActiveIndex);
            }
            else
            {
                IsCompleted = true;
                OnCheckpointSequenceCompleted?.Invoke();
            }

            return true;
        }
    }
}
