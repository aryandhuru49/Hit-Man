using ExitClause.Systems;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Player
{
    /// <summary>Finds the nearest usable Interactable in front of the player, shows its prompt and uses it on E.</summary>
    public class PlayerInteractor : MonoBehaviour
    {
        public Interactable Current { get; private set; }
        PlayerCharacter m_player;

        void Awake() { m_player = GetComponent<PlayerCharacter>(); }

        void Update()
        {
            Current = null;
            bool inCar = m_player.CurrentVehicle != null;
            if (m_player.Stealth.IsHidden)
            {
                UIRoot.Instance?.HUD?.SetPrompt("Leave hiding spot");
                return;
            }
            if (!m_player.InputEnabled || GameInput.GameplayBlocked || m_player.Health.IsDead)
            {
                UIRoot.Instance?.HUD?.SetPrompt(null);
                return;
            }

            if (inCar)
            {
                UIRoot.Instance?.HUD?.SetPrompt(m_player.CurrentVehicle.lockedForPlayer ? null : "Exit vehicle");
                if (GameInput.Pressed(GameInput.Interact) && !m_player.CurrentVehicle.lockedForPlayer) m_player.ExitVehicle();
                return;
            }

            float best = float.MaxValue;
            Vector3 pos = transform.position + Vector3.up;
            foreach (var it in Interactable.All)
            {
                if (it == null || !it.CanInteract(m_player)) continue;
                Vector3 to = it.FocusPosition - pos;
                float d = to.magnitude;
                if (d > it.range) continue;
                float facing = Vector3.Dot(transform.forward, new Vector3(to.x, 0, to.z).normalized);
                float score = d - facing * 0.75f;
                if (score < best) { best = score; Current = it; }
            }

            UIRoot.Instance?.HUD?.SetPrompt(Current != null ? Current.Prompt : null);
            if (Current != null && GameInput.Pressed(GameInput.Interact)) Current.Interact(m_player);
        }
    }
}
