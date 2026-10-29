using ExitClause.Systems;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>Lives in the Ending scene: reads the ending the player chose in Zurich and shows the epilogue and credits.</summary>
    public class EndingController : MonoBehaviour
    {
        void Start()
        {
            if (PauseController.Instance != null) PauseController.Instance.Allowed = false;
            GameInput.ForceCursorVisible = true;
            GameInput.RefreshCursor();
            UIRoot.Instance.HUD.SetVisible(false);
            var id = GameSession.Instance.Story.GetChoice(StoryKeys.ChoiceEnding);
            var ending = GameDatabase.Instance.GetEnding(id) ?? GameDatabase.Instance.GetEnding(EndingResolver.Disappear);
            if (ending == null) { Debug.LogError("No endings in GameDatabase."); return; }
            GameSession.Instance.State.progress.completedMissions.Add("game_complete");
            SaveSystem.Save(SaveSystem.AutosaveSlot, "Completed: " + ending.title);
            UIRoot.Instance.Ending.Show(ending.title, EndingResolver.BuildEpilogue(ending));
        }
    }
}
