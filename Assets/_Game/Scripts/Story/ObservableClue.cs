using System.Collections.Generic;
using ExitClause.CameraSystem;
using ExitClause.Player;
using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>
    /// Something the player can study and note in the Suspicion Journal: a watcher, a parked car,
    /// resealed mail. Must be on screen to observe. With distinctZones, each sighting has to be in a
    /// different ZoneVolume (the market tail seen in two aisles and the parking lot).
    /// </summary>
    public class ObservableClue : Interactable
    {
        public ClueDefinition clue;
        public bool distinctZones;
        [Tooltip("Only observable on these in-game days (empty = any day).")]
        public int[] days;

        public string CurrentZone { get; set; } = "";
        readonly HashSet<string> m_zonesSeen = new HashSet<string>();

        void Reset() { range = 15f; }

        public override string Prompt => clue != null ? "Observe: " + clue.title : "Observe";

        public override bool CanInteract(PlayerCharacter player)
        {
            if (!base.CanInteract(player) || clue == null || JournalManager.Instance.IsLogged(clue.id)) return false;
            if (days != null && days.Length > 0 && System.Array.IndexOf(days, GameSession.Instance.State.progress.day) < 0) return false;
            if (distinctZones && m_zonesSeen.Contains(CurrentZone)) return false;
            var rig = GameCameraRig.Instance;
            if (rig == null) return true;
            Vector3 vp = rig.MainCamera.WorldToViewportPoint(FocusPosition);
            return vp.z > 0 && vp.x > 0.2f && vp.x < 0.8f && vp.y > 0.15f && vp.y < 0.85f;
        }

        public override void Interact(PlayerCharacter player)
        {
            if (distinctZones) m_zonesSeen.Add(CurrentZone);
            JournalManager.Instance.Observe(clue);
        }
    }
}
