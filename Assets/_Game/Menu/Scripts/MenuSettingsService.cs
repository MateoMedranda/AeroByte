using UnityEngine;

namespace AeroByte.Menu
{
    public static class MenuSettingsService
    {
        private const string MasterVolumeKey = "AeroByte.Menu.MasterVolume";
        private const string MutedKey = "AeroByte.Menu.Muted";
        private const float DefaultVolume = 1f;

        public static float MasterVolume { get; private set; } = DefaultVolume;
        public static bool IsMuted { get; private set; }

        public static void Load()
        {
            MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, DefaultVolume));
            IsMuted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
            Apply();
        }

        public static void SetMasterVolume(float value)
        {
            MasterVolume = Mathf.Clamp01(value);
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
            MasterVolume = DefaultVolume;
            IsMuted = false;
            SaveAndApply();
        }

        private static void SaveAndApply()
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
            PlayerPrefs.SetInt(MutedKey, IsMuted ? 1 : 0);
            PlayerPrefs.Save();
            Apply();
        }

        private static void Apply()
        {
            AudioListener.volume = IsMuted ? 0f : MasterVolume;
        }
    }
}
