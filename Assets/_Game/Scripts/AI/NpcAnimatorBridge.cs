using ExitClause.Combat;
using UnityEngine;

namespace ExitClause.AI
{
    /// <summary>
    /// Feeds guards, civilians and story NPCs into the same Animator parameters as the player
    /// (Speed, Aim, Grounded). Speed comes from how far the character moved, so it works with or
    /// without a NavMeshAgent. Freezes the pose on death (the body is laid flat by DeadBody).
    /// </summary>
    public class NpcAnimatorBridge : MonoBehaviour
    {
        public Animator animator;
        EnemyAI m_enemy;
        Health m_health;
        Vector3 m_lastPos;
        float m_speed;
        static readonly int Speed = Animator.StringToHash("Speed");
        static readonly int Aim = Animator.StringToHash("Aim");
        static readonly int Grounded = Animator.StringToHash("Grounded");

        void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            m_enemy = GetComponent<EnemyAI>();
            m_health = GetComponent<Health>();
            m_lastPos = transform.position;
        }

        void Update()
        {
            if (animator == null || animator.runtimeAnimatorController == null) return;
            if (m_health != null && m_health.IsDead) { animator.speed = 0f; return; }
            var delta = transform.position - m_lastPos;
            delta.y = 0f;
            m_lastPos = transform.position;
            float target = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;
            m_speed = Mathf.Lerp(m_speed, target, 10f * Time.deltaTime);
            Set(Speed, m_speed);
            Set(Aim, m_enemy != null && m_enemy.State == EnemyState.Combat);
            Set(Grounded, true);
        }

        void Set(int hash, float v) { foreach (var p in animator.parameters) if (p.nameHash == hash) { animator.SetFloat(hash, v); return; } }
        void Set(int hash, bool v) { foreach (var p in animator.parameters) if (p.nameHash == hash) { animator.SetBool(hash, v); return; } }
    }
}
