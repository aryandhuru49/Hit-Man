using System.Collections;
using ExitClause.Combat;
using ExitClause.Story;
using ExitClause.UI;
using UnityEngine;
using UnityEngine.AI;

namespace ExitClause.AI
{
    /// <summary>
    /// Phase logic for bosses (the Handler, team leads). At each health threshold the boss becomes
    /// briefly invulnerable, says a line, falls back to the next position and activates reinforcements.
    /// </summary>
    [RequireComponent(typeof(EnemyAI))]
    public class BossController : MonoBehaviour
    {
        [System.Serializable]
        public class Phase
        {
            [Range(0, 1)] public float healthThreshold = 0.5f;
            public Transform fallbackPoint;
            public GameObject activateOnPhase;
            public DialogueLine line;
        }

        public string bossName = "The Handler";
        public Phase[] phases;

        Health m_health;
        int m_next;

        void Awake()
        {
            m_health = GetComponent<Health>();
            m_health.OnDamaged += OnDamaged;
        }

        void Start() => UIRoot.Instance.HUD.ShowBoss(bossName, m_health);

        void OnDamaged(DamageInfo info)
        {
            if (phases == null || m_next >= phases.Length || m_health.IsDead) return;
            if (m_health.Normalized > phases[m_next].healthThreshold) return;
            StartCoroutine(PhaseRoutine(phases[m_next]));
            m_next++;
        }

        IEnumerator PhaseRoutine(Phase phase)
        {
            m_health.invulnerable = true;
            if (phase.line != null && !string.IsNullOrEmpty(phase.line.text)) UIRoot.Instance.Subtitles.Show(phase.line);
            if (phase.activateOnPhase != null) phase.activateOnPhase.SetActive(true);
            var agent = GetComponent<NavMeshAgent>();
            if (phase.fallbackPoint != null && agent.isOnNavMesh) agent.SetDestination(phase.fallbackPoint.position);
            yield return new WaitForSeconds(2.5f);
            m_health.invulnerable = false;
        }

        void OnDestroy() { UIRoot.Instance?.HUD?.ShowBoss(null, null); }
    }
}
