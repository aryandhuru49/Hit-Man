using System.Collections.Generic;
using ExitClause.Combat;
using ExitClause.Player;
using ExitClause.Stealth;
using ExitClause.Systems;
using UnityEngine;
using UnityEngine.AI;

namespace ExitClause.AI
{
    /// <summary>
    /// Enemy agent: Patrol -> Investigate -> Search -> Chase/Combat. Uses AIPerception for sight,
    /// hearing and suspicion, a NavMeshAgent for movement and a WeaponDefinition for shooting.
    /// Can be taken down from behind while unaware, and leaves a body other guards can find.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent), typeof(Health), typeof(AIPerception))]
    public class EnemyAI : MonoBehaviour
    {
        public static readonly List<EnemyAI> All = new List<EnemyAI>();

        public string displayName = "Agent";
        public WeaponDefinition weapon;
        public PatrolRoute route;
        public float walkSpeed = 1.8f;
        public float runSpeed = 4.8f;
        public float combatRange = 16f;
        public float alertRadius = 18f;
        public float investigateLookTime = 4f;
        public float searchDuration = 12f;
        [Tooltip("Members of the five person team count toward the team-killed counter.")]
        public bool isTeamMember;
        [Tooltip("Disguise left behind on the body (empty = none).")]
        public string dropsDisguise = "";
        [Tooltip("Item placed in the player's inventory when the body is searched.")]
        public string dropsItem = "";
        public bool canBeTakenDown = true;
        [Tooltip("Stays at its post and only turns, never patrols.")]
        public bool stationary;

        public EnemyState State { get; private set; } = EnemyState.Patrol;
        public AIPerception Perception { get; private set; }
        public Health Health { get; private set; }
        public bool IsDead => Health == null || Health.IsDead;

        NavMeshAgent m_agent;
        int m_routeIndex, m_routeDir = 1;
        float m_stateTimer, m_nextShot, m_waitTimer;
        Vector3 m_investigatePoint, m_homePos;
        Quaternion m_homeRot;

        void Awake()
        {
            m_agent = GetComponent<NavMeshAgent>();
            Health = GetComponent<Health>();
            Perception = GetComponent<AIPerception>();
            m_homePos = transform.position;
            m_homeRot = transform.rotation;
            gameObject.layer = Layers.Character;
        }

        void OnEnable() => All.Add(this);
        void OnDisable() => All.Remove(this);

        void Start()
        {
            if (!m_agent.isOnNavMesh) m_agent.Warp(transform.position);
            Health.OnDied += OnDied;
            Health.OnDamaged += OnDamaged;
            Perception.OnNoise += OnNoise;
            m_agent.speed = walkSpeed;
            if (route != null && route.Count > 0) m_agent.SetDestination(route.Get(0));
        }

        void Update()
        {
            if (IsDead || !m_agent.isOnNavMesh) return;
            m_stateTimer += Time.deltaTime;

            if (Perception.Suspicion >= 1f && State != EnemyState.Combat) EnterCombat();
            else if (Perception.Suspicion >= 0.4f && (State == EnemyState.Patrol || State == EnemyState.Search) && Perception.CanSeePlayer)
                Investigate(Perception.LastKnownPlayerPosition);

            switch (State)
            {
                case EnemyState.Patrol: TickPatrol(); break;
                case EnemyState.Investigate: TickInvestigate(); break;
                case EnemyState.Search: TickSearch(); break;
                case EnemyState.Chase:
                case EnemyState.Combat: TickCombat(); break;
            }
        }

