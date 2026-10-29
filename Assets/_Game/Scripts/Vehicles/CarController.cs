using ExitClause.Combat;
using ExitClause.Player;
using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Vehicles
{
    /// <summary>
    /// Arcade car physics: four raycast suspension points, drive force, grip that kills sideways
    /// slide (less with the handbrake for drifts), speed-scaled steering and auto-righting.
    /// Driven either by the player or by an AIDriver through SetInput.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(Health))]
    public class CarController : MonoBehaviour
    {
        [Header("Layout")]
        public Vector3 halfExtents = new Vector3(0.9f, 0.5f, 2.1f);
        public float suspensionRest = 0.55f;
        public float springStrength = 32000f;
        public float springDamper = 3200f;
        public Vector3 seatOffset = new Vector3(-0.4f, 0.3f, 0.1f);
        public Transform[] wheelVisuals;

        [Header("Driving")]
        public float acceleration = 14f;
        public float reverseAcceleration = 7f;
        public float maxSpeed = 48f;
        public float brakeStrength = 22f;
        public float turnRate = 95f;
        [Range(0, 1)] public float grip = 0.92f;
        [Range(0, 1)] public float handbrakeGrip = 0.35f;
        public float downforce = 4f;

        [Header("State")]
        public bool lockedForPlayer;
        public bool engineOn = true;

        public float Speed => m_rb.linearVelocity.magnitude;
        public float ForwardSpeed => Vector3.Dot(m_rb.linearVelocity, transform.forward);
        public bool IsPlayerDriven { get; private set; }
        public bool IsWrecked => m_health.IsDead;
        public Health Health => m_health;

        Rigidbody m_rb;
        Health m_health;
        float m_throttle, m_steer, m_brake;
        bool m_handbrake;
        int m_groundedWheels;
        float m_upsideDownTime;

        void Awake()
        {
            m_rb = GetComponent<Rigidbody>();
            m_health = GetComponent<Health>();
            m_rb.mass = 1200f;
            m_rb.interpolation = RigidbodyInterpolation.Interpolate;
            m_rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            m_rb.centerOfMass = new Vector3(0, -0.3f, 0.1f);
            m_rb.linearDamping = 0.05f;
            m_rb.angularDamping = 2f;
            gameObject.layer = Layers.Vehicle;
            m_health.OnDied += OnWrecked;
        }

        public void SetPlayerDriver(bool driving)
        {
            IsPlayerDriven = driving;
            if (!driving) SetInput(0, 0, 1, true);
        }

        /// <summary>throttle -1..1, steer -1..1, brake 0..1.</summary>
        public void SetInput(float throttle, float steer, float brake, bool handbrake)
        {
            m_throttle = Mathf.Clamp(throttle, -1f, 1f);
            m_steer = Mathf.Clamp(steer, -1f, 1f);
            m_brake = Mathf.Clamp01(brake);
            m_handbrake = handbrake;
        }

        void Update()
        {
            if (!IsPlayerDriven) return;
            var p = PlayerCharacter.Instance;
            if (p == null || !p.InputEnabled || GameInput.GameplayBlocked) { SetInput(0, 0, 0.3f, false); return; }
            Vector2 mv = GameInput.Vector(GameInput.Move);
            float fwd = ForwardSpeed;
            float throttle = mv.y;
            float brake = 0f;
            // Holding back while rolling forward brakes first, then reverses.
            if (mv.y < -0.1f && fwd > 1f) { brake = -mv.y; throttle = 0f; }
            SetInput(throttle, mv.x, brake, GameInput.Held(GameInput.Handbrake));
        }

        void FixedUpdate()
        {
            if (IsWrecked || !engineOn) { m_throttle = 0; }
            m_groundedWheels = 0;
            Vector3[] corners =
            {
                new Vector3(-halfExtents.x, -halfExtents.y, halfExtents.z), new Vector3(halfExtents.x, -halfExtents.y, halfExtents.z),
                new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z), new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z)
            };
            for (int i = 0; i < 4; i++)
            {
                Vector3 origin = transform.TransformPoint(corners[i]);
                if (Physics.Raycast(origin, -transform.up, out var hit, suspensionRest, ~(Layers.VehicleMask | Layers.PlayerMask | Layers.CharacterMask), QueryTriggerInteraction.Ignore))
                {
                    m_groundedWheels++;
                    float compression = 1f - hit.distance / suspensionRest;
                    float velUp = Vector3.Dot(m_rb.GetPointVelocity(origin), transform.up);
                    float force = compression * springStrength - velUp * springDamper;
                    m_rb.AddForceAtPosition(transform.up * Mathf.Max(0, force) * 0.25f, origin);
                }
                if (wheelVisuals != null && i < wheelVisuals.Length && wheelVisuals[i] != null)
                {
                    wheelVisuals[i].Rotate(Vector3.right, ForwardSpeed * Time.fixedDeltaTime * 57f / 0.35f, Space.Self);
                }
            }

            bool grounded = m_groundedWheels >= 2;
            if (grounded)
            {
                float fwd = ForwardSpeed;
                if (m_throttle > 0f && fwd < maxSpeed) m_rb.AddForce(transform.forward * m_throttle * acceleration, ForceMode.Acceleration);
                else if (m_throttle < 0f && fwd > -maxSpeed * 0.3f) m_rb.AddForce(transform.forward * m_throttle * reverseAcceleration, ForceMode.Acceleration);
                if (m_brake > 0f) m_rb.AddForce(-m_rb.linearVelocity.normalized * m_brake * brakeStrength, ForceMode.Acceleration);
                if (Mathf.Approximately(m_throttle, 0f)) m_rb.AddForce(-transform.forward * fwd * 0.4f, ForceMode.Acceleration);

                // Grip: remove part of the sideways velocity each step.
                Vector3 right = transform.right;
                float side = Vector3.Dot(m_rb.linearVelocity, right);
                float g = m_handbrake ? handbrakeGrip : grip;
                m_rb.AddForce(-right * side * g / Time.fixedDeltaTime * 0.1f, ForceMode.Acceleration);
                if (m_handbrake) m_rb.AddForce(-m_rb.linearVelocity * 0.6f, ForceMode.Acceleration);

                // Steering scales with speed and flips in reverse.
                float speedFactor = Mathf.Clamp01(Mathf.Abs(fwd) / 8f);
                float yaw = m_steer * turnRate * speedFactor * Mathf.Sign(fwd == 0 ? 1 : fwd) * (m_handbrake ? 1.4f : 1f);
                Vector3 av = m_rb.angularVelocity;
                av.y = Mathf.Lerp(av.y, yaw * Mathf.Deg2Rad, 0.2f);
                m_rb.angularVelocity = av;

                m_rb.AddForce(-transform.up * downforce * Speed * 0.1f, ForceMode.Acceleration);
            }

            if (Vector3.Dot(transform.up, Vector3.up) < 0.2f && Speed < 2f)
            {
                m_upsideDownTime += Time.fixedDeltaTime;
                if (m_upsideDownTime > 2.5f)
                {
                    m_upsideDownTime = 0f;
                    transform.SetPositionAndRotation(transform.position + Vector3.up * 1.5f, Quaternion.Euler(0, transform.eulerAngles.y, 0));
                    m_rb.linearVelocity = Vector3.zero;
                    m_rb.angularVelocity = Vector3.zero;
                }
            }
            else m_upsideDownTime = 0f;

            if (Speed > 3f) NoiseSystem.Emit(transform.position, 10f, NoiseType.Vehicle, gameObject);
        }

        void OnCollisionEnter(Collision c)
        {
            float impact = c.relativeVelocity.magnitude;
            var other = c.collider.GetComponentInParent<Health>();
            if (other != null && other != m_health && impact > 6f && c.collider.gameObject.layer != Layers.Vehicle)
                other.TakeDamage(new DamageInfo(impact * 6f, DamageType.Vehicle, gameObject, c.GetContact(0).point, m_rb.linearVelocity.normalized));
            if (impact > 9f)
                m_health.TakeDamage(new DamageInfo((impact - 9f) * 1.5f, DamageType.Vehicle, c.gameObject, c.GetContact(0).point, -c.relativeVelocity.normalized));
        }

        void OnWrecked(DamageInfo info)
        {
            engineOn = false;
            foreach (var r in GetComponentsInChildren<Renderer>()) r.material.color = new Color(0.1f, 0.1f, 0.1f);
            NoiseSystem.Emit(transform.position, 40f, NoiseType.Gunshot, gameObject);
            foreach (var h in FindObjectsByType<Health>(FindObjectsInactive.Exclude))
            {
                if (h == m_health) continue;
                float d = Vector3.Distance(h.transform.position, transform.position);
                if (d < 5f) h.TakeDamage(new DamageInfo(60f * (1f - d / 5f), DamageType.Explosion, gameObject, transform.position, (h.transform.position - transform.position).normalized));
            }
            if (IsPlayerDriven) PlayerCharacter.Instance?.ExitVehicle();
        }
    }
}
