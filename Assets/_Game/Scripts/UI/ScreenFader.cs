using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ExitClause.UI
{
    /// <summary>Full-screen black fade used for sleeping, transitions and deaths.</summary>
    public class ScreenFader : MonoBehaviour
    {
        Image m_image;

        public void Build(Transform canvas)
        {
            m_image = UIKit.FullScreen(canvas, "Fader", new Color(0, 0, 0, 0));
            m_image.raycastTarget = false;
        }

        public IEnumerator FadeTo(float alpha, float seconds)
        {
            float start = m_image.color.a;
            for (float t = 0; t < 1f; t += Time.unscaledDeltaTime / Mathf.Max(0.01f, seconds))
            {
                m_image.color = new Color(0, 0, 0, Mathf.Lerp(start, alpha, t));
                yield return null;
            }
            m_image.color = new Color(0, 0, 0, alpha);
        }

        public void Set(float alpha) => m_image.color = new Color(0, 0, 0, alpha);
    }
}
