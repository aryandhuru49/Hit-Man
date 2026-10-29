using ExitClause.Player;
using ExitClause.Systems;
using ExitClause.UI;

namespace ExitClause.Story
{
    /// <summary>The map on the safehouse wall; opens the Act 2 destination picker.</summary>
    public class TravelMap : Interactable
    {
        public override string Prompt => "Plan next move (map)";

        public override void Interact(PlayerCharacter player) => UIRoot.Instance.Travel.Open();
    }
}
