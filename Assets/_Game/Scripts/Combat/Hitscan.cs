using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Combat
{
    /// <summary>Instant raycast shots with tracer, damage and gunshot noise. Used by every gun in the game.</summary>
    public static class Hitscan
    {
        public static readonly int ShootMask = ~((1 << 2) | (1 << Layers.Trigger));

        public static bool Fire(Vector3 origin, Vector3 direction, WeaponDefinition weapon, GameObject shooter, float damageScale = 1f)
        {
            var dir = direction.normalized;
            Vector3 end = origin + dir * weapon.range;
            bool hitSomething = false;

            var hits = Physics.RaycastAll(origin, dir, weapon.range, ShootMask, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                if (shooter != null && hit.collider.transform.IsChildOf(shooter.transform)) continue;
                end = hit.point;
                hitSomething = true;
                var health = hit.collider.GetComponentInParent<Health>();
                if (health != null)
                    health.TakeDamage(new DamageInfo(weapon.damage * damageScale, DamageType.Bullet, shooter, hit.point, dir));
                var rb = hit.rigidbody;
                if (rb != null && !rb.isKinematic) rb.AddForceAtPosition(dir * 4f, hit.point, ForceMode.Impulse);
                NoiseSystem.Emit(hit.point, 6f, NoiseType.Impact, shooter);
                break;
            }

            TracerEffect.Spawn(origin, end);
            float noise = weapon.suppressed ? weapon.noiseRadius * 0.15f : weapon.noiseRadius;
            NoiseSystem.Emit(origin, noise, NoiseType.Gunshot, shooter);
            AudioService.Instance?.PlayAt(weapon.fireSound, origin);
            return hitSomething;
        }

        public static Vector3 ApplySpread(Vector3 dir, float degrees)
        {
            if (degrees <= 0f) return dir;
            var rot = Quaternion.Euler(Random.Range(-degrees, degrees) * 0.5f, Random.Range(-degrees, degrees) * 0.5f, 0f);
            return Quaternion.LookRotation(dir) * rot * Vector3.forward;
        }
    }
}
