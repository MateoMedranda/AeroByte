using FlightSystem.Adapters;
using UnityEngine;

namespace AeroByte.CheckpointSystem.Adapters
{
    [RequireComponent(typeof(Collider))]
    public class CheckpointTrigger : MonoBehaviour
    {
        public int Index { get; private set; }
        private CheckpointSequenceController _controller;
        private bool _isInitialized = false;

        public void Initialize(int index, CheckpointSequenceController controller)
        {
            Index = index;
            _controller = controller;
            _isInitialized = true;

            // Asegurar que el collider sea un Trigger
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isInitialized) return;

            // Buscar el PlaneController en el objeto que entra (o en su padre)
            PlaneController plane = other.GetComponent<PlaneController>();
            if (plane == null)
            {
                plane = other.GetComponentInParent<PlaneController>();
            }

            if (plane != null)
            {
                // El avión del jugador ha tocado el trigger, reportar al controlador
                _controller.OnCheckpointTriggered(Index);
            }
        }
    }
}
