using ExitClause.Player;
using ExitClause.Systems;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Hacking
{
    /// <summary>Something in the world that opens the hacking minigames for a device (a recovered phone, a relay box, a laptop).</summary>
    public class HackingTerminal : Interactable
    {
        public DeviceDefinition device;

        void Reset() { prompt = "Hack device"; }

        public override string Prompt => device != null ? "Hack: " + device.deviceLabel : "Hack";

        public override bool CanInteract(PlayerCharacter player) => base.CanInteract(player) && device != null;

        public override void Interact(PlayerCharacter player) => UIRoot.Instance.Hacking.Open(device);
    }
}
