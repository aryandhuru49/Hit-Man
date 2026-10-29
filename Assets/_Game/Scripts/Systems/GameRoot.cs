using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Systems
{
    /// <summary>
    /// Persistent object hosting every global manager and the global UI. Created once by
    /// GameBootstrap and kept alive across scene loads.
    /// </summary>
    public class GameRoot : MonoBehaviour
    {
        public static GameRoot Instance { get; private set; }

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            GameDatabase.Load();
            GameInput.Enable();

            gameObject.AddComponent<GameSession>();
            gameObject.AddComponent<SettingsManager>();
            gameObject.AddComponent<EconomyManager>();
            gameObject.AddComponent<InventoryManager>();
            gameObject.AddComponent<JournalManager>();
            gameObject.AddComponent<MissionManager>();
            gameObject.AddComponent<GameClock>();
            gameObject.AddComponent<SceneLoader>();
            gameObject.AddComponent<AudioService>();
            gameObject.AddComponent<UIRoot>();
            gameObject.AddComponent<PauseController>();

            // Pressing Play in any scene starts a fresh session; the main menu may replace it.
            GameSession.Instance.NewGame();
        }
    }
}
