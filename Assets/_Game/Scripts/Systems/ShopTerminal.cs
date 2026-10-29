using ExitClause.Player;
using ExitClause.UI;

namespace ExitClause.Systems
{
    /// <summary>A dealer or shop counter that opens the shop screen.</summary>
    public class ShopTerminal : Interactable
    {
        public string shopName = "Dealer";
        public string objectiveOnUse;

        public override string Prompt => "Shop: " + shopName;

        public override void Interact(PlayerCharacter player)
        {
            UIRoot.Instance.Shop.Open(shopName);
            if (!string.IsNullOrEmpty(objectiveOnUse)) MissionManager.Instance.Progress(objectiveOnUse);
        }
    }
}
