using UnityEngine;
using FlightSystem.Adapters;

namespace MissionSystem.Adapters
{
    [RequireComponent(typeof(Collider))]
    public class OutOfBoundsZone : MonoBehaviour
    {
        private void Start()
        {
            Collider col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
                Debug.LogWarning($"[OutOfBoundsZone] El collider en {gameObject.name} no era Trigger. Se configuró automáticamente como Trigger.", this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var plane = other.GetComponentInParent<PlaneController>();
            if (plane != null)
            {
                // El jugador salió del cubo gigante (área jugable) -> Entra en estado Out of Bounds
                if (OutOfBoundsManager.Instance != null)
                {
                    OutOfBoundsManager.Instance.EnterOOB();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var plane = other.GetComponentInParent<PlaneController>();
            if (plane != null)
            {
                // El jugador volvió a entrar al cubo gigante (área jugable) -> Sale del estado Out of Bounds
                if (OutOfBoundsManager.Instance != null)
                {
                    OutOfBoundsManager.Instance.ExitOOB();
                }
            }
        }
    }
}
