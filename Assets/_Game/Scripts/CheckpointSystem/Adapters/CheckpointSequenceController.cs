using System.Collections.Generic;
using AeroByte.CheckpointSystem.Domain.Entities;
using AeroByte.CheckpointSystem.Domain.Interfaces;
using AeroByte.CheckpointSystem.Framework;
using AeroByte.CheckpointSystem.UseCases;
using UnityEngine;

namespace AeroByte.CheckpointSystem.Adapters
{
    [RequireComponent(typeof(UnityCheckpointPresenter))]
    public class CheckpointSequenceController : MonoBehaviour
    {
        [Header("Configuración de Secuencia")]
        [Tooltip("Lista ordenada de GameObjects que representan los checkpoints")]
        [SerializeField] private List<GameObject> checkpointObjects = new List<GameObject>();

        [Header("Colores de Checkpoints")]
        [Tooltip("Color por defecto para los checkpoints")]
        [SerializeField] private Color defaultColor = Color.red;

        [Tooltip("Lista de colores específicos por checkpoint. El elemento 0 corresponde al Checkpoint 1, etc.")]
        [SerializeField] private List<Color> checkpointColors = new List<Color>();

        private CheckpointSequence _sequenceState;
        private ICheckpointPresenter _presenter;
        private ReachCheckpointUseCase _reachUseCase;

        private void Start()
        {
            if (checkpointObjects.Count == 0)
            {
                Debug.LogWarning("[CheckpointSequenceController] No se han asignado GameObjects de checkpoints en la lista.");
                return;
            }

            // Obtener el presentador concreto
            _presenter = GetComponent<UnityCheckpointPresenter>();
            if (_presenter is UnityCheckpointPresenter unityPresenter)
            {
                unityPresenter.Initialize(checkpointObjects);
            }

            // Inicializar las entidades lógicas
            _sequenceState = new CheckpointSequence(checkpointObjects.Count);
            
            // Inicializar el caso de uso
            _reachUseCase = new ReachCheckpointUseCase(
                _sequenceState, 
                _presenter, 
                checkpointColors.ToArray(), 
                defaultColor
            );

            // Configurar los triggers físicos e inicializarlos
            for (int i = 0; i < checkpointObjects.Count; i++)
            {
                GameObject cpObj = checkpointObjects[i];
                if (cpObj != null)
                {
                    // Añadir el trigger si no lo tiene
                    CheckpointTrigger trigger = cpObj.GetComponent<CheckpointTrigger>();
                    if (trigger == null)
                    {
                        trigger = cpObj.AddComponent<CheckpointTrigger>();
                    }
                    
                    trigger.Initialize(i, this);
                }
            }

            // Activar el primer checkpoint al inicio
            Color initialColor = checkpointColors.Count > 0 ? checkpointColors[0] : defaultColor;
            _presenter.ActivateCheckpointVisual(0, initialColor);
            Debug.Log("[CheckpointSequenceController] Inicializado. Primer checkpoint activado.");
        }

        // Método que es llamado desde los CheckpointTrigger cuando son tocados
        public void OnCheckpointTriggered(int index)
        {
            // Delegar al caso de uso la decisión lógica y visual
            _reachUseCase.Execute(index);
        }
    }
}
