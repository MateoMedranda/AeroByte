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

        [Header("Carrera Contra el Reloj (Time Trial)")]
        [Tooltip("Si está activo, habilitará el contador en el HUD y se perderá la carrera si el tiempo llega a 0.")]
        [SerializeField] private bool isTimeTrialRace = true;
        [Tooltip("Tiempo límite de la carrera en segundos (ej. 120 = 2 minutos).")]
        [SerializeField] private float raceTimeLimit = 120f;
        [Tooltip("Si está activo, inicia el contador automáticamente al empezar la escena.")]
        [SerializeField] private bool autoStartRace = true;

        [Header("Audio de Checkpoint")]
        [Tooltip("Sonido que se reproduce al cruzar correctamente el checkpoint activo.")]
        [SerializeField] private AudioClip checkpointReachedSound;
        [Range(0f, 1f)]
        [SerializeField] private float checkpointSoundVolume = 1f;

        public static CheckpointSequenceController ActiveInstance { get; private set; }
        public bool IsRaceActive { get; private set; }
        public bool IsRaceWon { get; private set; }
        public bool IsRaceFailed { get; private set; }
        public float RemainingTime { get; private set; }
        public int CurrentCheckpointIndex => _sequenceState != null ? _sequenceState.ActiveIndex : 0;
        public int TotalCheckpoints => checkpointObjects != null ? checkpointObjects.Count : 0;

        private CheckpointSequence _sequenceState;
        private ICheckpointPresenter _presenter;
        private ReachCheckpointUseCase _reachUseCase;
        private AudioSource _checkpointAudioSource;

        private void Awake()
        {
            ActiveInstance = this;
            _checkpointAudioSource = gameObject.AddComponent<AudioSource>();
            _checkpointAudioSource.playOnAwake = false;
            _checkpointAudioSource.spatialBlend = 0f;
        }

        private void OnDestroy()
        {
            if (ActiveInstance == this) ActiveInstance = null;
        }

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
            _sequenceState.OnCheckpointSequenceCompleted += OnSequenceCompleted;
            
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

            if (isTimeTrialRace && autoStartRace)
            {
                StartRace();
            }
            else
            {
                IsRaceActive = true;
            }
        }

        public void StartRace()
        {
            RemainingTime = raceTimeLimit;
            IsRaceActive = true;
            IsRaceWon = false;
            IsRaceFailed = false;
            Debug.Log($"[CheckpointSequenceController] ¡Carrera iniciada! Tiempo límite: {raceTimeLimit}s");
        }

        private void Update()
        {
            if (!isTimeTrialRace || !IsRaceActive) return;

            RemainingTime -= Time.deltaTime;
            if (RemainingTime <= 0f)
            {
                RemainingTime = 0f;
                IsRaceActive = false;
                IsRaceFailed = true;
                Debug.LogWarning("[CheckpointSequenceController] ¡TIEMPO AGOTADO! Has perdido la carrera.");
            }
        }

        private void OnSequenceCompleted()
        {
            IsRaceActive = false;
            IsRaceWon = true;
            Debug.Log("[CheckpointSequenceController] ¡TODOS LOS CHECKPOINTS COMPLETADOS! Carrera Ganada.");
        }

        // Método que es llamado desde los CheckpointTrigger cuando son tocados
        public void OnCheckpointTriggered(int index)
        {
            if (!IsRaceActive || _sequenceState == null || index != _sequenceState.ActiveIndex) return;

            _reachUseCase.Execute(index);
            if (checkpointReachedSound != null)
            {
                _checkpointAudioSource.PlayOneShot(checkpointReachedSound, checkpointSoundVolume);
            }
        }

        public Transform GetCurrentActiveCheckpointTransform()
        {
            if (_sequenceState == null || _sequenceState.IsCompleted) return null;
            int idx = _sequenceState.ActiveIndex;
            if (idx >= 0 && idx < checkpointObjects.Count && checkpointObjects[idx] != null)
            {
                return checkpointObjects[idx].transform;
            }
            return null;
        }
    }
}
