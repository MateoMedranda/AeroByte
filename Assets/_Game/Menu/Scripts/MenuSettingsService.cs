using System;
using UnityEngine;

namespace AeroByte.Menu
{
    public static class MenuSettingsService
    {
        private const string EffectsVolumeKey = "AeroByte.Menu.MasterVolume";
        private const string MusicVolumeKey = "AeroByte.Menu.MusicVolume";
        private const string MutedKey = "AeroByte.Menu.Muted";
        private const float DefaultVolume = 1f;

        public static float EffectsVolume { get; private set; } = DefaultVolume;
        public static float MusicVolume { get; private set; } = DefaultVolume;
        public static bool IsMuted { get; private set; }
        public static event Action VolumesChanged;

        public static void Load()
        {
            EffectsVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(EffectsVolumeKey, DefaultVolume));
            MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, DefaultVolume));
            IsMuted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
            Apply();
        }

        public static void SetEffectsVolume(float value)
        {
            EffectsVolume = Mathf.Clamp01(value);
            IsMuted = false;
            SaveAndApply();
        }

        public static void SetMusicVolume(float value)
        {
            MusicVolume = Mathf.Clamp01(value);
            IsMuted = false;
            SaveAndApply();
        }

        public static void SetMuted(bool muted)
        {
            IsMuted = muted;
            SaveAndApply();
        }

        public static void RestoreDefaults()
        {
            EffectsVolume = DefaultVolume;
            MusicVolume = DefaultVolume;
            IsMuted = false;
            SaveAndApply();
        }

        private static void SaveAndApply()
        {
            PlayerPrefs.SetFloat(EffectsVolumeKey, EffectsVolume);
            PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
            PlayerPrefs.SetInt(MutedKey, IsMuted ? 1 : 0);
            PlayerPrefs.Save();
            Apply();
        }

        private static void Apply()
        {
            // Effects use the listener; music sources opt out and apply MusicVolume directly.
            AudioListener.volume = IsMuted ? 0f : EffectsVolume;
            VolumesChanged?.Invoke();
        }
    }
}
