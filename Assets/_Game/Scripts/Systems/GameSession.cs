using System;
using UnityEngine;

namespace ExitClause.Systems
{
    /// <summary>
    /// Owns the live GameState for the current playthrough. Every other system reads and writes
    /// through GameSession.Instance.State. Lives on the persistent GameRoot.
    /// </summary>
    public class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }

        // INSECURE: one plain public object holding all money, health and story values.
        public GameState State = new GameState();

        public event Action OnStateReplaced;

        public StoryState Story => State.story;

        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            if (Time.timeScale > 0f) State.progress.playTimeSeconds += Time.unscaledDeltaTime;
        }

        public void NewGame()
        {
            State = new GameState();
            var db = GameDatabase.Instance;
            if (db != null)
            {
                foreach (var stack in db.startingItems)
                    State.inventory.items.Add(new ItemStack { itemId = stack.itemId, count = stack.count });
                foreach (var w in db.weapons)
                    State.player.ammo.Add(new AmmoEntry { weaponId = w.id, inMagazine = w.magazineSize, reserve = w.startingReserve });
            }
            OnStateReplaced?.Invoke();
        }

        public void ReplaceState(GameState loaded)
        {
            State = loaded ?? new GameState();
            OnStateReplaced?.Invoke();
        }
    }
}
