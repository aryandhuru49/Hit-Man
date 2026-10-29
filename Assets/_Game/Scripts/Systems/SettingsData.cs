using System;

namespace ExitClause.Systems
{
    /// <summary>Player preferences, persisted to PlayerPrefs as JSON.</summary>
    [Serializable]
    public class SettingsData
    {
        public int qualityLevel = 1;          // 0 Low, 1 Medium, 2 High
        public float masterVolume = 0.8f;
        public float mouseSensitivity = 1f;
        public bool invertY;
        public bool freeAim;                  // false = classic lock-on auto aim
        public bool subtitles = true;
        public float fieldOfViewOffset;
        public bool fullscreen = true;
        public bool vsync = true;
        public float renderScale = 1f;
    }
}
