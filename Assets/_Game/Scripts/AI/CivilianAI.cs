using ExitClause.Combat;
using ExitClause.Player;
using ExitClause.Systems;
using UnityEngine;
using UnityEngine.AI;

namespace ExitClause.AI
{
    /// <summary>
    /// Ambient civilian with a daily routine: walks a route, lingers at each stop, and only
    /// follows the route during its active hours. Panics and flees from gunfire or a drawn weapon.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class CivilianAI : MonoBehaviour, INoiseListener
    {
        public PatrolRoute route;
        public Vector2 lingerSeconds = new Vector2(3f, 9f);
        [Tooltip("In-game hours when this civilian is out (start, end). 0-24 = always.")]
        public Vector2 activeHours = new Vector2(0f, 24f);
        public float walkSpeed = 1.4f;
        public float fleeSpeed = 5f;

        public bool IsPanicking => Time.time < m_panicUntil;
        public Vector3 ListenerPosition => transform.position;

        NavMeshAgent m_agent;
        int m_index, m_dir = 1;
        float m_lingerUntil, m_panicUntil;
        Vector3 m_home;

        void Awake()
        {
            m_agent = GetComponent<NavMeshAgent>();
            m_home = transform.position;
            gameObject.layer = Layers.Character;
            var h = GetComponent<Health>();
            if (h != null) h.OnDied += _ => { m_agent.enabled = false; transform.rotation = Quaternion.Euler(-90, transform.eulerAngles.y, 0); enabled = false; };
        }

        void OnEnable() => NoiseSystem.Register(this);
        void OnDisable() => NoiseSystem.Unregister(this);

        void Start()
        {
            if (!m_agent.isOnNavMesh) m_agent.Warp(transform.position);
            m_index = route != null && route.Count > 0 ? Random.Range(0, route.Count) : 0;
            m_agent.speed = walkSpeed;
        }

        void Update()
        {
            if (!m_agent.isOnNavMesh) return;
            var p = PlayerCharacter.Instance;
            if (p != null && p.Combat.IsAiming && Vector3.Distance(p.transform.position, transform.position) < 10f) Panic(p.transform.position);

            if (IsPanicking) return;
            m_agent.speed = walkSpeed;

            float hour = GameClock.Instance != null ? GameClock.Instance.Hour : 12f;
            bool active = hour >= activeHours.x && hour <= activeHours.y;
            if (!active || route == null || route.Count == 0)
            {
                if (Vector3.Distance(transform.position, m_home) > 1f) m_agent.SetDestination(m_home);
                return;
            }

            if (!m_agent.pathPending && m_agent.remainingDistance < 0.6f)
            {
                if (m_lingerUntil <= 0f) m_lingerUntil = Time.time + Random.Range(lingerSeconds.x, lingerSeconds.y);
                else if (Time.time > m_lingerUntil)
                {
                    m_lingerUntil = 0f;
                    m_index = route.Next(m_index, ref m_dir);
                    m_agent.SetDestination(route.Get(m_index));
                }
            }
        }

        public void HearNoise(Vector3 position, float loudness, NoiseType type, GameObject source)
        {
            if (type == NoiseType.Gunshot || type == NoiseType.Alarm) Panic(position);
        }

        public void Panic(Vector3 threat)
        {
            if (!m_agent.isOnNavMesh) return;
            m_panicUntil = Time.time + 8f;
            m_agent.speed = fleeSpeed;
            Vector3 away = (transform.position - threat).normalized * 15f;
            if (NavMesh.SamplePosition(transform.position + away, out var hit, 6f, NavMesh.AllAreas)) m_agent.SetDestination(hit.position);
        }
    }
}
