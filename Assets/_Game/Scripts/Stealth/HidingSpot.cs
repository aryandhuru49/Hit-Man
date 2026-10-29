using ExitClause.Player;
using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Stealth
{
    /// <summary>Wardrobe, dumpster or crate. While inside, the player is invisible to perception.</summary>
    public class HidingSpot : Interactable
    {
        public Transform exitPoint;
        bool m_occupied;

        void Reset() { prompt = "Hide"; }

        public override string Prompt => m_occupied ? "Leave hiding spot" : "Hide";

        public override bool CanInteract(PlayerCharacter player) => base.CanInteract(player) && (m_occupied || !player.Combat.IsAiming);

        public override void Interact(PlayerCharacter player)
        {
            if (m_occupied) return;
            m_occupied = true;
            m_enteredFrame = Time.frameCount;
            player.Stealth.EnterHidingSpot(this);
        }

        int m_enteredFrame;

        void Update()
        {
            if (!m_occupied || Time.frameCount == m_enteredFrame) return;
            var p = PlayerCharacter.Instance;
            if (p == null) return;
            if (!GameInput.GameplayBlocked && GameInput.Interact.WasPressedThisFrame())
            {
                m_occupied = false;
                Vector3 exit = exitPoint != null ? exitPoint.position : transform.position + transform.forward * 1.2f;
                p.Stealth.ExitHidingSpot(exit);
            }
        }
    }
}
