using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FlightSystem.Adapters;

namespace MissionSystem.Adapters
{
    [RequireComponent(typeof(Collider))]
    public class CheckpointRaceTrigger : MonoBehaviour
    {
        public int CheckpointIndex { get; private set; }
        private CheckpointRaceManager _manager;

        public void Initialize(int index, CheckpointRaceManager manager)
        {
            CheckpointIndex = index;
            _manager = manager;

            Collider col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            PlaneController plane = other.GetComponentInParent<PlaneController>();
            if (plane != null && _manager != null)
            {
                _manager.OnCheckpointTriggered(CheckpointIndex);
            }
        }
    }

    public class CheckpointRaceManager : MonoBehaviour
    {
        public static CheckpointRaceManager Instance { get; private set; }

        [Header("Configuración del Tiempo")]
        [Tooltip("Tiempo límite de la carrera en segundos (ej. 120 = 2 minutos).")]
        public float raceTimeLimit = 120f;
        [Tooltip("Si está marcado, el contador y la carrera comenzarán automáticamente al iniciar la escena.")]
        public bool autoStartOnStart = true;

        [Header("Checkpoints de la Carrera")]
        [Tooltip("Arrastra aquí en orden los Colliders o GameObjects de los checkpoints por donde debe pasar el avión.")]
        public List<Collider> checkpoints = new List<Collider>();

        [Header("Colores Visuales de Checkpoint (Opcional)")]
        public Color activeColor = Color.yellow;
        public Color inactiveColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        public Color completedColor = Color.green;

        [Header("Eventos de Carrera")]
        public UnityEvent OnRaceStarted;
        public UnityEvent OnCheckpointReached;
        public UnityEvent OnRaceWon;
        public UnityEvent OnRaceTimeout;

        public bool IsRaceActive { get; private set; }
        public bool IsRaceWon { get; private set; }
        public bool IsRaceFailed { get; private set; }
        public float RemainingTime { get; private set; }
        public int CurrentCheckpointIndex { get; private set; } = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Inicializar cada checkpoint con su script de trigger
            for (int i = 0; i < checkpoints.Count; i++)
            {
                if (checkpoints[i] != null)
                {
                    var trigger = checkpoints[i].GetComponent<CheckpointRaceTrigger>();
                    if (trigger == null)
                    {
                        trigger = checkpoints[i].gameObject.AddComponent<CheckpointRaceTrigger>();
                    }
                    trigger.Initialize(i, this);
                }
            }

            UpdateVisuals();

            if (autoStartOnStart)
            {
                StartRace();
            }
        }

        public void StartRace()
        {
            RemainingTime = raceTimeLimit;
            CurrentCheckpointIndex = 0;
            IsRaceActive = true;
            IsRaceWon = false;
            IsRaceFailed = false;
            UpdateVisuals();
            OnRaceStarted?.Invoke();
            Debug.Log($"[CheckpointRaceManager] ¡Carrera iniciada! Tiempo límite: {raceTimeLimit}s | Checkpoints: {checkpoints.Count}");
        }

        private void Update()
        {
            if (!IsRaceActive) return;

            RemainingTime -= Time.deltaTime;
            if (RemainingTime <= 0f)
            {
                RemainingTime = 0f;
                IsRaceActive = false;
                IsRaceFailed = true;
                OnRaceTimeout?.Invoke();
                Debug.LogWarning("[CheckpointRaceManager] ¡TIEMPO AGOTADO! Has perdido la carrera.");
            }
        }

        public void OnCheckpointTriggered(int index)
        {
            if (!IsRaceActive) return;

            // Debe pasar por los checkpoints en orden
            if (index == CurrentCheckpointIndex)
            {
                CurrentCheckpointIndex++;
                OnCheckpointReached?.Invoke();
                Debug.Log($"[CheckpointRaceManager] ¡Checkpoint {index + 1}/{checkpoints.Count} alcanzado!");

                if (CurrentCheckpointIndex >= checkpoints.Count)
                {
                    // ¡CARRERA GANADA!
                    IsRaceActive = false;
                    IsRaceWon = true;
                    OnRaceWon?.Invoke();
                    Debug.Log($"[CheckpointRaceManager] ¡CARRERA GANADA! Tiempo restante: {RemainingTime:F2}s");
                }

                UpdateVisuals();
            }
        }

        private void UpdateVisuals()
        {
            for (int i = 0; i < checkpoints.Count; i++)
            {
                if (checkpoints[i] == null) continue;

                Renderer r = checkpoints[i].GetComponent<Renderer>();
                if (r == null) r = checkpoints[i].GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    Color targetColor = inactiveColor;
                    if (i < CurrentCheckpointIndex)
                        targetColor = completedColor;
                    else if (i == CurrentCheckpointIndex && IsRaceActive)
                        targetColor = activeColor;

                    r.material.color = targetColor;
                    r.material.SetColor("_EmissionColor", targetColor * 1.5f);
                }
            }
        }

        public Transform GetCurrentActiveCheckpointTransform()
        {
            if (IsRaceActive && CurrentCheckpointIndex < checkpoints.Count && checkpoints[CurrentCheckpointIndex] != null)
            {
                return checkpoints[CurrentCheckpointIndex].transform;
            }
            return null;
        }
    }
}
