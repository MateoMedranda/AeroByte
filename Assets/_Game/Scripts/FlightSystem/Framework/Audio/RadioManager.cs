using UnityEngine;

namespace AeroByte.FlightSystem.Framework.Audio
{
    public class RadioManager : MonoBehaviour
    {
        [Header("Radio Settings")]
        [SerializeField] private AudioClip[] musicTracks;
        [SerializeField] private float defaultVolume = 0.5f;

        private AudioSource _radioSource;
        private int _currentTrackIndex = 0;
        private bool _isRadioOn = false;

        private void Awake()
        {
            SetupRadio();
        }

        private void SetupRadio()
        {
            _radioSource = gameObject.AddComponent<AudioSource>();
            
            // 2D Audio settings for background music
            _radioSource.spatialBlend = 0.0f; // Fully 2D
            _radioSource.loop = true;
            _radioSource.playOnAwake = false;
            _radioSource.volume = defaultVolume;

            if (musicTracks != null && musicTracks.Length > 0)
            {
                _radioSource.clip = musicTracks[0];
            }
        }

        public void ToggleMusic()
        {
            _isRadioOn = !_isRadioOn;

            if (_isRadioOn)
            {
                if (_radioSource.clip != null)
                {
                    _radioSource.Play();
                    Debug.Log($"[RadioManager] Radio ON: Playing {_radioSource.clip.name}");
                }
                else
                {
                    Debug.LogWarning("[RadioManager] Radio ON, but no music tracks assigned!");
                }
            }
            else
            {
                _radioSource.Pause();
                Debug.Log("[RadioManager] Radio OFF");
            }
        }

        public void NextTrack()
        {
            if (musicTracks == null || musicTracks.Length == 0) return;

            _currentTrackIndex = (_currentTrackIndex + 1) % musicTracks.Length;
            PlayCurrentTrack();
        }

        public void PreviousTrack()
        {
            if (musicTracks == null || musicTracks.Length == 0) return;

            _currentTrackIndex--;
            if (_currentTrackIndex < 0)
            {
                _currentTrackIndex = musicTracks.Length - 1;
            }
            PlayCurrentTrack();
        }

        private void PlayCurrentTrack()
        {
            _radioSource.clip = musicTracks[_currentTrackIndex];
            
            if (_isRadioOn)
            {
                _radioSource.Play();
                Debug.Log($"[RadioManager] Playing track: {_radioSource.clip.name}");
            }
        }
    }
}
