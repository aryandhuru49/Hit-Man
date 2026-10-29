using System;
using UnityEngine;

namespace ExitClause.Combat
{
    /// <summary>
    /// Hit points for the player, NPCs, drones and cars.
    /// INSECURE BY DESIGN: current is a plain public float (freeze it with a memory editor for god mode).
    /// </summary>
    public class Health : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float current = 100f;
        public bool invulnerable;
        [Tooltip("Multiplies incoming damage (bosses, armored targets).")]
        public float damageMultiplier = 1f;

        public bool IsDead => current <= 0f;
        public DamageInfo LastDamage { get; private set; }

        public event Action<DamageInfo> OnDamaged;
        public event Action<DamageInfo> OnDied;

        public float Normalized => maxHealth > 0 ? Mathf.Clamp01(current / maxHealth) : 0f;

        public void TakeDamage(DamageInfo info)
        {
            if (IsDead || invulnerable) return;
            info.amount *= damageMultiplier;
            current -= info.amount;
            LastDamage = info;
            OnDamaged?.Invoke(info);
            if (current <= 0f)
            {
                current = 0f;
                OnDied?.Invoke(info);
            }
        }

        public void Kill(DamageInfo info)
        {
            if (IsDead) return;
            info.amount = current + 1f;
            bool wasInvulnerable = invulnerable;
            invulnerable = false;
            float mult = damageMultiplier;
            damageMultiplier = 1f;
            TakeDamage(info);
            invulnerable = wasInvulnerable;
            damageMultiplier = mult;
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            current = Mathf.Min(maxHealth, current + amount);
        }

        public void ResetHealth(float value)
        {
            current = Mathf.Clamp(value, 1f, maxHealth);
        }
    }
}
