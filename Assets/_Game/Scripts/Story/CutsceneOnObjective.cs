using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>Plays a cutscene when an objective starts or completes, then optionally advances another objective.</summary>
    public class CutsceneOnObjective : MonoBehaviour
    {
        public string objectiveId;
        public bool onComplete;
        public CutsceneDirector cutscene;
        [Tooltip("Progressed when the cutscene ends (optional).")]
        public string progressWhenDone;

        bool m_played;

        void Start()
        {
            var mm = MissionManager.Instance;
            if (onComplete) mm.OnObjectiveCompleted += o => { if (o.id == objectiveId) Play(); };
            else
            {
                mm.OnObjectiveActivated += o => { if (o.id == objectiveId) Play(); };
                if (mm.IsObjectiveActive(objectiveId)) Play();
            }
        }

        void Play()
        {
            if (m_played || cutscene == null) return;
            m_played = true;
            cutscene.Play(() => MissionManager.Instance.Progress(progressWhenDone));
        }
    }
}
