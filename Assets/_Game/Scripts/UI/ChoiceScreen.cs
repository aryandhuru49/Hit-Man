using System;
using ExitClause.Story;
using UnityEngine;
using UnityEngine.UI;

namespace ExitClause.UI
{
    /// <summary>Branching decision prompt. Slows time while open; a timed choice auto-picks the default option.</summary>
    public class ChoiceScreen : UIScreen
    {
        RectTransform m_body;
        Text m_prompt;
        Image m_timerFill;
        ChoiceDefinition m_choice;
        Action<ChoiceOption> m_onPick;
        float m_timeLeft;

        public override bool ClosableByEscape => false;

        protected override void BuildLayout(Transform root)
        {
            UIKit.FullScreen(root, "Dim", new Color(0, 0, 0, 0.55f));
            UIKit.Window(root, "Decision", new Vector2(1100, 620), out var body);
            m_prompt = UIKit.Label(body, "", 30, TextAnchor.UpperLeft);
            UIKit.Place(m_prompt.rectTransform, new Vector2(0, 0.72f), Vector2.one);
            var tb = UIKit.Panel(body, "Timer", new Color(0.2f, 0.2f, 0.2f));
            UIKit.Place(tb.rectTransform, new Vector2(0, 0.68f), new Vector2(1, 0.7f));
            m_timerFill = UIKit.Panel(tb.transform, "Fill", new Color(0.9f, 0.3f, 0.2f));
            UIKit.Place(m_timerFill.rectTransform, Vector2.zero, Vector2.one);
            m_body = UIKit.VerticalLayout(body, "Options", 12f, 0);
            UIKit.Place(m_body, Vector2.zero, new Vector2(1, 0.64f));
        }

        public void Open(ChoiceDefinition choice, Action<ChoiceOption> onPick)
        {
            m_choice = choice;
            m_onPick = onPick;
            m_timeLeft = choice.timeLimit;
            OpenScreen();
            Time.timeScale = 0.15f;
        }

        protected override void Refresh()
        {
            if (m_choice == null) return;
            m_prompt.text = m_choice.prompt;
            m_timerFill.transform.parent.gameObject.SetActive(m_choice.timeLimit > 0);
            UIKit.Clear(m_body);
            foreach (var o in m_choice.options)
            {
                var option = o;
                bool ok = StoryChoices.IsAvailable(o);
                var b = UIKit.Button(m_body, o.label + (string.IsNullOrEmpty(o.consequenceHint) ? "" : "\n<size=18><color=#aaaaaa>" + o.consequenceHint + "</color></size>"), () => Pick(option), 84, 26);
                b.GetComponentInChildren<Text>().supportRichText = true;
                b.interactable = ok;
            }
        }

        void Update()
        {
            if (!IsOpen || m_choice == null || m_choice.timeLimit <= 0) return;
            m_timeLeft -= Time.unscaledDeltaTime;
            m_timerFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(m_timeLeft / m_choice.timeLimit), 1);
            if (m_timeLeft <= 0)
            {
                int i = Mathf.Clamp(m_choice.defaultOption, 0, m_choice.options.Count - 1);
                Pick(m_choice.options[i]);
            }
        }

        void Pick(ChoiceOption option)
        {
            if (!IsOpen) return;
            Time.timeScale = 1f;
            Close();
            var cb = m_onPick;
            m_onPick = null;
            cb?.Invoke(option);
        }
    }
}
