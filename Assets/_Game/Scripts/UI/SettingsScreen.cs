using ExitClause.CameraSystem;
using ExitClause.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace ExitClause.UI
{
    /// <summary>Graphics preset (Low/Medium/High for low-end PCs), audio, controls, aim mode and subtitles.</summary>
    public class SettingsScreen : UIScreen
    {
        RectTransform m_body;
        static readonly string[] Presets = { "Low", "Medium", "High" };

        protected override void BuildLayout(Transform root)
        {
            UIKit.FullScreen(root, "Dim", new Color(0, 0, 0, 0.5f));
            UIKit.Window(root, "Settings", new Vector2(820, 860), out m_body);
        }

        public void Open() => OpenScreen();

        protected override void Refresh()
        {
            UIKit.Clear(m_body);
            var d = SettingsManager.Instance.Data;
            var list = UIKit.VerticalLayout(m_body, "List", 8f, 0);
            UIKit.Place(list, Vector2.zero, Vector2.one);

            Button preset = null;
            preset = UIKit.Button(list, "Graphics: " + Presets[Mathf.Clamp(d.qualityLevel, 0, 2)], () =>
            {
                d.qualityLevel = (d.qualityLevel + 1) % Presets.Length;
                UIKit.SetButtonLabel(preset, "Graphics: " + Presets[d.qualityLevel]);
            });
            UIKit.Slider(list, "Render scale", 0.5f, 1f, d.renderScale, v => d.renderScale = v);
            UIKit.Slider(list, "Master volume", 0f, 1f, d.masterVolume, v => d.masterVolume = v);
            UIKit.Slider(list, "Mouse sensitivity", 0.2f, 3f, d.mouseSensitivity, v => d.mouseSensitivity = v);
            UIKit.Slider(list, "Field of view offset", -10f, 15f, d.fieldOfViewOffset, v => d.fieldOfViewOffset = v);
            UIKit.Toggle(list, "Invert Y", d.invertY, v => d.invertY = v);
            UIKit.Toggle(list, "Free aim (off = classic lock-on)", d.freeAim, v => d.freeAim = v);
            UIKit.Toggle(list, "Subtitles", d.subtitles, v => d.subtitles = v);
            UIKit.Toggle(list, "Fullscreen", d.fullscreen, v => d.fullscreen = v);
            UIKit.Toggle(list, "VSync", d.vsync, v => d.vsync = v);
            UIKit.Button(list, "Apply and close", () =>
            {
                SettingsManager.Instance.Save();
                GameCameraRig.Instance?.ApplySettings();
                Close();
            });
        }
    }
}
