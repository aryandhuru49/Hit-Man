using System;
using UnityEngine;

namespace ExitClause.Systems
{
    /// <summary>
    /// Loads, applies and saves SettingsData. Quality presets map to the Low/Medium/High quality
    /// levels created by the Editor project setup (each has its own lighter or heavier HDRP asset).
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        const string PrefsKey = "ExitClause.Settings";
        public static SettingsManager Instance { get; private set; }
        public SettingsData Data { get; private set; }
        public event Action OnApplied;

        void Awake()
        {
            Instance = this;
            Data = JsonUtility.FromJson<SettingsData>(PlayerPrefs.GetString(PrefsKey, "{}")) ?? new SettingsData();
            Apply();
        }

        public void Apply()
        {
            int levels = QualitySettings.names.Length;
            if (levels > 0) QualitySettings.SetQualityLevel(Mathf.Clamp(Data.qualityLevel, 0, levels - 1), true);
            AudioListener.volume = Data.masterVolume;
            QualitySettings.vSyncCount = Data.vsync ? 1 : 0;
            if (!Application.isEditor) Screen.fullScreen = Data.fullscreen;
            ScalableBufferManager.ResizeBuffers(Data.renderScale, Data.renderScale);
            OnApplied?.Invoke();
        }

        public void Save()
        {
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
            Apply();
        }
    }
}
