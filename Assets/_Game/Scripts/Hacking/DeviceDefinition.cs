using System.Collections.Generic;
using UnityEngine;

namespace ExitClause.Hacking
{
    /// <summary>
    /// A hackable phone, laptop or relay box. INSECURE BY DESIGN: the PIN, cipher shifts and auth codes
    /// are stored in plain text here (readable with any asset ripper) and mirrored to
    /// StreamingAssets/hacking/devices.json, which overrides these values at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Exit Clause/Device")]
    public class DeviceDefinition : ScriptableObject
    {
        public string id;
        public string deviceLabel;
        public string ownerName;
        public string pin = "1234";
        public int attemptsBeforeLockout = 5;
        public float lockoutSeconds = 15f;
        public List<InterceptedMessage> messages = new List<InterceptedMessage>();
        public List<SpoofOrder> orders = new List<SpoofOrder>();
        [Header("Story hooks")]
        public string flagOnUnlock;
        public string objectiveOnUnlock;
        public string objectiveOnAllMessages;
        public string disableCameraGroupOnUnlock;
    }
}
