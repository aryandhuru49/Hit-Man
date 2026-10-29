using ExitClause.Player;
using ExitClause.Systems;
using ExitClause.UI;

namespace ExitClause.Stealth
{
    /// <summary>A uniform in a locker or laundry cart. Wearing it fools agents that accept that disguise id.</summary>
    public class DisguisePickup : Interactable
    {
        public string disguiseId = "guard";
        public string objectiveOnUse;

        public override string Prompt => "Wear " + disguiseId.Replace('_', ' ') + " outfit";

        public override bool CanInteract(PlayerCharacter player) => base.CanInteract(player) && player.Stealth.DisguiseId != disguiseId;

        public override void Interact(PlayerCharacter player)
        {
            player.Stealth.SetDisguise(disguiseId);
            UIRoot.Instance.HUD.Notify("Disguise: " + disguiseId.Replace('_', ' ') + ". Don't run or draw weapons near them.");
            if (!string.IsNullOrEmpty(objectiveOnUse)) MissionManager.Instance.Progress(objectiveOnUse);
        }
    }
}
