using ExitClause.Player;
using ExitClause.Stealth;
using ExitClause.Systems;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>
    /// A face in the Marseille crowd. Study people to spot the Infiltrator's tells, then confront the
    /// one you believe is her. A wrong accusation blows your cover and brings the agency down on you.
    /// </summary>
    public class CrowdSuspect : Interactable
    {
        [TextArea] public string observation;
        public bool isInfiltrator;
        public string studyObjective = "study_crowd";
        public string confrontObjective = "confront";
        [Tooltip("Activated when the infiltrator is unmasked (she and her backup).")]
        public GameObject[] revealObjects;

        bool m_studied;
        static bool s_resolved;

        void Start() { s_resolved = false; }

        public override string Prompt
        {
            get
            {
                if (MissionManager.Instance.IsObjectiveActive(confrontObjective)) return "Confront";
                return m_studied ? "Studied" : "Study this person";
            }
        }

        public override bool CanInteract(PlayerCharacter player)
        {
            if (!base.CanInteract(player) || s_resolved) return false;
            var mm = MissionManager.Instance;
            return (mm.IsObjectiveActive(studyObjective) && !m_studied) || mm.IsObjectiveActive(confrontObjective);
        }

        public override void Interact(PlayerCharacter player)
        {
            var mm = MissionManager.Instance;
            if (mm.IsObjectiveActive(confrontObjective))
            {
                s_resolved = true;
                if (!isInfiltrator)
                {
                    UIRoot.Instance.HUD.Notify("Wrong person. Your cover is blown!");
                    AlertManager.RaiseAlarm(player.transform.position, true);
                }
                else UIRoot.Instance.HUD.Notify("\"...Adrian. You always did have good eyes.\"");
                foreach (var go in revealObjects) if (go != null) go.SetActive(true);
                if (isInfiltrator) gameObject.SetActive(false);
                else foreach (var s in FindObjectsByType<CrowdSuspect>(FindObjectsInactive.Exclude)) if (s.isInfiltrator) s.gameObject.SetActive(false);
                mm.Progress(confrontObjective);
                return;
            }
            m_studied = true;
            UIRoot.Instance.Subtitles.Show(new DialogueLine("Adrian (thinking)", observation, 5f));
            mm.Progress(studyObjective);
        }
    }
}
