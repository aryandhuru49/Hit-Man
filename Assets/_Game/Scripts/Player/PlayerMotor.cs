using System.Collections;
using ExitClause.CameraSystem;
using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Player
{
    /// <summary>
    /// Third person movement on a CharacterController: camera-relative jog, sprint, crouch,
    /// a small jump, vaulting over low ledges and sticking to cover along walls.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("Speeds (m/s)")]
        public float jogSpeed = 4.2f;
        public float sprintSpeed = 6.8f;
        public float crouchSpeed = 1.9f;
        public float aimSpeed = 2.4f;
        public float coverSpeed = 1.6f;
        public float turnSpeed = 720f;
        public float gravity = -20f;
        public float jumpHeight = 0.9f;

        [Header("Crouch")]
        public float standHeight = 1.8f;
        public float crouchHeight = 1.1f;

        [Header("Vault")]
        public float maxVaultHeight = 1.5f;
        public float vaultCheckDistance = 1.1f;

        [Header("Cover")]
        public float coverCheckDistance = 1.3f;

        public bool IsCrouching { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsInCover { get; private set; }
        public bool IsLowCover { get; private set; }
        public bool IsGrounded => m_cc.isGrounded;
        public float PlanarSpeed { get; private set; }
        public bool IsBusy { get; private set; }

        /// <summary>Rage mode and other buffs scale movement speed.</summary>
        public float SpeedMultiplier = 1f;

        CharacterController m_cc;
        PlayerCharacter m_player;
        Vector3 m_velocity;
        Vector3 m_coverNormal;

        void Awake()
        {
            m_cc = GetComponent<CharacterController>();
            m_player = GetComponent<PlayerCharacter>();
            m_cc.height = standHeight;
            m_cc.center = Vector3.up * standHeight * 0.5f;
        }

        void Update()
        {
            if (m_player.CurrentVehicle != null || IsBusy || !m_cc.enabled) { PlanarSpeed = 0; return; }
            bool locked = m_player.ControlsLocked;
            Vector2 input = locked ? Vector2.zero : GameInput.Vector(GameInput.Move);
            var rig = GameCameraRig.Instance;
            bool aiming = rig != null && rig.CurrentMode == GameCameraRig.Mode.Aim;

            if (!locked && GameInput.Pressed(GameInput.Crouch)) SetCrouch(!IsCrouching);
            if (!locked && GameInput.Pressed(GameInput.Cover)) ToggleCover();

            if (!locked && GameInput.Pressed(GameInput.Jump) && m_cc.isGrounded && !IsInCover)
            {
                if (!TryVault()) m_velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            Vector3 move;
            if (IsInCover) move = CoverMove(input, aiming);
            else move = FreeMove(input, aiming, rig);

            if (m_cc.isGrounded && m_velocity.y < 0) m_velocity.y = -2f;
            m_velocity.y += gravity * Time.deltaTime;
            m_cc.Move((move + Vector3.up * m_velocity.y) * Time.deltaTime);
            PlanarSpeed = new Vector3(m_cc.velocity.x, 0, m_cc.velocity.z).magnitude;
        }

        Vector3 FreeMove(Vector2 input, bool aiming, GameCameraRig rig)
        {
            float yaw = rig != null ? rig.Yaw : transform.eulerAngles.y;
            Quaternion camYaw = Quaternion.Euler(0, yaw, 0);
            Vector3 dir = camYaw * new Vector3(input.x, 0, input.y);
            if (dir.sqrMagnitude > 1f) dir.Normalize();

            IsSprinting = GameInput.Held(GameInput.Sprint) && !aiming && dir.sqrMagnitude > 0.1f;
            if (IsSprinting && IsCrouching) SetCrouch(false);
            float speed = aiming ? aimSpeed : IsCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : jogSpeed;
            speed *= SpeedMultiplier;

            if (aiming)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, camYaw, turnSpeed * 2f * Time.deltaTime);
            else if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);
            return dir * speed;
        }

        Vector3 CoverMove(Vector2 input, bool aiming)
        {
            IsSprinting = false;
            // Leave cover by pulling away from the wall.
            var rig = GameCameraRig.Instance;
            float yaw = rig != null ? rig.Yaw : transform.eulerAngles.y;
            Vector3 world = Quaternion.Euler(0, yaw, 0) * new Vector3(input.x, 0, input.y);
            if (Vector3.Dot(world, m_coverNormal) > 0.7f) { ExitCover(); return Vector3.zero; }

            Vector3 along = Vector3.Cross(Vector3.up, m_coverNormal).normalized;
            float lateral = Vector3.Dot(world, along);
            Vector3 move = along * lateral * coverSpeed;

            // Stop at the edge of the wall.
            Vector3 probe = transform.position + Vector3.up * 0.8f + move.normalized * 0.4f;
            if (move.sqrMagnitude > 0.01f && !Physics.Raycast(probe, -m_coverNormal, coverCheckDistance, Layers.EnvironmentMask, QueryTriggerInteraction.Ignore))
                move = Vector3.zero;

            Quaternion face = aiming && rig != null ? Quaternion.Euler(0, rig.Yaw, 0) : Quaternion.LookRotation(m_coverNormal);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, face, turnSpeed * Time.deltaTime);
            // Peek: stand up from low cover while aiming.
            SetCrouchInternal(IsLowCover && !aiming);
            return move;
        }

        void ToggleCover()
        {
            if (IsInCover) { ExitCover(); return; }
            Vector3 origin = transform.position + Vector3.up * 0.8f;
            if (Physics.Raycast(origin, transform.forward, out var hit, coverCheckDistance, Layers.EnvironmentMask, QueryTriggerInteraction.Ignore)
                && Vector3.Angle(hit.normal, Vector3.up) > 60f)
            {
                IsInCover = true;
                m_coverNormal = new Vector3(hit.normal.x, 0, hit.normal.z).normalized;
                IsLowCover = !Physics.Raycast(transform.position + Vector3.up * 1.6f, -m_coverNormal, coverCheckDistance + 0.5f, Layers.EnvironmentMask, QueryTriggerInteraction.Ignore);
                Vector3 target = hit.point + m_coverNormal * (m_cc.radius + 0.05f);
                target.y = transform.position.y;
                m_cc.Move(target - transform.position);
            }
        }

        public void ExitCover()
        {
            IsInCover = false;
            SetCrouchInternal(IsCrouching);
        }

        public void SetCrouch(bool crouch)
        {
            if (!crouch && !CanStand()) return;
            IsCrouching = crouch;
            SetCrouchInternal(crouch);
        }

        void SetCrouchInternal(bool crouch)
        {
            if (!crouch && !CanStand()) crouch = true;
            float h = crouch ? crouchHeight : standHeight;
            m_cc.height = h;
            m_cc.center = Vector3.up * h * 0.5f;
        }

        bool CanStand()
        {
            return !Physics.SphereCast(transform.position + Vector3.up * crouchHeight * 0.5f, m_cc.radius * 0.9f, Vector3.up,
                out _, standHeight - crouchHeight * 0.5f, Layers.EnvironmentMask, QueryTriggerInteraction.Ignore);
        }

        bool TryVault()
        {
            Vector3 knee = transform.position + Vector3.up * 0.5f;
            if (!Physics.Raycast(knee, transform.forward, out var wall, vaultCheckDistance, Layers.EnvironmentMask, QueryTriggerInteraction.Ignore))
                return false;
            Vector3 top = wall.point + transform.forward * 0.35f + Vector3.up * (maxVaultHeight + 0.3f);
            if (!Physics.Raycast(top, Vector3.down, out var ledge, maxVaultHeight + 0.3f, Layers.EnvironmentMask, QueryTriggerInteraction.Ignore))
                return false;
            float height = ledge.point.y - transform.position.y;
            if (height < 0.3f || height > maxVaultHeight) return false;
            Vector3 landing = ledge.point + transform.forward * 0.4f;
            if (Physics.CheckCapsule(landing + Vector3.up * 0.5f, landing + Vector3.up * 1.4f, 0.3f, Layers.EnvironmentMask, QueryTriggerInteraction.Ignore))
                return false;
            StartCoroutine(VaultRoutine(landing));
            return true;
        }

        IEnumerator VaultRoutine(Vector3 landing)
        {
            IsBusy = true;
            m_cc.enabled = false;
            Vector3 start = transform.position;
            Vector3 mid = new Vector3((start.x + landing.x) * 0.5f, landing.y + 0.4f, (start.z + landing.z) * 0.5f);
            for (float t = 0; t < 1f; t += Time.deltaTime / 0.45f)
            {
                Vector3 a = Vector3.Lerp(start, mid, t);
                Vector3 b = Vector3.Lerp(mid, landing, t);
                transform.position = Vector3.Lerp(a, b, t);
                yield return null;
            }
            transform.position = landing;
            m_cc.enabled = true;
            m_velocity = Vector3.zero;
            IsBusy = false;
        }

        /// <summary>Freezes movement for scripted actions (takedowns).</summary>
        public IEnumerator Busy(float seconds)
        {
            IsBusy = true;
            yield return new WaitForSeconds(seconds);
            IsBusy = false;
        }
    }
}
