using UnityEngine;

namespace ExitClause.Player
{
    /// <summary>
    /// Feeds movement state into an Animator once you swap the capsule for a Mixamo character.
    /// Expected Animator parameters: Speed (float), Crouch (bool), Aim (bool), Cover (bool), Grounded (bool).
    /// Missing parameters are ignored, and with no Animator this does nothing.
    /// </summary>
    public class CharacterAnimatorBridge : MonoBehaviour
    {
        public Animator animator;
        PlayerCharacter m_player;
        static readonly int Speed = Animator.StringToHash("Speed");
        static readonly int Crouch = Animator.StringToHash("Crouch");
        static readonly int Aim = Animator.StringToHash("Aim");
        static readonly int Cover = Animator.StringToHash("Cover");
        static readonly int Grounded = Animator.StringToHash("Grounded");

        void Awake()
        {
            m_player = GetComponent<PlayerCharacter>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        void Update()
        {
            if (animator == null || animator.runtimeAnimatorController == null || m_player == null) return;
            var m = m_player.Motor;
            Set(Speed, m.PlanarSpeed);
            Set(Crouch, m.IsCrouching);
            Set(Aim, m_player.Combat.IsAiming);
            Set(Cover, m.IsInCover);
            Set(Grounded, m.IsGrounded);
        }

        void Set(int hash, float v) { foreach (var p in animator.parameters) if (p.nameHash == hash) { animator.SetFloat(hash, v); return; } }
        void Set(int hash, bool v) { foreach (var p in animator.parameters) if (p.nameHash == hash) { animator.SetBool(hash, v); return; } }
    }
}
