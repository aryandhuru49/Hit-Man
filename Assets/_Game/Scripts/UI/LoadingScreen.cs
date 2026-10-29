using UnityEngine;
using UnityEngine.UI;

namespace ExitClause.UI
{
    /// <summary>Location card, gameplay tip and progress bar shown while a scene loads asynchronously.</summary>
    public class LoadingScreen : MonoBehaviour
    {
        GameObject m_root;
        Text m_title, m_tip;
        Image m_fill;

        static readonly string[] Tips =
        {
            "Crouch (C) to halve how fast guards notice you.",
            "Throw a coin (G) to pull a guard off his patrol.",
            "Hold right mouse to aim. The camera locks onto the nearest threat unless Free Aim is on.",
            "Press Q next to a wall to take cover. Aim to peek over low cover.",
            "Take down unaware guards from behind with F, then take their uniform.",
            "Press T for the high-angle survey camera.",
            "Your journal (J) holds every observation. Pin clues to suspects to make sense of them.",
            "Rage (V) makes you strong and fast, but every kill leaves evidence behind.",
            "Driving: Space is the handbrake, B looks behind.",
        };

        static readonly System.Collections.Generic.Dictionary<string, string> Names = new System.Collections.Generic.Dictionary<string, string>
        {
            { "MainMenu", "Exit Clause" },
            { "Prologue_Prague", "Prologue - Prague" },
            { "Act0_Riviera", "Act 0 - The Riviera" },
            { "Act1_Mansion", "Act 1 - The Ambush" },
            { "Act1_CoastRoad", "Act 1 - The Coast Road" },
            { "Interlude_Harbor", "Interlude - The Loved One" },
            { "Act2_Safehouse", "Act 2 - Safehouse, Genoa" },
            { "Act2_Alps", "Act 2 - The Sniper, Swiss Alps" },
            { "Act2_Marseille", "Act 2 - The Infiltrator, Marseille" },
            { "Act2_Berlin", "Act 2 - The Tech Specialist, Berlin" },
            { "Act3_Zurich", "Act 3 - The Agency, Zurich" },
            { "Ending", "Epilogue" },
        };

        public void Build(Transform canvas)
        {
            var bg = UIKit.FullScreen(canvas, "Loading", new Color(0.03f, 0.03f, 0.04f, 1f));
            m_root = bg.gameObject;
            m_title = UIKit.Label(bg.transform, "", 56, TextAnchor.MiddleLeft, UIKit.Accent, FontStyle.Bold);
            UIKit.Place(m_title.rectTransform, new Vector2(0.06f, 0.2f), new Vector2(0.94f, 0.32f));
            m_tip = UIKit.Label(bg.transform, "", 24, TextAnchor.MiddleLeft, UIKit.Dim);
            UIKit.Place(m_tip.rectTransform, new Vector2(0.06f, 0.13f), new Vector2(0.94f, 0.2f));
            var barBg = UIKit.Panel(bg.transform, "Bar", new Color(0.2f, 0.2f, 0.2f));
            UIKit.Place(barBg.rectTransform, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.09f));
            m_fill = UIKit.Panel(barBg.transform, "Fill", UIKit.Accent);
            UIKit.Place(m_fill.rectTransform, Vector2.zero, new Vector2(0, 1));
            m_root.SetActive(false);
        }

        public void Show(string sceneName)
        {
            m_root.SetActive(true);
            m_title.text = Names.TryGetValue(sceneName, out var n) ? n : sceneName;
            m_tip.text = Tips[Random.Range(0, Tips.Length)];
            SetProgress(0);
        }

        public void SetProgress(float p) => m_fill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(p), 1);
        public void Hide() => m_root.SetActive(false);
    }
}
