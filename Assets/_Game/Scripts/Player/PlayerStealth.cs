using ExitClause.AI;
using ExitClause.Stealth;
using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Player
{
    /// <summary>
    /// How noticeable the player is: footstep noise, crouch visibility, hiding spots and disguises.
    /// AI perception asks this component how visible the player currently is.
    /// </summary>
    public class PlayerStealth : MonoBehaviour
    {
        public float jogNoiseRadius = 5f;
        public float sprintNoiseRadius = 10f;
        public float crouchNoiseRadius = 1f;
        public float stepInterval = 0.45f;

        public HidingSpot CurrentHidingSpot { get; private set; }
        public bool IsHidden => CurrentHidingSpot != null;
        public string DisguiseId => GameSession.Instance.State.player.disguiseId;
        public bool IsDisguised => !string.IsNullOrEmpty(DisguiseId);

        /// <summary>Rage mode makes the player loud.</summary>
        public float NoiseMultiplier = 1f;

        /// <summary>Highest suspicion of any enemy this frame (0..1), used by the HUD.</summary>
        public float HighestSuspicion { get; private set; }
        public bool AnyEnemyInCombat { get; private set; }

        PlayerCharacter m_player;
        float m_stepTimer;

        void Awake() { m_player = GetComponent<PlayerCharacter>(); }

        /// <summary>0 = invisible, 1 = normal. Used to scale detection speed.</summary>
        public float Visibility
        {
            get
            {
                if (IsHidden) return 0f;
                var m = m_player.Motor;
                float v = 1f;
                if (m.IsCrouching) v *= 0.55f;
                if (m.IsInCover && m.IsLowCover) v *= 0.4f;
                if (m.IsSprinting) v *= 1.3f;
                return v;
            }
        }

        void Update()
        {
            var m = m_player.Motor;
            if (m_player.CurrentVehicle == null && m.IsGrounded && m.PlanarSpeed > 0.5f)
            {
                m_stepTimer -= Time.deltaTime;
                if (m_stepTimer <= 0f)
                {
                    m_stepTimer = stepInterval;
                    float r = m.IsCrouching ? crouchNoiseRadius : m.IsSprinting ? sprintNoiseRadius : jogNoiseRadius;
                    NoiseSystem.Emit(transform.position, r * NoiseMultiplier, NoiseType.Footstep, gameObject);
                }
            }

            float s = 0f; bool combat = false;
            foreach (var e in EnemyAI.All)
            {
                if (e == null || e.IsDead) continue;
                s = Mathf.Max(s, e.Perception != null ? e.Perception.Suspicion : 0f);
                combat |= e.State == EnemyState.Combat;
            }
            HighestSuspicion = s;
            AnyEnemyInCombat = combat;
        }

        public void EnterHidingSpot(HidingSpot spot)
        {
            CurrentHidingSpot = spot;
            m_player.InputEnabled = false;
            GetComponent<CharacterController>().enabled = false;
            if (m_player.bodyVisual != null) m_player.bodyVisual.SetActive(false);
        }

        public void ExitHidingSpot(Vector3 exitPosition)
        {
            CurrentHidingSpot = null;
            if (m_player.bodyVisual != null) m_player.bodyVisual.SetActive(true);
            m_player.InputEnabled = true;
            m_player.Teleport(exitPosition, transform.rotation);
        }

        public void SetDisguise(string disguiseId)
        {
            GameSession.Instance.State.player.disguiseId = disguiseId ?? "";
            var tint = m_player.bodyVisual != null ? m_player.bodyVisual.GetComponentInChildren<Renderer>() : null;
            if (tint != null) tint.material.color = string.IsNullOrEmpty(disguiseId) ? new Color(0.15f, 0.15f, 0.18f) : new Color(0.55f, 0.45f, 0.3f);
        }
    }
}
