using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ExitClause.Hacking
{
    /// <summary>
    /// Reads StreamingAssets/hacking/devices.json, the "server config" for device answers.
    /// INSECURE BY DESIGN: it ships next to the executable in plain text, contains every answer,
    /// and anything edited in it is trusted at runtime.
    /// </summary>
    public static class HackingDataFile
    {
        [Serializable]
        public class DeviceRecord
        {
            public string id;
            public string pin;
            public List<int> shifts = new List<int>();
            public List<string> authCodes = new List<string>();
        }

        [Serializable]
        class FileRoot
        {
            public List<DeviceRecord> devices = new List<DeviceRecord>();
        }

        static Dictionary<string, DeviceRecord> s_records;

        public static string FilePath => Path.Combine(Application.streamingAssetsPath, "hacking", "devices.json");

        static void Load()
        {
            s_records = new Dictionary<string, DeviceRecord>();
            try
            {
                if (!File.Exists(FilePath)) return;
                var root = JsonUtility.FromJson<FileRoot>(File.ReadAllText(FilePath));
                foreach (var r in root.devices) s_records[r.id] = r;
            }
            catch (Exception e) { Debug.LogWarning("[Hacking] devices.json unreadable: " + e.Message); }
        }

        static DeviceRecord Get(string id)
        {
            if (s_records == null) Load();
            return s_records.TryGetValue(id, out var r) ? r : null;
        }

        public static string Pin(DeviceDefinition d)
        {
            var r = Get(d.id);
            return r != null && !string.IsNullOrEmpty(r.pin) ? r.pin : d.pin;
        }

        public static int Shift(DeviceDefinition d, int messageIndex)
        {
            var r = Get(d.id);
            return r != null && messageIndex < r.shifts.Count ? r.shifts[messageIndex] : d.messages[messageIndex].shift;
        }

        public static string AuthCode(DeviceDefinition d, int orderIndex)
        {
            var r = Get(d.id);
            return r != null && orderIndex < r.authCodes.Count ? r.authCodes[orderIndex] : d.orders[orderIndex].authCode;
        }
    }
}
