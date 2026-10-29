using System.Linq;
using ExitClause.Hacking;
using ExitClause.Stealth;
using ExitClause.Story;
using ExitClause.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace ExitClause.UI
{
    /// <summary>
    /// Device hacking minigames: crack the PIN (smudge marks plus correct-position feedback),
    /// intercept messages (find the cipher shift), and spoof orders (pick a location and enter the
    /// auth code learned from a decrypted message).
    /// </summary>
    public class HackingScreen : UIScreen
    {
        enum Tab { Unlock, Messages, Orders }

        RectTransform m_content;
        Text m_header;
        DeviceDefinition m_device;
        Tab m_tab;
        string m_entry = "";
        string m_feedback = "";
        int m_attempts;
        float m_lockedUntil;
        int m_msgIndex;
        int m_shiftGuess;
        readonly System.Collections.Generic.Dictionary<string, int> m_locationPick = new System.Collections.Generic.Dictionary<string, int>();
        readonly System.Collections.Generic.Dictionary<string, string> m_codeEntry = new System.Collections.Generic.Dictionary<string, string>();

        protected override void BuildLayout(Transform root)
        {
            UIKit.FullScreen(root, "Dim", new Color(0, 0.02f, 0.03f, 0.75f));
            UIKit.Window(root, "Device", new Vector2(1300, 860), out var body);
            m_header = UIKit.Label(body, "", 22, TextAnchor.MiddleLeft, new Color(0.4f, 1f, 0.6f));
            UIKit.Place(m_header.rectTransform, new Vector2(0, 1), Vector2.one, new Vector2(0, -36), Vector2.zero);
            var tabs = UIKit.HorizontalLayout(body, "Tabs", 10f, 48f);
            UIKit.Place(tabs, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -92), new Vector2(0, -44));
            UIKit.Button(tabs, "Unlock", () => { m_tab = Tab.Unlock; Refresh(); });
            UIKit.Button(tabs, "Messages", () => { m_tab = Tab.Messages; Refresh(); });
            UIKit.Button(tabs, "Orders", () => { m_tab = Tab.Orders; Refresh(); });
            UIKit.Button(tabs, "Disconnect", Close);
            m_content = UIKit.Rect(body, "Content");
            UIKit.Place(m_content, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0, -104));
        }

        public void Open(DeviceDefinition device)
        {
            m_device = device;
            m_tab = DeviceProgress.IsUnlocked(device) ? Tab.Messages : Tab.Unlock;
            m_entry = "";
            m_feedback = "";
            m_attempts = 0;
            m_msgIndex = 0;
            OpenScreen();
        }

        protected override void Refresh()
        {
            if (m_device == null) return;
            bool unlocked = DeviceProgress.IsUnlocked(m_device);
            m_header.text = $"{m_device.deviceLabel}  //  owner: {m_device.ownerName}  //  {(unlocked ? "UNLOCKED" : "LOCKED")}";
            UIKit.Clear(m_content);
            if (!unlocked) m_tab = Tab.Unlock;
            switch (m_tab)
            {
                case Tab.Unlock: BuildUnlock(unlocked); break;
                case Tab.Messages: BuildMessages(); break;
                case Tab.Orders: BuildOrders(); break;
            }
        }

        void BuildUnlock(bool unlocked)
        {
            var list = UIKit.VerticalLayout(m_content, "Unlock", 10f, 0);
            UIKit.Place(list, new Vector2(0.25f, 0), new Vector2(0.75f, 1));
            if (unlocked) { UIKit.LayoutLabel(list, "Device unlocked.", 28, TextAnchor.MiddleCenter, new Color(0.4f, 1f, 0.6f)); return; }

            string pin = HackingDataFile.Pin(m_device);
            var smudges = string.Join("  ", pin.Distinct().OrderBy(c => c));
            UIKit.LayoutLabel(list, $"Smudge marks on the screen over keys: {smudges}", 22, TextAnchor.MiddleCenter, UIKit.Dim);
            string shown = m_entry.PadRight(pin.Length, '_');
            UIKit.LayoutLabel(list, string.Join(" ", shown.ToCharArray()), 48, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            UIKit.LayoutLabel(list, m_feedback, 22, TextAnchor.MiddleCenter, UIKit.Accent);

            string[] rows = { "123", "456", "789", "C0>" };
            foreach (var r in rows)
            {
                var row = UIKit.HorizontalLayout(list, "Row", 10f, 64f);
                foreach (char c in r)
                {
                    char key = c;
                    string label = c == 'C' ? "Clear" : c == '>' ? "Enter" : c.ToString();
                    UIKit.Button(row, label, () => Key(key, pin), 64, 30);
                }
            }
        }

        void Key(char key, string pin)
        {
            if (Time.unscaledTime < m_lockedUntil)
            {
                m_feedback = $"LOCKED OUT. Try again in {Mathf.CeilToInt(m_lockedUntil - Time.unscaledTime)}s";
                Refresh();
                return;
            }
            if (key == 'C') m_entry = "";
            else if (key == '>') Submit(pin);
            else if (m_entry.Length < pin.Length) m_entry += key;
            Refresh();
        }

        void Submit(string pin)
        {
            if (m_entry == pin)
            {
                DeviceProgress.SetUnlocked(m_device);
                if (!string.IsNullOrEmpty(m_device.flagOnUnlock)) GameSession.Instance.Story.SetFlag(m_device.flagOnUnlock);
                if (!string.IsNullOrEmpty(m_device.disableCameraGroupOnUnlock))
                {
                    SecurityCamera.DisableGroup(m_device.disableCameraGroupOnUnlock);
                    UIRoot.Instance.HUD.Notify("Cameras offline.");
                }
                MissionManager.Instance.Progress(m_device.objectiveOnUnlock);
                m_tab = m_device.messages.Count > 0 ? Tab.Messages : Tab.Unlock;
                m_feedback = "";
                if (m_device.messages.Count == 0 && m_device.orders.Count == 0) Close();
                return;
            }
            int correct = 0;
            for (int i = 0; i < pin.Length && i < m_entry.Length; i++) if (pin[i] == m_entry[i]) correct++;
            m_attempts++;
            m_feedback = $"Wrong PIN. {correct} digit(s) in the right place. ({m_device.attemptsBeforeLockout - m_attempts} tries before lockout)";
            if (m_attempts >= m_device.attemptsBeforeLockout)
            {
                m_attempts = 0;
                m_lockedUntil = Time.unscaledTime + m_device.lockoutSeconds;
                m_feedback = "Too many attempts. Device locked for " + m_device.lockoutSeconds + "s.";
            }
            m_entry = "";
        }

        void BuildMessages()
        {
            if (m_device.messages.Count == 0) { UIKit.LayoutLabel(m_content, "No messages.", 24); return; }
            var left = UIKit.VerticalLayout(m_content, "Inbox", 8f, 0);
            UIKit.Place(left, Vector2.zero, new Vector2(0.3f, 1), Vector2.zero, new Vector2(-10, 0));
            for (int i = 0; i < m_device.messages.Count; i++)
            {
                int idx = i;
                bool done = DeviceProgress.IsDecrypted(m_device, i);
                UIKit.Button(left, (done ? "[OK] " : "[ENC] ") + m_device.messages[i].from, () => { m_msgIndex = idx; m_shiftGuess = 0; Refresh(); }, 52, 20);
            }

            var right = UIKit.VerticalLayout(m_content, "Reader", 10f, 10);
            UIKit.Place(right, new Vector2(0.3f, 0), Vector2.one, new Vector2(10, 0), Vector2.zero);
            var msg = m_device.messages[m_msgIndex];
            int shift = HackingDataFile.Shift(m_device, m_msgIndex);
            string cipher = ShiftCipher.Encrypt(msg.plainText, shift);
            bool decrypted = DeviceProgress.IsDecrypted(m_device, m_msgIndex);
            UIKit.LayoutLabel(right, "From: " + msg.from, 22, TextAnchor.MiddleLeft, UIKit.Accent);
            if (decrypted)
            {
                UIKit.LayoutLabel(right, msg.plainText, 24, TextAnchor.UpperLeft, new Color(0.4f, 1f, 0.6f));
                return;
            }
            UIKit.LayoutLabel(right, "Intercepted (encrypted):\n" + cipher, 22, TextAnchor.UpperLeft, UIKit.Dim);
            UIKit.LayoutLabel(right, $"Decoder shift: {m_shiftGuess}", 24, TextAnchor.MiddleLeft, Color.white, FontStyle.Bold);
            UIKit.LayoutLabel(right, ShiftCipher.Decrypt(cipher, m_shiftGuess), 24, TextAnchor.UpperLeft, Color.white);
            var row = UIKit.HorizontalLayout(right, "Shift", 10f, 52f);
            UIKit.Button(row, "< Shift", () => { m_shiftGuess = (m_shiftGuess + 25) % 26; Refresh(); });
            UIKit.Button(row, "Shift >", () => { m_shiftGuess = (m_shiftGuess + 1) % 26; Refresh(); });
            UIKit.Button(row, "Lock decryption", () => TryDecrypt(shift));
        }

        void TryDecrypt(int correctShift)
        {
            if (m_shiftGuess % 26 != correctShift % 26)
            {
                UIRoot.Instance.HUD.Notify("That's still gibberish.");
                return;
            }
            var msg = m_device.messages[m_msgIndex];
            DeviceProgress.SetDecrypted(m_device, m_msgIndex);
            var story = GameSession.Instance.Story;
            if (!string.IsNullOrEmpty(msg.revealsFlag)) story.SetFlag(msg.revealsFlag);
            if (msg.evidenceValue != 0) story.AddCounter(StoryKeys.EvidenceCollected, msg.evidenceValue);
            if (!string.IsNullOrEmpty(msg.logsClueId))
            {
                var clue = GameDatabase.Instance.GetClue(msg.logsClueId);
                if (clue != null) JournalManager.Instance.Log(clue);
            }
            MissionManager.Instance.Progress("decrypt_message");
            if (DeviceProgress.AllDecrypted(m_device)) MissionManager.Instance.Progress(m_device.objectiveOnAllMessages);
            Refresh();
        }

        void BuildOrders()
        {
            var list = UIKit.VerticalLayout(m_content, "Orders", 10f, 0);
            UIKit.Place(list, Vector2.zero, Vector2.one);
            if (m_device.orders.Count == 0) { UIKit.LayoutLabel(list, "This device cannot issue orders.", 24); return; }
            for (int i = 0; i < m_device.orders.Count; i++)
            {
                var o = m_device.orders[i];
                int idx = i;
                bool sent = DeviceProgress.IsSent(m_device, i);
                UIKit.LayoutLabel(list, $"To: {o.recipient}  -  {o.description}", 24, TextAnchor.MiddleLeft, UIKit.Accent);
                if (sent) { UIKit.LayoutLabel(list, "Order delivered.", 22, TextAnchor.MiddleLeft, new Color(0.4f, 1f, 0.6f)); continue; }
                m_locationPick.TryGetValue(o.id, out int pick);
                pick = Mathf.Clamp(pick, 0, Mathf.Max(0, o.locations.Count - 1));
                var row = UIKit.HorizontalLayout(list, "Row", 10f, 52f);
                UIKit.Button(row, "Location: " + (o.locations.Count > 0 ? o.locations[pick] : "-"), () =>
                {
                    m_locationPick[o.id] = (pick + 1) % Mathf.Max(1, o.locations.Count);
                    Refresh();
                }, 52, 20);
                var code = UIKit.InputField(row, "Auth code (from intercepted messages)", InputField.ContentType.Standard);
                m_codeEntry.TryGetValue(o.id, out var typed);
                code.text = typed ?? "";
                code.onValueChanged.AddListener(v => m_codeEntry[o.id] = v);
                UIKit.Button(row, "Send", () => Send(idx, o.locations.Count > 0 ? o.locations[pick] : ""), 52, 22);
            }
        }

        void Send(int index, string location)
        {
            var o = m_device.orders[index];
            m_codeEntry.TryGetValue(o.id, out var typed);
            string code = HackingDataFile.AuthCode(m_device, index);
            if (!string.Equals((typed ?? "").Trim(), code, System.StringComparison.OrdinalIgnoreCase))
            {
                UIRoot.Instance.HUD.Notify("Rejected: invalid authorisation.");
                return;
            }
            DeviceProgress.SetSent(m_device, index);
            bool deceptive = location != o.correctLocation;
            if (deceptive && !string.IsNullOrEmpty(o.setFlag)) GameSession.Instance.Story.SetFlag(o.setFlag);
            UIRoot.Instance.HUD.Notify(deceptive ? "Order accepted. They'll be looking in the wrong place." : "Order accepted... but that's where they already were.");
            MissionManager.Instance.Progress(o.objectiveId);
            Refresh();
        }
    }
}
