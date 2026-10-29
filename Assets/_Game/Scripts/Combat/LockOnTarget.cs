using System.Collections.Generic;
using UnityEngine;

namespace ExitClause.Combat
{
    /// <summary>Marks something the classic auto aim can lock onto (enemy agents, drones, the boss).</summary>
    public class LockOnTarget : MonoBehaviour
    {
        public static readonly List<LockOnTarget> All = new List<LockOnTarget>();

        public Transform aimPoint;
        Health m_health;

        public Vector3 AimPosition => aimPoint != null ? aimPoint.position : transform.position + Vector3.up * 1.3f;
        public bool IsValid => isActiveAndEnabled && (m_health == null || !m_health.IsDead);

        void Awake() { m_health = GetComponent<Health>(); }
        void OnEnable() => All.Add(this);
        void OnDisable() => All.Remove(this);
    }
}
