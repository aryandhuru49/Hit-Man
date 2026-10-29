using ExitClause.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace ExitClause.UI
{
    /// <summary>Death or mission failure: retry from the last autosave or quit to the menu.</summary>
    public class GameOverScreen : UIScreen
    {
        Text m_title, m_sub;
        public override bool ClosableByEscape => false;

        protected override void BuildLayout(Transform root)
        {
            UIKit.FullScreen(root, "Dim", new Color(0.15f, 0, 0, 0.6f));
            m_title = UIKit.Label(root, "", 96, TextAnchor.MiddleCenter, new Color(0.9f, 0.2f, 0.15f), FontStyle.Bold);
            UIKit.Place(m_title.rectTransform, new Vector2(0, 0.55f), new Vector2(1, 0.75f));
            m_sub = UIKit.Label(root, "", 28, TextAnchor.MiddleCenter);
            UIKit.Place(m_sub.rectTransform, new Vector2(0, 0.48f), new Vector2(1, 0.55f));
            var list = UIKit.VerticalLayout(root, "Buttons", 12f, 0);
            UIKit.PlaceFixed(list, new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(460, 140));
            UIKit.Button(list, "Retry from checkpoint", Retry);
            UIKit.Button(list, "Main menu", () => { Close(); SceneLoader.Instance.Load("MainMenu"); });
        }

        public void Show(string title, string subtitle)
        {
            if (IsOpen) return;
            m_title.text = title;
            m_sub.text = subtitle;
            OpenScreen();
        }

        void Retry()
        {
            Close();
            if (!SaveSystem.LoadAndContinue(SaveSystem.AutosaveSlot))
            {
                GameSession.Instance.State.player.health = GameSession.Instance.State.player.maxHealth;
                SceneLoader.Instance.ReloadCurrent();
            }
        }
    }
}
