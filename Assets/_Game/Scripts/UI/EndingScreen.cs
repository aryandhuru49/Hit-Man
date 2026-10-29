using ExitClause.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace ExitClause.UI
{
    /// <summary>Ending title, epilogue (with branch variations) and rolling credits.</summary>
    public class EndingScreen : UIScreen
    {
        Text m_title, m_body;
        RectTransform m_scroll;
        public override bool ClosableByEscape => false;

        protected override void BuildLayout(Transform root)
        {
            UIKit.FullScreen(root, "BG", new Color(0.02f, 0.02f, 0.03f, 1f));
            m_title = UIKit.Label(root, "", 72, TextAnchor.MiddleCenter, UIKit.Accent, FontStyle.Bold);
            UIKit.Place(m_title.rectTransform, new Vector2(0, 0.82f), new Vector2(1, 0.95f));
            var mask = UIKit.Rect(root, "Mask");
            UIKit.Place(mask, new Vector2(0.15f, 0.15f), new Vector2(0.85f, 0.8f));
            mask.gameObject.AddComponent<RectMask2D>();
            m_body = UIKit.Label(mask, "", 28, TextAnchor.UpperCenter);
            m_scroll = m_body.rectTransform;
            m_scroll.anchorMin = new Vector2(0, 1);
            m_scroll.anchorMax = new Vector2(1, 1);
            m_scroll.pivot = new Vector2(0.5f, 1);
            m_body.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var b = UIKit.Button(root, "Return to main menu", () => { Close(); SceneLoader.Instance.Load("MainMenu"); });
            UIKit.PlaceFixed((RectTransform)b.transform, new Vector2(0.5f, 0.06f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(460, 56));
        }

        public void Show(string title, string epilogue)
        {
            m_title.text = title;
            m_body.text = epilogue + "\n\n\n\n<b>EXIT CLAUSE</b>\n\nA solo developer project\nBuilt with Unity HDRP, Cinemachine and Timeline\n\nGame security portfolio target build:\nevery system here is deliberately attackable.\n\nThank you for playing.";
            m_body.supportRichText = true;
            m_scroll.anchoredPosition = Vector2.zero;
            OpenScreen();
        }

        void Update()
        {
            if (!IsOpen) return;
            if (Time.unscaledTime > 0) m_scroll.anchoredPosition += new Vector2(0, Time.unscaledDeltaTime * 22f);
        }
    }
}
