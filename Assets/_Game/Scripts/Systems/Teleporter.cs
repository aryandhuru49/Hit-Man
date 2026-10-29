using System.Collections;
using ExitClause.Player;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Systems
{
    /// <summary>Lift or stairwell that moves the player to another floor behind a quick fade.</summary>
    public class Teleporter : Interactable
    {
        public Transform destination;
        public string label = "Take the lift";
        public string objectiveOnUse;

        public override string Prompt => label;

        public override void Interact(PlayerCharacter player) => StartCoroutine(Go(player));

        IEnumerator Go(PlayerCharacter player)
        {
            var fader = UIRoot.Instance.Fader;
            GameInput.PushBlock();
            yield return fader.FadeTo(1f, 0.4f);
            player.Teleport(destination.position, destination.rotation);
            if (!string.IsNullOrEmpty(objectiveOnUse)) MissionManager.Instance.Progress(objectiveOnUse);
            yield return new WaitForSecondsRealtime(0.3f);
            yield return fader.FadeTo(0f, 0.4f);
            GameInput.PopBlock();
        }
    }
}
