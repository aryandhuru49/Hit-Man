using ExitClause.Combat;
using ExitClause.Player;
using ExitClause.Vehicles;
using UnityEngine;

namespace ExitClause.AI
{
    /// <summary>
    /// Drives a CarController: follows a waypoint route, chases the player, or flees.
    /// Pursuers can have a gunman who shoots at the player's car when close.
    /// </summary>
    [RequireComponent(typeof(CarController))]
    public class AIDriver : MonoBehaviour
    {
        public enum DriveMode { Route, ChasePlayer, Flee, Idle }

        public DriveMode mode = DriveMode.Route;
        public PatrolRoute route;
        public float cruiseSpeed = 22f;
        public float waypointRadius = 7f;
        [Header("Gunman")]
        public WeaponDefinition gunmanWeapon;
        public float gunmanRange = 28f;

        CarController m_car;
        int m_index;
        int m_dir = 1;
        float m_stuckTime, m_reverseUntil, m_nextShot;

        void Awake() { m_car = GetComponent<CarController>(); }

        void FixedUpdate()
        {
            if (m_car.IsPlayerDriven || m_car.IsWrecked) return;
            Vector3 target;
            float desiredSpeed = cruiseSpeed;
            var player = PlayerCharacter.Instance;

            switch (mode)
            {
                case DriveMode.Route:
                    if (route == null || route.Count == 0) { m_car.SetInput(0, 0, 1, false); return; }
                    target = route.Get(m_index);
                    if (Vector3.Distance(Flat(transform.position), Flat(target)) < waypointRadius) m_index = route.Next(m_index, ref m_dir);
                    break;
                case DriveMode.ChasePlayer:
                    if (player == null) return;
                    Transform t = player.CurrentVehicle != null ? player.CurrentVehicle.transform : player.transform;
                    var rb = t.GetComponent<Rigidbody>();
                    target = t.position + (rb != null ? rb.linearVelocity * 0.6f : Vector3.zero);
                    desiredSpeed = cruiseSpeed * 1.3f;
                    TryShoot(player, t);
                    break;
                case DriveMode.Flee:
                    if (player == null) return;
                    target = transform.position + (transform.position - player.transform.position).normalized * 30f;
                    if (route != null && route.Count > 0)
                    {
                        target = route.Get(m_index);
                        if (Vector3.Distance(Flat(transform.position), Flat(target)) < waypointRadius) m_index = route.Next(m_index, ref m_dir);
                    }
                    desiredSpeed = cruiseSpeed * 1.2f;
                    break;
                default:
                    m_car.SetInput(0, 0, 1, false);
                    return;
            }

            Vector3 local = transform.InverseTransformPoint(target);
            float steer = Mathf.Clamp(local.x / Mathf.Max(4f, local.magnitude) * 2.2f, -1f, 1f);
            float speed = m_car.ForwardSpeed;
            float throttle = speed < desiredSpeed * (1f - Mathf.Abs(steer) * 0.5f) ? 1f : 0f;
            float brake = Mathf.Abs(steer) > 0.8f && speed > 15f ? 0.5f : 0f;

            // Unstick: reverse briefly when blocked.
            if (Mathf.Abs(speed) < 1f && throttle > 0f) m_stuckTime += Time.fixedDeltaTime; else m_stuckTime = 0f;
            if (m_stuckTime > 1.5f) { m_reverseUntil = Time.time + 1.2f; m_stuckTime = 0f; }
            if (Time.time < m_reverseUntil) { throttle = -1f; steer = -steer; brake = 0f; }

            m_car.SetInput(throttle, steer, brake, false);
        }

        void TryShoot(PlayerCharacter player, Transform target)
        {
            if (gunmanWeapon == null || Time.time < m_nextShot) return;
            float d = Vector3.Distance(transform.position, target.position);
            if (d > gunmanRange) return;
            m_nextShot = Time.time + 1f / gunmanWeapon.fireRate + Random.Range(0.2f, 0.8f);
            Vector3 origin = transform.position + Vector3.up * 1.6f + transform.forward;
            Vector3 aim = target.position + Vector3.up;
            if (Random.value > gunmanWeapon.aiAccuracy) aim += Random.insideUnitSphere * 2.5f;
            Hitscan.Fire(origin, aim - origin, gunmanWeapon, gameObject);
        }

        static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0, v.z);
    }
}
