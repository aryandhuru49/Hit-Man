using System;

namespace ExitClause.Hacking
{
    /// <summary>An agency message stored on a device, encrypted with a simple shift cipher.</summary>
    [Serializable]
    public class InterceptedMessage
    {
        public string from;
        public string plainText;
        public int shift = 3;
        public string revealsFlag;
        public string logsClueId;
        public int evidenceValue;
    }
}
