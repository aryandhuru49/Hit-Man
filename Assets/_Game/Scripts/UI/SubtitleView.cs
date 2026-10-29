using ExitClause.Story;
using ExitClause.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace ExitClause.UI
{
    /// <summary>Bottom-of-screen subtitles with the speaker's name highlighted. Honors the Subtitles setting.</summary>
    public class SubtitleView : MonoBehaviour
    {
        Text m_text;
        Image m_bg;
        float m_hideAt = -1f;

        public void Build(Transform canvas)
        {
            m_bg = UIKit.Panel(canvas, "SubtitleBG", new Color(0, 0, 0, 0.55f));
            UIKit.PlaceFixed(m_bg.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 120), new Vector2(1300, 90));
            m_text = UIKit.Label(m_bg.transform, "", 28, TextAnchor.MiddleCenter);
            m_text.supportRichText = true;
            UIKit.Place(m_text.rectTransform, Vector2.zero, Vector2.one, new Vector2(20, 6), new Vector2(-20, -6));
            m_bg.gameObject.SetActive(false);
        }

        public void Show(DialogueLine line) { Show(line.speaker, line.text); m_hideAt = Time.time + line.duration; }

        public void Show(string speaker, string text)
        {
            bool enabledSetting = SettingsManager.Instance == null || SettingsManager.Instance.Data.subtitles;
            if (!enabledSetting || string.IsNullOrEmpty(text)) { Clear(); return; }
            m_bg.gameObject.SetActive(true);
            m_text.text = string.IsNullOrEmpty(speaker) ? text : $"<color=#f2b84d><b>{speaker}:</b></color> {text}";
            m_hideAt = -1f;
        }

        public void Clear()
        {
            if (m_bg != null) m_bg.gameObject.SetActive(false);
        }

        void Update()
        {
            if (m_hideAt > 0 && Time.time > m_hideAt) { m_hideAt = -1f; Clear(); }
        }
    }
}
