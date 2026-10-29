using ExitClause.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace ExitClause.UI
{
    /// <summary>Main menu scene: new game, continue, load, settings, quit.</summary>
    public class MainMenuController : MonoBehaviour
    {
        public string firstScene = "Prologue_Prague";
        Canvas m_canvas;

        void Start()
        {
            GameInput.ForceCursorVisible = true;
            GameInput.ResetBlocks();
            if (PauseController.Instance != null) PauseController.Instance.Allowed = false;
            UIRoot.Instance.HUD.SetVisible(false);
            Time.timeScale = 1f;

            m_canvas = UIKit.CreateCanvas("MainMenu", 5, transform);
            UIKit.FullScreen(m_canvas.transform, "Vignette", new Color(0, 0, 0, 0.45f));
            var title = UIKit.Label(m_canvas.transform, "EXIT CLAUSE", 120, TextAnchor.MiddleLeft, UIKit.Accent, FontStyle.Bold);
            UIKit.Place(title.rectTransform, new Vector2(0.07f, 0.68f), new Vector2(0.9f, 0.85f));
            var tag = UIKit.Label(m_canvas.transform, "The agency never lets anyone retire.", 30, TextAnchor.MiddleLeft, UIKit.TextColor, FontStyle.Italic);
            UIKit.Place(tag.rectTransform, new Vector2(0.07f, 0.62f), new Vector2(0.9f, 0.68f));

            var list = UIKit.VerticalLayout(m_canvas.transform, "Buttons", 12f, 0);
            UIKit.Place(list, new Vector2(0.07f, 0.18f), new Vector2(0.3f, 0.56f));
            int recent = SaveSystem.MostRecentSlot();
            var cont = UIKit.Button(list, "Continue", () => SaveSystem.LoadAndContinue(recent));
            cont.interactable = recent >= 0;
            UIKit.Button(list, "New Game", NewGame);
            UIKit.Button(list, "Load Game", () => UIRoot.Instance.SaveLoad.Open(false));
            UIKit.Button(list, "Settings", () => UIRoot.Instance.Settings.Open());
            UIKit.Button(list, "Quit", () => Application.Quit());

            var hint = UIKit.Label(m_canvas.transform, "Saves: " + SaveSystem.SaveFolder, 16, TextAnchor.LowerLeft, UIKit.Dim);
            UIKit.Place(hint.rectTransform, new Vector2(0.02f, 0.01f), new Vector2(0.98f, 0.05f));
        }

        void NewGame()
        {
            GameSession.Instance.NewGame();
            GameInput.ForceCursorVisible = false;
            SceneLoader.Instance.Load(firstScene);
        }

        void OnDestroy()
        {
            GameInput.ForceCursorVisible = false;
        }
    }
}
