using System.Text;
using UnityEngine;

namespace AeroByte.Menu.Profile
{
    public static class PilotProfileService
    {
        private const string PilotNameKey = "AeroByte.Profile.PilotName";
        private const string AvatarIdKey = "AeroByte.Profile.AvatarId";
        private const string DefaultPilotName = "AEROBYTE";
        private const int AvatarCount = 6;

        public static string PilotName => SanitizeName(PlayerPrefs.GetString(PilotNameKey, DefaultPilotName));
        public static int AvatarId => Mathf.Clamp(PlayerPrefs.GetInt(AvatarIdKey, 0), 0, AvatarCount - 1);

        public static void Save(string pilotName, int avatarId)
        {
            string sanitizedName = SanitizeName(pilotName);
            PlayerPrefs.SetString(PilotNameKey, string.IsNullOrWhiteSpace(sanitizedName) ? DefaultPilotName : sanitizedName);
            PlayerPrefs.SetInt(AvatarIdKey, Mathf.Clamp(avatarId, 0, AvatarCount - 1));
            PlayerPrefs.Save();
        }

        public static string SanitizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return DefaultPilotName;

            var builder = new StringBuilder(16);
            foreach (char character in value.Trim())
            {
                if (builder.Length >= 16) break;
                if (char.IsLetterOrDigit(character) || character == ' ' || character == '-' || character == '_')
                {
                    builder.Append(character);
                }
            }

            string result = builder.ToString().Trim();
            return string.IsNullOrWhiteSpace(result) ? DefaultPilotName : result;
        }

        public static char ValidateCharacter(string currentText, int characterIndex, char addedCharacter)
        {
            if (currentText.Length >= 16) return '\0';
            return char.IsLetterOrDigit(addedCharacter) || addedCharacter == ' ' || addedCharacter == '-' || addedCharacter == '_'
                ? addedCharacter
                : '\0';
        }
    }
}
