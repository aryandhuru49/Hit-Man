using UnityEngine;

namespace ExitClause.Stealth
{
    /// <summary>
    /// Sweeping CCTV camera. Fully spotting the player raises the scene alarm. Relays (hacked through
    /// a terminal) switch off every camera that shares their group id.
    /// </summary>
    [RequireComponent(typeof(AIPerception))]
    public class SecurityCamera : MonoBehaviour
    {
        public string groupId = "cctv";
        public float sweepAngle = 70f;
        public float sweepSpeed = 0.35f;
        public bool Disabled { get; private set; }

        AIPerception m_perception;
        Quaternion m_base;
        float m_cooldown;

        void Awake()
        {
            m_perception = GetComponent<AIPerception>();
            m_base = transform.localRotation;
        }

        void Update()
        {
            if (Disabled) return;
            float a = Mathf.Sin(Time.time * sweepSpeed) * sweepAngle * 0.5f;
            transform.localRotation = m_base * Quaternion.Euler(0, a, 0);
            m_cooldown -= Time.deltaTime;
            if (m_perception.Suspicion >= 1f && m_cooldown <= 0f)
            {
                m_cooldown = 10f;
                AlertManager.RaiseAlarm(m_perception.LastKnownPlayerPosition, true);
                m_perception.Suspicion = 0.5f;
            }
        }

        public static void DisableGroup(string groupId)
        {
            foreach (var cam in FindObjectsByType<SecurityCamera>(FindObjectsInactive.Exclude))
            {
                if (cam.groupId != groupId) continue;
                cam.Disabled = true;
                cam.m_perception.Blind = true;
                cam.m_perception.Suspicion = 0f;
            }
        }
    }
}
