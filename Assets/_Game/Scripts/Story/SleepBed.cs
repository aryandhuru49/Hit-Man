using System.Collections;
using ExitClause.Player;
using ExitClause.Systems;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>The mansion bed: ends the day, autosaves and advances the Act 0 routine.</summary>
    public class SleepBed : Interactable
    {
        public string objectiveId = "sleep";
        public Transform wakePoint;
        public int maxDay = 4;

        public override string Prompt => "Sleep (end day " + GameSession.Instance.State.progress.day + ")";

        public override bool CanInteract(PlayerCharacter player) =>
            base.CanInteract(player) && GameSession.Instance.State.progress.day < maxDay &&
            MissionManager.Instance.IsObjectiveActive(objectiveId + "_" + GameSession.Instance.State.progress.day);

        public override void Interact(PlayerCharacter player) => StartCoroutine(Sleep(player));

        IEnumerator Sleep(PlayerCharacter player)
        {
            var ui = UIRoot.Instance;
            GameInput.PushBlock();
            yield return ui.Fader.FadeTo(1f, 0.8f);
            int endedDay = GameSession.Instance.State.progress.day;
            GameClock.Instance.AdvanceDay();
            if (wakePoint != null) player.Teleport(wakePoint.position, wakePoint.rotation);
            ui.HUD.ShowMissionTitle("Day " + GameSession.Instance.State.progress.day, "Riviera");
            // Objectives are named sleep_1, sleep_2... after the day that ends.
            MissionManager.Instance.Progress(objectiveId + "_" + endedDay);
            SaveSystem.Save(SaveSystem.AutosaveSlot);
            yield return new WaitForSecondsRealtime(0.6f);
            yield return ui.Fader.FadeTo(0f, 0.8f);
            GameInput.PopBlock();
        }
    }
}
