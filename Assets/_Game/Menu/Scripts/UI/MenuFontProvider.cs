using System;
using UnityEngine;

namespace AeroByte.Menu.UI
{
    public static class MenuFontProvider
    {
        private static Font _displayFont;
        private static Font _bodyFont;

        public static Font DisplayFont => _displayFont != null ? _displayFont : _displayFont = CreateFont("Bahnschrift", "Arial");
        public static Font BodyFont => _bodyFont != null ? _bodyFont : _bodyFont = CreateFont("Segoe UI", "Arial");

        private static Font CreateFont(string preferredName, string fallbackName)
        {
            string selectedName = fallbackName;
            var installedFonts = Font.GetOSInstalledFontNames();
            if (Array.Exists(installedFonts, name => string.Equals(name, preferredName, StringComparison.OrdinalIgnoreCase)))
            {
                selectedName = preferredName;
            }

            return Font.CreateDynamicFontFromOSFont(selectedName, 32)
                ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
