using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.UI
{
    /// <summary>Pause menu: resume, save, load, settings, quit to menu or desktop.</summary>
    public class PauseMenu : MonoBehaviour
    {
        GameObject m_root;

        public void Build(Transform canvas)
        {
            var dim = UIKit.FullScreen(canvas, "PauseMenu", new Color(0, 0, 0, 0.6f));
            m_root = dim.gameObject;
            UIKit.Window(dim.transform, "Paused", new Vector2(520, 560), out var body);
            var list = UIKit.VerticalLayout(body, "Buttons", 12f, 0);
            UIKit.Place(list, Vector2.zero, Vector2.one);
            UIKit.Button(list, "Resume", () => PauseController.Instance.Resume());
            UIKit.Button(list, "Save Game", () => UIRoot.Instance.SaveLoad.Open(true));
            UIKit.Button(list, "Load Game", () => UIRoot.Instance.SaveLoad.Open(false));
            UIKit.Button(list, "Settings", () => UIRoot.Instance.Settings.Open());
            UIKit.Button(list, "Quit to Main Menu", () => { PauseController.Instance.Resume(); SceneLoader.Instance.Load("MainMenu"); });
            UIKit.Button(list, "Quit to Desktop", Quit);
            m_root.SetActive(false);
        }

        public void Show() { m_root.SetActive(true); m_root.transform.SetAsFirstSibling(); }
        public void Hide()
        {
            m_root.SetActive(false);
            if (UIRoot.Instance.SaveLoad.IsOpen) UIRoot.Instance.SaveLoad.Close();
            if (UIRoot.Instance.Settings.IsOpen) UIRoot.Instance.Settings.Close();
        }

        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
