using ExitClause.Systems;

namespace ExitClause.Hacking
{
    /// <summary>Per-device hacking progress, stored as story flags so it is saved with everything else.</summary>
    public static class DeviceProgress
    {
        static StoryState S => GameSession.Instance.Story;

        public static bool IsUnlocked(DeviceDefinition d) => S.HasFlag("dev_" + d.id + "_unlocked");
        public static void SetUnlocked(DeviceDefinition d) => S.SetFlag("dev_" + d.id + "_unlocked");
        public static bool IsDecrypted(DeviceDefinition d, int i) => S.HasFlag($"dev_{d.id}_msg{i}");
        public static void SetDecrypted(DeviceDefinition d, int i) => S.SetFlag($"dev_{d.id}_msg{i}");
        public static bool IsSent(DeviceDefinition d, int i) => S.HasFlag($"dev_{d.id}_order{i}");
        public static void SetSent(DeviceDefinition d, int i) => S.SetFlag($"dev_{d.id}_order{i}");

        public static bool AllDecrypted(DeviceDefinition d)
        {
            for (int i = 0; i < d.messages.Count; i++) if (!IsDecrypted(d, i)) return false;
            return true;
        }
    }
}
