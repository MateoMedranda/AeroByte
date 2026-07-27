using AeroByte.Menu.UI;
using UnityEngine;
using UnityEngine.UI;

namespace AeroByte.Menu.Profile
{
    public sealed class PilotAvatarOption : MonoBehaviour
    {
        [SerializeField, Range(0, 5)] private int avatarId;
        [SerializeField] private MenuRoundedGraphic background;
        [SerializeField] private PilotAvatarGraphic avatar;
        [SerializeField] private Text label;

        public int AvatarId => avatarId;
        public Button Button => GetComponent<Button>();

        public void Configure(int id, MenuRoundedGraphic cardBackground, PilotAvatarGraphic avatarGraphic, Text nameLabel)
        {
            avatarId = Mathf.Clamp(id, 0, 5);
            background = cardBackground;
            avatar = avatarGraphic;
            label = nameLabel;
        }

        public void SetSelected(bool selected)
        {
            if (background != null)
            {
                background.SetStyle(
                    selected ? new Color(0.04f, 0.35f, 0.54f, 1f) : new Color(0.018f, 0.11f, 0.17f, 0.96f),
                    selected ? new Color(0.02f, 0.20f, 0.34f, 1f) : new Color(0.01f, 0.06f, 0.10f, 0.98f),
                    16f,
                    selected ? new Color(0.20f, 0.78f, 1f, 0.95f) : new Color(0.20f, 0.64f, 0.86f, 0.18f),
                    selected ? 2f : 1f);
            }
            if (label != null) label.color = selected ? Color.white : new Color(0.64f, 0.76f, 0.83f, 1f);
            if (avatar != null) avatar.color = selected ? Color.white : new Color(0.78f, 0.85f, 0.90f, 1f);
        }
    }
}
