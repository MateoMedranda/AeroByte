namespace AeroByte.CheckpointSystem.Domain.Entities
{
    public class CheckpointState
    {
        public int Index { get; private set; }
        public bool IsReached { get; private set; }

        public CheckpointState(int index)
        {
            Index = index;
            IsReached = false;
        }

        public void Reach()
        {
            IsReached = true;
        }
    }
}
