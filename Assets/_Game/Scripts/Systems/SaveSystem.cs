using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ExitClause.Systems
{
    /// <summary>
    /// Writes GameState to persistentDataPath/saves/slotN.json and reads it back.
    /// INSECURE BY DESIGN: plain, pretty-printed JSON with no encryption, signature or schema
    /// validation. Any value can be edited in a text editor and is loaded as-is.
    /// </summary>
    public static class SaveSystem
    {
        public const int AutosaveSlot = 0;
        public const int SlotCount = 4;

        public static string SaveFolder => Path.Combine(Application.persistentDataPath, "saves");
        public static string SlotPath(int slot) => Path.Combine(SaveFolder, $"slot{slot}.json");

        public static bool Save(int slot, string label = null)
        {
            try
            {
                var state = GameSession.Instance.State;
                state.savedAtUtc = DateTime.UtcNow.ToString("u");
                state.saveName = label ?? (slot == AutosaveSlot ? "Autosave" : $"Slot {slot}");
                Player.PlayerCharacter.Instance?.WriteToState();
                Directory.CreateDirectory(SaveFolder);
                File.WriteAllText(SlotPath(slot), JsonUtility.ToJson(state, true));
                Debug.Log($"[SaveSystem] Saved slot {slot} to {SlotPath(slot)}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("[SaveSystem] Save failed: " + e);
                return false;
            }
        }

        public static bool Exists(int slot) => File.Exists(SlotPath(slot));

        public static GameState Read(int slot)
        {
            if (!Exists(slot)) return null;
            try { return JsonUtility.FromJson<GameState>(File.ReadAllText(SlotPath(slot))); }
            catch (Exception e) { Debug.LogError("[SaveSystem] Corrupt save: " + e.Message); return null; }
        }

        /// <summary>Loads a slot into the session and travels to the saved scene.</summary>
        public static bool LoadAndContinue(int slot)
        {
            var state = Read(slot);
            if (state == null) return false;
            GameSession.Instance.ReplaceState(state);
            SceneLoader.Instance.Load(state.progress.sceneName, state.progress.spawnPointId, true);
            return true;
        }

        public static int MostRecentSlot()
        {
            int best = -1;
            DateTime bestTime = DateTime.MinValue;
            for (int i = 0; i < SlotCount; i++)
            {
                if (!Exists(i)) continue;
                var t = File.GetLastWriteTimeUtc(SlotPath(i));
                if (t > bestTime) { bestTime = t; best = i; }
            }
            return best;
        }

        public static List<string> DescribeSlots()
        {
            var list = new List<string>();
            for (int i = 0; i < SlotCount; i++)
            {
                var s = Read(i);
                list.Add(s == null ? $"{(i == 0 ? "Autosave" : "Slot " + i)}: empty"
                    : $"{s.saveName}: {s.progress.sceneName}  ${s.economy.cash}  {s.savedAtUtc}");
            }
            return list;
        }
    }
}
