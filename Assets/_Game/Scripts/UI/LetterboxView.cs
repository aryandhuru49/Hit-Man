using UnityEngine;
using UnityEngine.UI;

namespace ExitClause.UI
{
    /// <summary>Cinematic black bars and a skip hint shown during cutscenes.</summary>
    public class LetterboxView : MonoBehaviour
    {
        RectTransform m_top, m_bottom;
        Text m_skip;
        float m_target, m_current;

        public void Build(Transform canvas)
        {
            m_top = UIKit.Panel(canvas, "BarTop", Color.black).rectTransform;
            m_bottom = UIKit.Panel(canvas, "BarBottom", Color.black).rectTransform;
            m_skip = UIKit.Label(canvas, "[Enter] Skip", 18, TextAnchor.LowerRight, UIKit.Dim);
            UIKit.PlaceFixed(m_skip.rectTransform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-30, 20), new Vector2(300, 30));
            Apply();
            m_skip.enabled = false;
        }

        public void Show(bool skippable) { m_target = 1f; m_skip.enabled = skippable; }
        public void Hide() { m_target = 0f; if (m_skip != null) m_skip.enabled = false; }

        void Update()
        {
            m_current = Mathf.MoveTowards(m_current, m_target, Time.unscaledDeltaTime * 2.5f);
            Apply();
        }

        void Apply()
        {
            float h = 0.11f * m_current;
            UIKit.Place(m_top, new Vector2(0, 1 - h), Vector2.one);
            UIKit.Place(m_bottom, Vector2.zero, new Vector2(1, h));
        }
    }
}
