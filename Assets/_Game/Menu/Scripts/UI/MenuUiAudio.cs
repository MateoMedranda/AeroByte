using UnityEngine;

namespace AeroByte.Menu.UI
{
    public sealed class MenuUiAudio : MonoBehaviour
    {
        private static MenuUiAudio _instance;
        private AudioSource _source;
        private AudioClip _hoverClip;
        private AudioClip _clickClip;
        private float _lastHoverTime;

        private void Awake()
        {
            _instance = this;
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = false;
            _source.spatialBlend = 0f;
            _source.ignoreListenerPause = true;
            _source.volume = 0.22f;
            _hoverClip = CreateTone("Menu Hover", 760f, 920f, 0.045f, 0.16f);
            _clickClip = CreateTone("Menu Click", 430f, 620f, 0.075f, 0.24f);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
            if (_hoverClip != null) Destroy(_hoverClip);
            if (_clickClip != null) Destroy(_clickClip);
        }

        public static void PlayHover()
        {
            if (_instance == null || Time.unscaledTime - _instance._lastHoverTime < 0.045f) return;
            _instance._lastHoverTime = Time.unscaledTime;
            _instance._source.PlayOneShot(_instance._hoverClip);
        }

        public static void PlayClick()
        {
            if (_instance != null) _instance._source.PlayOneShot(_instance._clickClip);
        }

        private static AudioClip CreateTone(string clipName, float startFrequency, float endFrequency, float duration, float amplitude)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];
            float phase = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                float progress = i / (float)(sampleCount - 1);
                float frequency = Mathf.Lerp(startFrequency, endFrequency, progress);
                phase += frequency / sampleRate * Mathf.PI * 2f;
                float envelope = Mathf.Pow(1f - progress, 2.2f) * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress * 10f));
                samples[i] = Mathf.Sin(phase) * envelope * amplitude;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
