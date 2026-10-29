using System;
using System.Collections.Generic;

namespace ExitClause.Systems
{
    /// <summary>
    /// The entire mutable game state as plain serializable fields. This is the single object the
    /// save system writes to disk. INSECURE BY DESIGN: every value is a plain field in memory
    /// (easy to find with Cheat Engine) and is serialized to unsigned JSON.
    /// </summary>
    [Serializable]
    public class GameState
    {
        public int version = 1;
        public string saveName = "New Game";
        public string savedAtUtc;
        public ProgressState progress = new ProgressState();
        public PlayerState player = new PlayerState();
        public EconomyState economy = new EconomyState();
        public InventoryState inventory = new InventoryState();
        public JournalState journal = new JournalState();
        public StoryState story = new StoryState();
    }

    [Serializable]
    public class ProgressState
    {
        public string sceneName = "Prologue_Prague";
        public string spawnPointId = "";
        public string missionId = "";
        public int objectiveIndex;
        public List<string> completedObjectives = new List<string>();
        public List<string> completedMissions = new List<string>();
        public int day = 1;
        public float playTimeSeconds;
    }

    [Serializable]
    public class PlayerState
    {
        public float health = 100f;
        public float maxHealth = 100f;
        public float rage;
        public string disguiseId = "";
        public string equippedWeaponId = "pistol";
        public List<AmmoEntry> ammo = new List<AmmoEntry>();
    }

    [Serializable]
    public class AmmoEntry
    {
        public string weaponId;
        public int inMagazine;
        public int reserve;
    }

    [Serializable]
    public class EconomyState
    {
        public int cash = 500;
        public float pensionBalance;
        public bool ownsMansion;
        public List<string> purchaseLog = new List<string>();
    }

    [Serializable]
    public class InventoryState
    {
        public List<ItemStack> items = new List<ItemStack>();
    }

    [Serializable]
    public class ItemStack
    {
        public string itemId;
        public int count;
    }

    [Serializable]
    public class JournalState
    {
        public List<string> loggedClues = new List<string>();
        public List<ClueObservationProgress> observationProgress = new List<ClueObservationProgress>();
        public List<BoardPin> pins = new List<BoardPin>();
    }

    [Serializable]
    public class ClueObservationProgress
    {
        public string clueId;
        public int count;
        public int lastDayObserved = -1;
    }

    [Serializable]
    public class BoardPin
    {
        public string clueId;
        public string suspectId;
    }
}
