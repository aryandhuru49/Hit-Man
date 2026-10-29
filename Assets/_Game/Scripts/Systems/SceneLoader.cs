using System.Collections;
using ExitClause.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ExitClause.Systems
{
    /// <summary>
    /// Loads location scenes asynchronously behind the loading screen. Each location is its
    /// own scene so memory stays low on a 16 GB machine.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }
        public bool IsLoading { get; private set; }

        /// <summary>True when the current scene was entered from a save (mission resumes instead of restarting).</summary>
        public bool ResumingFromSave { get; private set; }

        void Awake() { Instance = this; }

        public void Load(string sceneName, string spawnPointId = "", bool fromSave = false)
        {
            if (IsLoading) return;
            StartCoroutine(LoadRoutine(sceneName, spawnPointId, fromSave));
        }

        IEnumerator LoadRoutine(string sceneName, string spawnPointId, bool fromSave)
        {
            IsLoading = true;
            ResumingFromSave = fromSave;
            Time.timeScale = 1f;
            GameInput.ResetBlocks();
            UIRoot.Instance.CloseAllScreens();
            var loading = UIRoot.Instance.Loading;
            loading.Show(sceneName);
            yield return null;

            var p = GameSession.Instance.State.progress;
            p.sceneName = sceneName;
            p.spawnPointId = spawnPointId;

            MissionManager.Instance.ResetForSceneLoad();
            GameClock.Instance.ResetForSceneLoad();
            var op = SceneManager.LoadSceneAsync(sceneName);
            if (op == null)
            {
                Debug.LogError($"[SceneLoader] Scene '{sceneName}' is not in Build Settings. Run Exit Clause > Build Everything.");
                loading.Hide();
                IsLoading = false;
                yield break;
            }
            while (!op.isDone)
            {
                loading.SetProgress(op.progress / 0.9f);
                yield return null;
            }
            loading.SetProgress(1f);
            yield return new WaitForSecondsRealtime(0.3f);
            loading.Hide();
            IsLoading = false;
        }

        public void ReloadCurrent()
        {
            Load(SceneManager.GetActiveScene().name, GameSession.Instance.State.progress.spawnPointId, true);
        }
    }
}
