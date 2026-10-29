using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Systems
{
    /// <summary>Escape toggles the pause menu (or closes the top-most open screen).</summary>
    public class PauseController : MonoBehaviour
    {
        public static PauseController Instance { get; private set; }
        public bool IsPaused { get; private set; }

        /// <summary>Disabled in the main menu and during loading.</summary>
        public bool Allowed = true;

        void Awake() { Instance = this; }

        void Update()
        {
            if (GameInput.Pause == null || !GameInput.Pause.WasPressedThisFrame()) return;
            var ui = UIRoot.Instance;
            if (ui == null || SceneLoader.Instance.IsLoading) return;
            if (IsPaused) { Resume(); return; }
            if (ui.CloseTopScreen()) return;
            if (Allowed) Pause();
        }

        public void Pause()
        {
            if (IsPaused) return;
            IsPaused = true;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            GameInput.PushBlock();
            UIRoot.Instance.PauseMenu.Show();
        }

        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
            GameInput.PopBlock();
            UIRoot.Instance.PauseMenu.Hide();
        }
    }
}
