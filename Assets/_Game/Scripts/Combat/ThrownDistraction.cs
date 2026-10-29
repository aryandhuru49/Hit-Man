using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Combat
{
    /// <summary>
    /// Distraction gadget (a coin or noisemaker). Makes one loud noise where it lands, pulling
    /// guards away from their patrol to investigate.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ThrownDistraction : MonoBehaviour
    {
        public float noiseRadius = 14f;
        public float lifetime = 12f;
        bool m_triggered;

        public static void Throw(Vector3 origin, Vector3 velocity, GameObject thrower, float radius)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Distraction";
            go.transform.position = origin;
            go.transform.localScale = Vector3.one * 0.15f;
            go.layer = Layers.Props;
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 0.2f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.linearVelocity = velocity;
            go.AddComponent<ThrownDistraction>().noiseRadius = radius;
            foreach (var c in thrower.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(go.GetComponent<Collider>(), c);
        }

        void OnCollisionEnter(Collision c)
        {
            if (m_triggered) return;
            m_triggered = true;
            NoiseSystem.Emit(transform.position, noiseRadius, NoiseType.Distraction, gameObject);
            Destroy(gameObject, lifetime);
        }
    }
}
