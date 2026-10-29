using ExitClause.Player;
using ExitClause.UI;

namespace ExitClause.Systems
{
    /// <summary>The mansion's laptop with the agency pension portal.</summary>
    public class BankTerminal : Interactable
    {
        public string objectiveOnUse;

        public override string Prompt => "Check pension account";

        public override void Interact(PlayerCharacter player)
        {
            UIRoot.Instance.Bank.Open();
            if (!string.IsNullOrEmpty(objectiveOnUse)) MissionManager.Instance.Progress(objectiveOnUse);
        }
    }
}
