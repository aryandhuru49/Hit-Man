using ExitClause.Combat;
using ExitClause.Player;
using ExitClause.Stealth;
using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.AI
{
    /// <summary>
    /// The Alps sniper. Scans the valley from a nest; when it sees the player it paints them with
    /// a visible laser, charges, and fires a heavy shot unless line of sight is broken first.
    /// After each shot it must reload: that is the window to move between cover.
    /// </summary>
    [RequireComponent(typeof(AIPerception), typeof(Health))]
    public class SniperAI : MonoBehaviour
    {
        public WeaponDefinition rifle;
        public float chargeTime = 2.2f;
        public float reloadTime = 3f;
        public float closeRange = 15f;
        public float scanSpeed = 0.25f;
        public float scanArc = 100f;
        public Transform head;

        public bool IsCharging => m_charge > 0f;

        AIPerception m_perception;
        Health m_health;
        LineRenderer m_laser;
        float m_charge, m_reloadUntil;
        Quaternion m_base;

        void Awake()
        {
            m_perception = GetComponent<AIPerception>();
            m_health = GetComponent<Health>();
            m_base = (head != null ? head : transform).localRotation;
            m_laser = gameObject.AddComponent<LineRenderer>();
            m_laser.positionCount = 2;
            m_laser.startWidth = m_laser.endWidth = 0.025f;
            var db = GameDatabase.Instance;
            if (db != null) m_laser.sharedMaterial = db.tracerMaterial;
            m_laser.enabled = false;
            m_health.OnDied += _ => { m_laser.enabled = false; m_perception.Blind = true; enabled = false; };
        }

        void Update()
        {
            var p = PlayerCharacter.Instance;
            var pivot = head != null ? head : transform;
            if (p == null) return;
            Vector3 target = p.transform.position + Vector3.up * 1.2f;

            if (m_perception.CanSeePlayer && m_perception.Suspicion > 0.25f)
            {
                pivot.rotation = Quaternion.Slerp(pivot.rotation, Quaternion.LookRotation(target - m_perception.EyePosition), Time.deltaTime * 4f);
                m_laser.enabled = true;
                m_laser.SetPosition(0, m_perception.EyePosition);
                m_laser.SetPosition(1, target);
                if (Time.time < m_reloadUntil) { m_laser.enabled = false; return; }

                float dist = Vector3.Distance(transform.position, p.transform.position);
                m_charge += Time.deltaTime * (dist < closeRange ? 3f : 1f);
                if (m_charge >= chargeTime)
                {
                    m_charge = 0f;
                    m_reloadUntil = Time.time + reloadTime;
                    Hitscan.Fire(m_perception.EyePosition, target - m_perception.EyePosition, rifle, gameObject);
                    AlertManager.RaiseAlarm(p.transform.position, false);
                }
            }
            else
            {
                m_charge = Mathf.Max(0f, m_charge - Time.deltaTime);
                m_laser.enabled = false;
                float a = Mathf.Sin(Time.time * scanSpeed) * scanArc * 0.5f;
                pivot.localRotation = Quaternion.Slerp(pivot.localRotation, m_base * Quaternion.Euler(0, a, 0), Time.deltaTime * 2f);
            }
        }
    }
}
