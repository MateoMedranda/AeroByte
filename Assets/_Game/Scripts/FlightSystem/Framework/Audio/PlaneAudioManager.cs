using UnityEngine;
using FlightSystem.Adapters;
using FlightSystem.Domain.Entities;
using AeroByte.FlightSystem.Framework.Config;

namespace AeroByte.FlightSystem.Framework.Audio
{
    [RequireComponent(typeof(PlaneController))]
    public class PlaneAudioManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float minPitch = 0.8f;
        [SerializeField] private float maxPitch = 2.0f;
        [SerializeField] private float minVolume = 0.2f;
        [SerializeField] private float maxVolume = 1.0f;
        [SerializeField] private float pitchSpeed = 2f;

        private PlaneController _planeController;
        private AudioSource _engineAudioSource;
        private float _currentPitch;

        private void Awake()
        {
            _planeController = GetComponent<PlaneController>();
            SetupEngineAudio();
        }

        private void SetupEngineAudio()
        {
            // We read the clip from the StatsConfig
            if (_planeController.statsConfig == null || _planeController.statsConfig.EngineSoundClip == null)
            {
                Debug.LogWarning("[PlaneAudioManager] No EngineSoundClip assigned in PlaneStatsConfig.");
                return;
            }

            _engineAudioSource = gameObject.AddComponent<AudioSource>();
            _engineAudioSource.clip = _planeController.statsConfig.EngineSoundClip;
            
            // 3D Audio settings
            _engineAudioSource.spatialBlend = 1.0f; // Fully 3D
            _engineAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            _engineAudioSource.minDistance = 10f;
            _engineAudioSource.maxDistance = 500f;
            
            _engineAudioSource.loop = true;
            _engineAudioSource.playOnAwake = true;
            _engineAudioSource.volume = minVolume;
            _engineAudioSource.pitch = minPitch;
            _currentPitch = minPitch;
            
            _engineAudioSource.Play();
        }

        private void Update()
        {
            if (_engineAudioSource == null || _planeController == null) return;

            PlaneState state = _planeController.State;
            if (state == null) return;

            // Calculate target pitch and volume based on throttle
            float targetPitch = Mathf.Lerp(minPitch, maxPitch, Mathf.Abs(state.throttle));
            float targetVolume = Mathf.Lerp(minVolume, maxVolume, Mathf.Abs(state.throttle));

            // Smooth transition
            _currentPitch = Mathf.Lerp(_currentPitch, targetPitch, Time.deltaTime * pitchSpeed);
            
            _engineAudioSource.pitch = _currentPitch;
            _engineAudioSource.volume = Mathf.Lerp(_engineAudioSource.volume, targetVolume, Time.deltaTime * pitchSpeed);
        }
    }
}
