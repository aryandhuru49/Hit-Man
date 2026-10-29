using System;
using System.Collections.Generic;
using ExitClause.Story;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Systems
{
    /// <summary>
    /// Runs the active MissionDefinition: objectives activate in order, optional objectives can
    /// be completed at any time, and completing the last required objective completes the mission.
    /// Scene components report progress with MissionManager.Instance.Progress(objectiveId).
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager Instance { get; private set; }

        public MissionDefinition Current { get; private set; }
        public int CurrentIndex { get; private set; }
        public bool IsComplete { get; private set; }

        public event Action<MissionDefinition> OnMissionStarted;
        public event Action<ObjectiveDefinition> OnObjectiveActivated;
        public event Action<ObjectiveDefinition> OnObjectiveCompleted;
        public event Action<MissionDefinition> OnMissionCompleted;
        public event Action OnProgressChanged;

        readonly Dictionary<string, int> m_counts = new Dictionary<string, int>();
        readonly HashSet<string> m_done = new HashSet<string>();

        ProgressState P => GameSession.Instance.State.progress;

        void Awake() { Instance = this; }

        /// <summary>
        /// Called by SceneLoader before a new scene loads: scene objects subscribe to the mission events
        /// with lambdas, so drop them all and stop the old mission.
        /// </summary>
        public void ResetForSceneLoad()
        {
            OnMissionStarted = null;
            OnObjectiveActivated = null;
            OnObjectiveCompleted = null;
            OnMissionCompleted = null;
            Current = null;
            IsComplete = false;
            m_counts.Clear();
            m_done.Clear();
            OnProgressChanged?.Invoke();
        }

        public ObjectiveDefinition CurrentObjective =>
            Current != null && CurrentIndex < Current.objectives.Count ? Current.objectives[CurrentIndex] : null;

        public void StartMission(MissionDefinition mission, bool resume)
        {
            Current = mission;
            IsComplete = false;
            m_counts.Clear();
            m_done.Clear();
            CurrentIndex = 0;
            if (mission == null) return;

            if (resume && P.missionId == mission.id)
            {
                foreach (var key in P.completedObjectives)
                    if (key.StartsWith(mission.id + "/")) m_done.Add(key.Substring(mission.id.Length + 1));
            }
            else
            {
                P.completedObjectives.RemoveAll(k => k.StartsWith(mission.id + "/"));
            }
            P.missionId = mission.id;

            OnMissionStarted?.Invoke(mission);
            UIRoot.Instance?.HUD?.ShowMissionTitle(mission.title, mission.subtitle);
            AdvanceToNextOpen();
        }

        void AdvanceToNextOpen()
        {
            while (Current != null && CurrentIndex < Current.objectives.Count)
            {
                var o = Current.objectives[CurrentIndex];
                if (o.optional || m_done.Contains(o.id)) { CurrentIndex++; continue; }
                break;
            }
            P.objectiveIndex = CurrentIndex;
            OnProgressChanged?.Invoke();

            var obj = CurrentObjective;
            if (obj != null)
            {
                OnObjectiveActivated?.Invoke(obj);
                foreach (var f in obj.setFlagsOnActivate) GameSession.Instance.Story.SetFlag(f);
                // A clue noticed before its objective came up still counts.
                foreach (var clue in GameDatabase.Instance.clues)
                    if (clue != null && clue.objectiveId == obj.id && JournalManager.Instance.IsLogged(clue.id) && CurrentObjective == obj)
                    {
                        Progress(obj.id, obj.requiredCount);
                        return;
                    }
            }
            else if (Current != null && !IsComplete)
            {
                CompleteMission();
            }
        }

        public bool IsObjectiveActive(string objectiveId)
        {
            if (Current == null || IsComplete || m_done.Contains(objectiveId)) return false;
            var cur = CurrentObjective;
            if (cur != null && cur.id == objectiveId) return true;
            var o = Current.objectives.Find(x => x.id == objectiveId);
            return o != null && o.optional;
        }

        public bool IsObjectiveDone(string objectiveId) => m_done.Contains(objectiveId);

        public int GetCount(string objectiveId) => m_counts.TryGetValue(objectiveId, out var c) ? c : 0;

        /// <summary>Adds progress to an objective. Ignored unless the objective is currently active.</summary>
        public void Progress(string objectiveId, int amount = 1)
        {
            if (string.IsNullOrEmpty(objectiveId) || !IsObjectiveActive(objectiveId)) return;
            var o = Current.objectives.Find(x => x.id == objectiveId);
            m_counts[objectiveId] = GetCount(objectiveId) + amount;
            OnProgressChanged?.Invoke();
            if (m_counts[objectiveId] >= Mathf.Max(1, o.requiredCount)) CompleteObjective(o);
        }

        public void ForceComplete(string objectiveId)
        {
            if (Current == null) return;
            var o = Current.objectives.Find(x => x.id == objectiveId);
            if (o != null && !m_done.Contains(o.id)) CompleteObjective(o);
        }

        void CompleteObjective(ObjectiveDefinition o)
        {
            m_done.Add(o.id);
            P.completedObjectives.Add(Current.id + "/" + o.id);
            foreach (var f in o.setFlagsOnComplete) GameSession.Instance.Story.SetFlag(f);
            if (o.rewardCash != 0) EconomyManager.Instance.AddCash(o.rewardCash, "objective " + o.id);
            OnObjectiveCompleted?.Invoke(o);
            if (!o.optional && !o.hidden) UIRoot.Instance?.HUD?.Notify("Objective complete: " + o.text);
            if (CurrentObjective == o) { CurrentIndex++; AdvanceToNextOpen(); }
            else OnProgressChanged?.Invoke();
        }

        void CompleteMission()
        {
            IsComplete = true;
            var m = Current;
            if (!P.completedMissions.Contains(m.id)) P.completedMissions.Add(m.id);
            foreach (var f in m.setFlagsOnComplete) GameSession.Instance.Story.SetFlag(f);
            // INSECURE: reward amount comes straight from client-side data.
            if (m.rewardCash != 0) EconomyManager.Instance.AddCash(m.rewardCash, "mission " + m.id);
            UIRoot.Instance?.HUD?.ShowMissionTitle("MISSION COMPLETE", m.title);
            OnMissionCompleted?.Invoke(m);
        }
    }
}
