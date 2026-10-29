using UnityEngine;

namespace ExitClause.Combat
{
    public enum DamageType { Bullet, Melee, Takedown, Explosion, Vehicle, Fall }

    /// <summary>Everything a Health component needs to know about one hit.</summary>
    public struct DamageInfo
    {
        public float amount;
        public DamageType type;
        public GameObject source;
        public Vector3 point;
        public Vector3 direction;

        public DamageInfo(float amount, DamageType type, GameObject source, Vector3 point, Vector3 direction)
        {
            this.amount = amount;
            this.type = type;
            this.source = source;
            this.point = point;
            this.direction = direction;
        }
    }
}
