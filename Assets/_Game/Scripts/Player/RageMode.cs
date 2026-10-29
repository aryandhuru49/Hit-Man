using ExitClause.Combat;
using ExitClause.Stealth;
using ExitClause.Systems;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Player
{
    /// <summary>
    /// Rage mode (unlocked in the Interlude). Faster and stronger, takes less damage, but sloppier:
    /// worse accuracy, louder footsteps, and every kill leaves evidence. The evidence counter is read
    /// in Act 2 to spawn extra agency teams that tracked the player.
    /// </summary>
    public class RageMode : MonoBehaviour
    {
        public float activateThreshold = 50f;
        public float drainPerSecond = 7f;
        public float gainPerDamage = 0.6f;
        public float speedMultiplier = 1.35f;
        public float damageMultiplier = 1.6f;
        public float damageTakenMultiplier = 0.6f;
        public float spreadMultiplier = 2.2f;
        public float noiseMultiplier = 2f;

        public bool IsActive { get; private set; }
        public bool Unlocked => GameSession.Instance.Story.HasFlag(StoryKeys.RageUnlocked);

        /// <summary>INSECURE: plain float in GameState.</summary>
        public float Meter
        {
            get => GameSession.Instance.State.player.rage;
            set => GameSession.Instance.State.player.rage = Mathf.Clamp(value, 0f, 100f);
        }

        PlayerCharacter m_player;

        void Awake() { m_player = GetComponent<PlayerCharacter>(); }

        void Start()
        {
            m_player.Health.OnDamaged += d => { if (Unlocked && !IsActive) Meter += d.amount * gainPerDamage; };
        }

        void Update()
        {
            if (!Unlocked) return;
            if (!IsActive && Meter >= activateThreshold && GameInput.Pressed(GameInput.Rage) && !m_player.ControlsLocked) Activate();
            if (IsActive)
            {
                Meter -= drainPerSecond * Time.deltaTime;
                if (Meter <= 0f || m_player.Health.IsDead) Deactivate();
            }
        }

        public void Fill(float amount) => Meter += amount;

        public void Activate()
        {
            IsActive = true;
            GameSession.Instance.Story.AddCounter(StoryKeys.RageUses, 1);
            Apply(true);
            UIRoot.Instance.HUD.Notify("RAGE");
        }

        public void Deactivate()
        {
            IsActive = false;
            Apply(false);
        }

        void Apply(bool on)
        {
            m_player.Motor.SpeedMultiplier = on ? speedMultiplier : 1f;
            m_player.Combat.DamageMultiplier = on ? damageMultiplier : 1f;
            m_player.Combat.SpreadMultiplier = on ? spreadMultiplier : 1f;
            m_player.Stealth.NoiseMultiplier = on ? noiseMultiplier : 1f;
            m_player.Health.damageMultiplier = on ? damageTakenMultiplier : 1f;
        }

        /// <summary>Called by enemies when the player kills them; leaves evidence while raging.</summary>
        public void OnPlayerKill(Vector3 position)
        {
            if (!IsActive) return;
            EvidenceMarker.Spawn(position);
            GameSession.Instance.Story.AddCounter(StoryKeys.RageEvidence, 1);
        }
    }
}
