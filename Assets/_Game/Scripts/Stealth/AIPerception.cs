using System;
using ExitClause.Player;
using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Stealth
{
    /// <summary>
    /// Vision cone, hearing and a suspicion meter for one NPC, drone or camera.
    /// Suspicion rises while the player is visible (faster when close, slower when crouched,
    /// much slower when wearing a matching disguise) and decays when they are out of sight.
    /// </summary>
    public class AIPerception : MonoBehaviour, INoiseListener
    {
        public Transform eyes;
        public float viewDistance = 18f;
        [Range(10, 360)] public float viewAngle = 110f;
        [Tooltip("Within this range the NPC notices the player in any direction.")]
        public float closeAwareness = 1.6f;
        [Tooltip("Seconds of full visibility at mid range to go from calm to fully alerted.")]
        public float detectTime = 1.6f;
        public float decayPerSecond = 0.2f;
        public float hearingMultiplier = 1f;
        [Tooltip("Disguise id this NPC accepts as one of their own (e.g. 'guard', 'bank_staff').")]
        public string acceptsDisguise = "";

        public float Suspicion { get; set; }
        public bool CanSeePlayer { get; private set; }
        public Vector3 LastKnownPlayerPosition { get; private set; }
        public float LastSeenTime { get; private set; } = -100f;
        public bool Blind;
        /// <summary>Raised by alarms and by the Act 2 evidence trail.</summary>
        public float SuspicionMultiplier = 1f;
        /// <summary>Set while in combat so the meter does not decay.</summary>
        public bool HoldSuspicion;

        public event Action<Vector3, float, NoiseType, GameObject> OnNoise;

        public Vector3 EyePosition => eyes != null ? eyes.position : transform.position + Vector3.up * 1.6f;
        public Vector3 ListenerPosition => transform.position;
        public Vector3 Forward => eyes != null ? eyes.forward : transform.forward;

        void OnEnable() => NoiseSystem.Register(this);
        void OnDisable() => NoiseSystem.Unregister(this);

        void Update()
        {
            CanSeePlayer = false;
            var p = PlayerCharacter.Instance;
            if (Blind || p == null || p.Health.IsDead) { Decay(); return; }

            Vector3 target = p.CurrentVehicle != null ? p.CurrentVehicle.transform.position + Vector3.up : p.transform.position + Vector3.up * 1.1f;
            float visibility = p.CurrentVehicle != null ? 1f : p.Stealth.Visibility;
            if (visibility > 0f && IsInSight(target))
            {
                CanSeePlayer = true;
                LastKnownPlayerPosition = p.transform.position;
                LastSeenTime = Time.time;
                float dist = Vector3.Distance(EyePosition, target);
                float distanceFactor = Mathf.Lerp(2.5f, 0.4f, dist / Mathf.Max(1f, viewDistance));
                float rate = visibility * distanceFactor * DisguiseFactor(p, dist) * SuspicionMultiplier / Mathf.Max(0.1f, detectTime);
                Suspicion = Mathf.Clamp01(Suspicion + rate * Time.deltaTime);
            }
            else Decay();
        }

        void Decay()
        {
            if (!HoldSuspicion) Suspicion = Mathf.Max(0f, Suspicion - decayPerSecond * Time.deltaTime);
        }

        float DisguiseFactor(PlayerCharacter p, float dist)
        {
            if (string.IsNullOrEmpty(acceptsDisguise) || p.Stealth.DisguiseId != acceptsDisguise) return 1f;
            if (p.Combat.IsAiming || p.Motor.IsSprinting) return 1f;
            return dist < 2.5f ? 0.45f : 0.06f;
        }

        public bool IsInSight(Vector3 target)
        {
            Vector3 eye = EyePosition;
            Vector3 to = target - eye;
            float dist = to.magnitude;
            if (dist > viewDistance) return false;
            if (dist > closeAwareness && Vector3.Angle(Forward, to) > viewAngle * 0.5f) return false;
            if (Physics.Linecast(eye, target, out var hit, Layers.EnvironmentMask, QueryTriggerInteraction.Ignore))
            {
                var p = PlayerCharacter.Instance;
                bool hitPlayerCar = p != null && p.CurrentVehicle != null && hit.collider.transform.IsChildOf(p.CurrentVehicle.transform);
                if (!hitPlayerCar && !hit.collider.transform.IsChildOf(transform)) return false;
            }
            return true;
        }

        public void HearNoise(Vector3 position, float loudness, NoiseType type, GameObject source)
        {
            if (Blind) return;
            if (source != null && source.transform.IsChildOf(transform)) return;
            OnNoise?.Invoke(position, loudness * hearingMultiplier, type, source);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 e = EyePosition;
            Vector3 l = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * Forward;
            Vector3 r = Quaternion.Euler(0, viewAngle * 0.5f, 0) * Forward;
            Gizmos.DrawRay(e, l * viewDistance);
            Gizmos.DrawRay(e, r * viewDistance);
        }
    }
}
