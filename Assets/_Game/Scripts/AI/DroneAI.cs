using ExitClause.Combat;
using ExitClause.Player;
using ExitClause.Stealth;
using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.AI
{
    /// <summary>
    /// The Tech Specialist's surveillance drone: flies a route at fixed height, looks down with
    /// a vision cone, raises the alarm when it spots the player, then hovers above and shoots.
    /// </summary>
    [RequireComponent(typeof(AIPerception), typeof(Health))]
    public class DroneAI : MonoBehaviour
    {
        public PatrolRoute route;
        public float height = 6f;
        public float speed = 4f;
        public WeaponDefinition weapon;

        AIPerception m_perception;
        Health m_health;
        int m_index, m_dir = 1;
        bool m_alerted;
        float m_nextShot;

        void Awake()
        {
            m_perception = GetComponent<AIPerception>();
            m_health = GetComponent<Health>();
            m_health.OnDied += _ =>
            {
                m_perception.Blind = true;
                var rb = gameObject.AddComponent<Rigidbody>();
                rb.mass = 5f;
                enabled = false;
            };
        }

        void Update()
        {
            var p = PlayerCharacter.Instance;
            Vector3 target;
            if (m_perception.Suspicion >= 1f && p != null)
            {
                if (!m_alerted) { m_alerted = true; AlertManager.RaiseAlarm(p.transform.position, true); }
                target = p.transform.position + Vector3.up * height * 0.7f - p.transform.forward * 3f;
                if (weapon != null && Time.time > m_nextShot && m_perception.CanSeePlayer)
                {
                    m_nextShot = Time.time + 1f / weapon.fireRate + 0.5f;
                    Vector3 aim = p.transform.position + Vector3.up;
                    if (Random.value > weapon.aiAccuracy) aim += Random.insideUnitSphere * 1.5f;
                    Hitscan.Fire(transform.position - Vector3.up * 0.3f, aim - transform.position, weapon, gameObject);
                }
                if (!m_perception.CanSeePlayer && Time.time - m_perception.LastSeenTime > 8f) { m_alerted = false; m_perception.Suspicion = 0.5f; }
            }
            else if (route != null && route.Count > 0)
            {
                target = route.Get(m_index);
                target.y = route.Get(m_index).y + height;
                if (Vector3.Distance(transform.position, target) < 1f) m_index = route.Next(m_index, ref m_dir);
            }
            else return;

            Vector3 to = target - transform.position;
            transform.position += Vector3.ClampMagnitude(to, speed * Time.deltaTime);
            Vector3 flat = new Vector3(to.x, 0, to.z);
            if (flat.sqrMagnitude > 0.1f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat), Time.deltaTime * 3f);
        }
    }
}
