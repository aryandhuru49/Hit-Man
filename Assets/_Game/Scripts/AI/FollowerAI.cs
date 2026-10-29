using ExitClause.CameraSystem;
using ExitClause.Player;
using ExitClause.Systems;
using UnityEngine;
using UnityEngine.AI;

namespace ExitClause.AI
{
    /// <summary>
    /// Someone tailing the player (Act 0 paranoia). Keeps a casual distance behind, and when the
    /// player turns to look at them they stop and "act natural" by facing away.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class FollowerAI : MonoBehaviour
    {
        public float minDistance = 7f;
        public float maxDistance = 14f;
        public float walkSpeed = 1.6f;
        public float catchUpSpeed = 3.2f;
        [Tooltip("Only tail while this objective is active (empty = always).")]
        public string activeDuringObjective;

        public bool IsActingNatural { get; private set; }

        NavMeshAgent m_agent;
        float m_repath;

        void Awake()
        {
            m_agent = GetComponent<NavMeshAgent>();
            gameObject.layer = Layers.Character;
        }

        void Start()
        {
            if (!m_agent.isOnNavMesh) m_agent.Warp(transform.position);
        }

        void Update()
        {
            var p = PlayerCharacter.Instance;
            if (p == null || !m_agent.isOnNavMesh) return;
            if (!string.IsNullOrEmpty(activeDuringObjective) && !MissionManager.Instance.IsObjectiveActive(activeDuringObjective))
            {
                m_agent.ResetPath();
                return;
            }

            Vector3 toMe = transform.position - p.transform.position;
            float dist = toMe.magnitude;
            var rig = GameCameraRig.Instance;
            bool watched = rig != null && dist < 25f &&
                           Vector3.Angle(rig.MainCamera.transform.forward, transform.position - rig.MainCamera.transform.position) < 18f;

            IsActingNatural = watched;
            if (watched)
            {
                m_agent.ResetPath();
                Quaternion away = Quaternion.LookRotation(Vector3.ProjectOnPlane(toMe, Vector3.up).normalized + transform.right * 0.5f);
                transform.rotation = Quaternion.Slerp(transform.rotation, away, Time.deltaTime * 3f);
                return;
            }

            m_repath -= Time.deltaTime;
            if (m_repath > 0f) return;
            m_repath = 0.5f;
            if (dist > maxDistance)
            {
                m_agent.speed = catchUpSpeed;
                m_agent.SetDestination(p.transform.position - toMe.normalized * -minDistance);
            }
            else if (dist < minDistance)
            {
                m_agent.ResetPath();
            }
            else
            {
                m_agent.speed = walkSpeed;
                m_agent.SetDestination(p.transform.position + toMe.normalized * minDistance);
            }
        }
    }
}