        void TickPatrol()
        {
            m_agent.speed = walkSpeed;
            if (stationary || route == null || route.Count == 0)
            {
                if (!m_agent.pathPending && m_agent.remainingDistance < 0.5f)
                    transform.rotation = Quaternion.Slerp(transform.rotation, m_homeRot, Time.deltaTime * 2f);
                if (Vector3.Distance(transform.position, m_homePos) > 1f) m_agent.SetDestination(m_homePos);
                return;
            }
            if (!m_agent.pathPending && m_agent.remainingDistance < 0.4f)
            {
                m_waitTimer += Time.deltaTime;
                if (m_waitTimer >= route.waitAtPoint)
                {
                    m_waitTimer = 0f;
                    m_routeIndex = route.Next(m_routeIndex, ref m_routeDir);
                    m_agent.SetDestination(route.Get(m_routeIndex));
                }
            }
        }

        public void Investigate(Vector3 point)
        {
            if (IsDead || State == EnemyState.Combat) return;
            State = EnemyState.Investigate;
            m_stateTimer = 0f;
            m_investigatePoint = point;
            m_agent.speed = walkSpeed * 1.3f;
            m_agent.SetDestination(point);
        }

        void TickInvestigate()
        {
            if (!m_agent.pathPending && m_agent.remainingDistance < 1.2f)
            {
                transform.Rotate(0, 60f * Time.deltaTime, 0);
                m_waitTimer += Time.deltaTime;
                if (m_waitTimer > investigateLookTime) { m_waitTimer = 0; ReturnToPatrol(); }
            }
            else if (m_stateTimer > 20f) ReturnToPatrol();
        }

        void TickSearch()
        {
            if (m_stateTimer > searchDuration) { ReturnToPatrol(); return; }
            if (!m_agent.pathPending && m_agent.remainingDistance < 1f)
            {
                Vector2 r = Random.insideUnitCircle * 7f;
                Vector3 p = m_investigatePoint + new Vector3(r.x, 0, r.y);
                if (NavMesh.SamplePosition(p, out var hit, 3f, NavMesh.AllAreas)) m_agent.SetDestination(hit.position);
            }
        }

        void ReturnToPatrol()
        {
            State = EnemyState.Patrol;
            m_stateTimer = 0f;
            Perception.HoldSuspicion = false;
            Perception.Suspicion = Mathf.Min(Perception.Suspicion, 0.3f);
            if (route != null && route.Count > 0) m_agent.SetDestination(route.Get(m_routeIndex));
            else m_agent.SetDestination(m_homePos);
        }

        public void EnterCombat()
        {
            if (IsDead) return;
            bool wasCalm = State != EnemyState.Combat;
            State = EnemyState.Combat;
            m_stateTimer = 0f;
            Perception.Suspicion = 1f;
            Perception.HoldSuspicion = true;
            if (wasCalm) AlertNearby();
        }

        void AlertNearby()
        {
            foreach (var e in All)
            {
                if (e == this || e.IsDead) continue;
                if (Vector3.Distance(e.transform.position, transform.position) > alertRadius) continue;
                e.Perception.Suspicion = Mathf.Max(e.Perception.Suspicion, 0.6f);
                e.Investigate(Perception.LastKnownPlayerPosition);
            }
        }

        void TickCombat()
        {
            var p = PlayerCharacter.Instance;
            if (p == null || p.Health.IsDead) { ReturnToPatrol(); return; }
            Vector3 target = p.CurrentVehicle != null ? p.CurrentVehicle.transform.position : p.transform.position;
            float dist = Vector3.Distance(transform.position, target);

            if (Perception.CanSeePlayer)
            {
                m_stateTimer = 0f;
                Vector3 look = target - transform.position; look.y = 0;
                if (look.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), Time.deltaTime * 8f);
                if (dist > combatRange * 0.8f) { m_agent.speed = runSpeed; m_agent.SetDestination(target); }
                else m_agent.ResetPath();
                if (weapon != null && dist <= weapon.range && Time.time >= m_nextShot) Shoot(p, target, dist);
            }
            else
            {
                m_agent.speed = runSpeed;
                m_agent.SetDestination(Perception.LastKnownPlayerPosition);
                if (m_stateTimer > 7f)
                {
                    State = EnemyState.Search;
                    m_stateTimer = 0f;
                    m_investigatePoint = Perception.LastKnownPlayerPosition;
                    Perception.HoldSuspicion = false;
                    Perception.Suspicion = 0.7f;
                }
            }
        }

