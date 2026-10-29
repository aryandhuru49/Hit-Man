using System.Collections;
using ExitClause.Systems;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>
    /// One per location scene. Starts the scene's mission (resuming objectives when loaded from a save),
    /// plays the intro cutscene, autosaves, and when any mission completes plays the outro and
    /// travels to that mission's next scene.
    /// </summary>
    public class SceneMissionDirector : MonoBehaviour
    {
        public MissionDefinition mission;
        [Tooltip("Used instead of 'mission' when this flag is set (e.g. returning to the safehouse hub).")]
        public MissionDefinition alternateMission;
        public string alternateIfFlag;
        public CutsceneDirector introCutscene;
        public CutsceneDirector outroCutscene;
        public AudioClip music;
        [Tooltip("Optional radio dialogue after the intro.")]
        public DialogueSequence openingRadio;
        [Tooltip("Flags applied the first time this scene starts (not on reload).")]
        public string[] flagsOnFirstEnter;

        void Start()
        {
            GameInput.ForceCursorVisible = false;
            GameInput.RefreshCursor();
            if (PauseController.Instance != null) PauseController.Instance.Allowed = true;
            AudioService.Instance?.PlayMusic(music);
            UIRoot.Instance.HUD.SetVisible(true);
            MissionManager.Instance.OnMissionCompleted += OnMissionCompleted;
            StartCoroutine(Begin());
        }

        void OnDestroy()
        {
            if (MissionManager.Instance != null) MissionManager.Instance.OnMissionCompleted -= OnMissionCompleted;
        }

        IEnumerator Begin()
        {
            yield return null; // let the player and camera initialise
            var progress = GameSession.Instance.State.progress;
            bool resume = SceneLoader.Instance.ResumingFromSave && !string.IsNullOrEmpty(progress.missionId);
            bool useAlternate = alternateMission != null && GameSession.Instance.Story.HasFlag(alternateIfFlag);
            var toStart = useAlternate ? alternateMission : mission;
            if (resume)
            {
                var saved = GameDatabase.Instance.GetMission(progress.missionId);
                if (saved != null && saved.sceneName == mission.sceneName) toStart = saved;
            }

            if (!resume)
            {
                if (flagsOnFirstEnter != null) foreach (var f in flagsOnFirstEnter) GameSession.Instance.Story.SetFlag(f);
                if (introCutscene != null && !useAlternate)
                {
                    bool done = false;
                    introCutscene.Play(() => done = true);
                    while (!done) yield return null;
                }
            }

            MissionManager.Instance.StartMission(toStart, resume);
            if (!resume) SaveSystem.Save(SaveSystem.AutosaveSlot);
            if (openingRadio != null && !resume) DialogueRunner.Play(openingRadio);
        }

        void OnMissionCompleted(MissionDefinition m)
        {
            StartCoroutine(Finish(m));
        }

        IEnumerator Finish(MissionDefinition m)
        {
            yield return new WaitForSeconds(1.5f);
            var outro = m == mission ? outroCutscene : FindOutroFor(m);
            if (outro != null)
            {
                bool done = false;
                outro.Play(() => done = true);
                while (!done) yield return null;
            }
            if (!string.IsNullOrEmpty(m.nextScene))
            {
                var p = GameSession.Instance.State.progress;
                p.missionId = "";
                p.sceneName = m.nextScene;
                p.spawnPointId = m.nextSpawnPoint;
                SaveSystem.Save(SaveSystem.AutosaveSlot);
                SceneLoader.Instance.Load(m.nextScene, m.nextSpawnPoint);
            }
        }

        CutsceneDirector FindOutroFor(MissionDefinition m)
        {
            foreach (var c in FindObjectsByType<CutsceneDirector>(FindObjectsInactive.Include))
                if (c.playAfterMissionId == m.id) return c;
            return null;
        }
    }
}
