using System.Collections.Generic;
using ExitClause.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace ExitClause.UI
{
    /// <summary>
    /// Builds and owns all persistent UI: HUD, subtitles, letterbox, fader, loading screen, pause menu
    /// and every modal screen. Lives on the GameRoot so it survives scene loads.
    /// </summary>
    public class UIRoot : MonoBehaviour
    {
        public static UIRoot Instance { get; private set; }

        public HUDView HUD { get; private set; }
        public SubtitleView Subtitles { get; private set; }
        public LetterboxView Letterbox { get; private set; }
        public ScreenFader Fader { get; private set; }
        public LoadingScreen Loading { get; private set; }
        public PauseMenu PauseMenu { get; private set; }
        public SettingsScreen Settings { get; private set; }
        public SaveLoadScreen SaveLoad { get; private set; }
        public JournalScreen Journal { get; private set; }
        public ChoiceScreen Choice { get; private set; }
        public ShopScreen Shop { get; private set; }
        public BankScreen Bank { get; private set; }
        public HackingScreen Hacking { get; private set; }
        public LockpickScreen Lockpick { get; private set; }
        public TravelScreen Travel { get; private set; }
        public GameOverScreen GameOver { get; private set; }
        public EndingScreen Ending { get; private set; }

        readonly List<UIScreen> m_stack = new List<UIScreen>();

        void Awake()
        {
            Instance = this;
            var es = new GameObject("EventSystem");
            es.transform.SetParent(transform, false);
            es.AddComponent<EventSystem>();
            var module = es.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();

            var hudCanvas = UIKit.CreateCanvas("HUDCanvas", 0, transform);
            HUD = hudCanvas.gameObject.AddComponent<HUDView>();
            HUD.Build(hudCanvas.transform);

            var overlay = UIKit.CreateCanvas("OverlayCanvas", 10, transform);
            Letterbox = overlay.gameObject.AddComponent<LetterboxView>();
            Letterbox.Build(overlay.transform);
            Subtitles = overlay.gameObject.AddComponent<SubtitleView>();
            Subtitles.Build(overlay.transform);

            var screens = UIKit.CreateCanvas("ScreenCanvas", 20, transform);
            Journal = Add<JournalScreen>(screens);
            Choice = Add<ChoiceScreen>(screens);
            Shop = Add<ShopScreen>(screens);
            Bank = Add<BankScreen>(screens);
            Hacking = Add<HackingScreen>(screens);
            Lockpick = Add<LockpickScreen>(screens);
            Travel = Add<TravelScreen>(screens);
            GameOver = Add<GameOverScreen>(screens);
            Ending = Add<EndingScreen>(screens);

            var pause = UIKit.CreateCanvas("PauseCanvas", 30, transform);
            PauseMenu = pause.gameObject.AddComponent<PauseMenu>();
            PauseMenu.Build(pause.transform);
            SaveLoad = Add<SaveLoadScreen>(pause);
            Settings = Add<SettingsScreen>(pause);

            var top = UIKit.CreateCanvas("FaderCanvas", 50, transform);
            Fader = top.gameObject.AddComponent<ScreenFader>();
            Fader.Build(top.transform);
            Loading = top.gameObject.AddComponent<LoadingScreen>();
            Loading.Build(top.transform);
        }

        T Add<T>(Canvas canvas) where T : UIScreen
        {
            var s = canvas.gameObject.AddComponent<T>();
            s.Build(canvas.transform);
            return s;
        }

        void Update()
        {
            if (GameInput.Journal != null && GameInput.Journal.WasPressedThisFrame() && !SceneLoader.Instance.IsLoading)
            {
                if (Journal.IsOpen) Journal.Close();
                else if (!GameInput.GameplayBlocked && Player.PlayerCharacter.Instance != null) Journal.Open();
            }
        }

        public void PushScreen(UIScreen s) { m_stack.Remove(s); m_stack.Add(s); }
        public void PopScreen(UIScreen s) => m_stack.Remove(s);
        public bool AnyScreenOpen => m_stack.Count > 0;

        /// <summary>Closes the top-most closable screen. Returns true if something was closed or a modal is blocking.</summary>
        public bool CloseTopScreen()
        {
            if (m_stack.Count == 0) return false;
            var top = m_stack[m_stack.Count - 1];
            if (top.ClosableByEscape) top.Close();
            return true;
        }

        public void CloseAllScreens()
        {
            for (int i = m_stack.Count - 1; i >= 0; i--) m_stack[i].Close();
            m_stack.Clear();
            if (PauseController.Instance != null && PauseController.Instance.IsPaused) PauseController.Instance.Resume();
            Letterbox.Hide();
            Subtitles.Clear();
            HUD.ShowBoss(null, null);
            HUD.SetTimer(-1f);
        }
    }
}