        void Shoot(PlayerCharacter p, Vector3 target, float dist)
        {
            m_nextShot = Time.time + 1f / Mathf.Max(0.1f, weapon.fireRate) + Random.Range(0.1f, 0.6f);
            Vector3 origin = Perception.EyePosition - Vector3.up * 0.2f + transform.forward * 0.4f;
            Vector3 aim = target + Vector3.up * 1.1f;
            float accuracy = weapon.aiAccuracy * Mathf.Lerp(1.3f, 0.5f, dist / weapon.range);
            if (p.Motor.IsInCover && p.Motor.IsLowCover && !p.Combat.IsAiming) accuracy *= 0.3f;
            if (Random.value > accuracy) aim += Random.insideUnitSphere * 1.4f;
            Hitscan.Fire(origin, aim - origin, weapon, gameObject);
        }

        void OnNoise(Vector3 pos, float loudness, NoiseType type, GameObject source)
        {
            if (IsDead || State == EnemyState.Combat) return;
            switch (type)
            {
                case NoiseType.Gunshot:
                case NoiseType.Alarm:
                    Perception.Suspicion = Mathf.Max(Perception.Suspicion, 0.75f);
                    Perception.SuspicionMultiplier = Mathf.Max(Perception.SuspicionMultiplier, 1.5f);
                    Investigate(pos);
                    break;
                case NoiseType.Distraction:
                case NoiseType.Impact:
                case NoiseType.Takedown:
                    Investigate(pos);
                    break;
                case NoiseType.Footstep:
                    Perception.Suspicion = Mathf.Min(0.6f, Perception.Suspicion + 0.25f * loudness);
                    if (Perception.Suspicion > 0.35f) Investigate(pos);
                    break;
            }
        }

        void OnDamaged(DamageInfo info)
        {
            if (IsDead) return;
            if (info.source != null && info.source.GetComponent<PlayerCharacter>() != null)
            {
                Perception.Suspicion = 1f;
                EnterCombat();
            }
        }

        public bool CanBeTakenDown(Transform attacker)
        {
            if (!canBeTakenDown || IsDead || State == EnemyState.Combat) return false;
            Vector3 toAttacker = (attacker.position - transform.position).normalized;
            bool behind = Vector3.Dot(transform.forward, toAttacker) < 0.2f;
            return behind || !Perception.CanSeePlayer;
        }

        public void ReceiveTakedown(GameObject attacker)
        {
            Health.Kill(new DamageInfo(999f, DamageType.Takedown, attacker, transform.position, attacker.transform.forward));
        }

        void OnDied(DamageInfo info)
        {
            State = EnemyState.Dead;
            Perception.Blind = true;
            if (m_agent.isOnNavMesh) m_agent.ResetPath();
            m_agent.enabled = false;
            var p = PlayerCharacter.Instance;
            bool byPlayer = info.source != null && p != null && info.source == p.gameObject;
            if (byPlayer && p.Rage != null) p.Rage.OnPlayerKill(transform.position);
            if (isTeamMember) GameSession.Instance.Story.AddCounter(StoryKeys.TeamKilled, 1);
            DeadBody.MakeBody(this);
        }

        /// <summary>Sends this agent to a point at high alert (used by alarms and cameras).</summary>
        public void RespondToAlarm(Vector3 point)
        {
            if (IsDead) return;
            Perception.Suspicion = Mathf.Max(Perception.Suspicion, 0.8f);
            Perception.SuspicionMultiplier = Mathf.Max(Perception.SuspicionMultiplier, 2f);
            if (State != EnemyState.Combat) Investigate(point);
        }
    }
}
