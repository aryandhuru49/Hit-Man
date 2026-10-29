using ExitClause.Systems;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>Completes an objective after the player survives for a number of seconds while it is active.</summary>
    public class SurvivalTimer : MonoBehaviour
    {
        public string objectiveId;
        public float seconds = 45f;
        float m_elapsed;

        void Update()
        {
            if (!MissionManager.Instance.IsObjectiveActive(objectiveId)) { m_elapsed = 0f; return; }
            m_elapsed += Time.deltaTime;
            UIRoot.Instance.HUD.SetTimer(seconds - m_elapsed);
            if (m_elapsed >= seconds)
            {
                UIRoot.Instance.HUD.SetTimer(-1f);
                MissionManager.Instance.Progress(objectiveId);
            }
        }
    }
}
