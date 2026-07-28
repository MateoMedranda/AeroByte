using UnityEngine;

namespace AeroByte.Menu.Audio
{
    public sealed class MenuMusicController : MonoBehaviour
    {
        private const float MusicVolume = 0.52f;
        private const float DefaultFadeDuration = 1.8f;

        private static MenuMusicController _instance;

        private AudioSource _mainSource;
        private AudioSource _levelInfoSource;
        private float _mainTarget;
        private float _levelInfoTarget;
        private float _fadeSpeed;
        private bool _mainStarted;
        private bool _levelInfoStarted;

        private void Awake()
        {
            _instance = this;
            EnsureSources();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void Update()
        {
            float step = _fadeSpeed * Time.unscaledDeltaTime;
            UpdateSource(_mainSource, ref _mainStarted, _mainTarget, step);
            UpdateSource(_levelInfoSource, ref _levelInfoStarted, _levelInfoTarget, step);
        }

        public void Initialize(AudioClip mainMenuClip, AudioClip levelInfoClip)
        {
            EnsureSources();
            _mainSource.clip = mainMenuClip;
            _levelInfoSource.clip = levelInfoClip;
            if (mainMenuClip != null && mainMenuClip.loadState == AudioDataLoadState.Unloaded) mainMenuClip.LoadAudioData();
            if (levelInfoClip != null && levelInfoClip.loadState == AudioDataLoadState.Unloaded) levelInfoClip.LoadAudioData();
            SwitchToMain(2.4f);
        }

        public static void PlayMain(float fadeDuration = DefaultFadeDuration)
        {
            _instance?.SwitchToMain(fadeDuration);
        }

        public static void PlayLevelInfo(float fadeDuration = DefaultFadeDuration)
        {
            _instance?.SwitchToLevelInfo(fadeDuration);
        }

        public static void FadeToSilence(float fadeDuration = 1.6f)
        {
            if (_instance == null) return;
            _instance._mainTarget = 0f;
            _instance._levelInfoTarget = 0f;
            _instance.SetFadeSpeed(fadeDuration);
        }

        private void SwitchToMain(float fadeDuration)
        {
            EnsurePlaying(_mainSource, ref _mainStarted);
            _mainTarget = MusicVolume;
            _levelInfoTarget = 0f;
            SetFadeSpeed(fadeDuration);
        }

        private void SwitchToLevelInfo(float fadeDuration)
        {
            EnsurePlaying(_levelInfoSource, ref _levelInfoStarted);
            _mainTarget = 0f;
            _levelInfoTarget = MusicVolume;
            SetFadeSpeed(fadeDuration);
        }

        private void SetFadeSpeed(float duration)
        {
            _fadeSpeed = MusicVolume / Mathf.Max(0.05f, duration);
        }

        private static void EnsurePlaying(AudioSource source, ref bool started)
        {
            if (source == null || source.clip == null || source.isPlaying) return;
            if (started) source.UnPause();
            else
            {
                source.Play();
                started = true;
            }
        }

        private static void UpdateSource(AudioSource source, ref bool started, float target, float step)
        {
            if (source == null || source.clip == null) return;
            if (target > 0f) EnsurePlaying(source, ref started);
            source.volume = Mathf.MoveTowards(source.volume, target, step);
            if (target <= 0f && source.volume <= 0.001f && source.isPlaying) source.Pause();
        }

        private void EnsureSources()
        {
            _mainSource ??= FindOrCreateSource("Main Menu Music");
            _levelInfoSource ??= FindOrCreateSource("Level Information Music");
        }

        private AudioSource FindOrCreateSource(string sourceName)
        {
            var sourceTransform = transform.Find(sourceName);
            var source = sourceTransform == null ? null : sourceTransform.GetComponent<AudioSource>();
            if (source == null)
            {
                var sourceObject = new GameObject(sourceName);
                sourceObject.transform.SetParent(transform, false);
                source = sourceObject.AddComponent<AudioSource>();
            }
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;
            source.volume = 0f;
            return source;
        }
    }
}
