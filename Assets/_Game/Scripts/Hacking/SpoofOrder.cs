using System;
using System.Collections.Generic;

namespace ExitClause.Hacking
{
    /// <summary>A forged order the player can send from a compromised device (send the sniper to the wrong ridge).</summary>
    [Serializable]
    public class SpoofOrder
    {
        public string id;
        public string recipient;
        public string description;
        public List<string> locations = new List<string>();
        public string correctLocation;
        public string authCode;
        public string setFlag;
        public string objectiveId;
    }
}
