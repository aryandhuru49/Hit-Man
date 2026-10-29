using System;
using ExitClause.Story;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Systems
{
    /// <summary>
    /// Suspicion Journal: records observations, multi-day clue progress and pins on the suspect board.
    /// Observing a clue enough times logs it; logged clues can be pinned to suspects.
    /// </summary>
    public class JournalManager : MonoBehaviour
    {
        public static JournalManager Instance { get; private set; }
        public event Action<ClueDefinition> OnClueLogged;
        public event Action OnBoardChanged;

        JournalState J => GameSession.Instance.State.journal;

        void Awake() { Instance = this; }

        public bool IsLogged(string clueId) => J.loggedClues.Contains(clueId);

        public int ObservationCount(string clueId)
        {
            var p = J.observationProgress.Find(x => x.clueId == clueId);
            return p != null ? p.count : 0;
        }

        /// <summary>Registers one sighting. Clues that need separate days only count once per in-game day.</summary>
        public bool Observe(ClueDefinition clue)
        {
            if (clue == null || IsLogged(clue.id)) return false;
            var p = J.observationProgress.Find(x => x.clueId == clue.id);
            if (p == null) { p = new ClueObservationProgress { clueId = clue.id }; J.observationProgress.Add(p); }
            int today = GameSession.Instance.State.progress.day;
            if (clue.requiresSeparateDays && p.lastDayObserved == today)
            {
                UIRoot.Instance?.HUD?.Notify("Already noted today.");
                return false;
            }
            p.count++;
            p.lastDayObserved = today;
            if (p.count >= Mathf.Max(1, clue.observationsRequired))
            {
                Log(clue);
                return true;
            }
            UIRoot.Instance?.HUD?.Notify($"Noted: {clue.title} ({p.count}/{clue.observationsRequired})");
            return true;
        }

        public void Log(ClueDefinition clue)
        {
            if (clue == null || IsLogged(clue.id)) return;
            J.loggedClues.Add(clue.id);
            var story = GameSession.Instance.Story;
            story.SetFlag("clue_" + clue.id);
            if (clue.evidenceValue != 0) story.AddCounter(StoryKeys.EvidenceCollected, clue.evidenceValue);
            UIRoot.Instance?.HUD?.Notify("Journal updated: " + clue.title + "  [J]");
            OnClueLogged?.Invoke(clue);
            if (!string.IsNullOrEmpty(clue.objectiveId)) MissionManager.Instance.Progress(clue.objectiveId);
        }

        public void Pin(string clueId, string suspectId)
        {
            J.pins.RemoveAll(p => p.clueId == clueId);
            J.pins.Add(new BoardPin { clueId = clueId, suspectId = suspectId });
            MissionManager.Instance.Progress("pin_board");
            OnBoardChanged?.Invoke();
        }

        public void Unpin(string clueId)
        {
            J.pins.RemoveAll(p => p.clueId == clueId);
            OnBoardChanged?.Invoke();
        }

        public string PinnedSuspect(string clueId) => J.pins.Find(p => p.clueId == clueId)?.suspectId;

        /// <summary>Number of clues pinned to the suspect the clue actually points to.</summary>
        public int CorrectPins()
        {
            int n = 0;
            foreach (var pin in J.pins)
            {
                var clue = GameDatabase.Instance.GetClue(pin.clueId);
                if (clue != null && clue.trueSuspectId == pin.suspectId) n++;
            }
            return n;
        }
    }
}
