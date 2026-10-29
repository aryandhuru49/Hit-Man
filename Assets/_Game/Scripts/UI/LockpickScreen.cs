using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ExitClause.UI
{
    /// <summary>
    /// Lockpick minigame: a pick sweeps across the lock; press Space or click while it is inside the
    /// pin's sweet spot to set the pin. Set three pins to open; one miss ends the attempt.
    /// </summary>
    public class LockpickScreen : UIScreen
    {
        RectTransform m_bar, m_zone, m_needle;
        Text m_status;
        Action<bool> m_done;
        int m_pinsSet;
        float m_t, m_speed;
        float m_zoneCenter, m_zoneWidth;

        protected override void BuildLayout(Transform root)
        {
            UIKit.FullScreen(root, "Dim", new Color(0, 0, 0, 0.5f));
            UIKit.Window(root, "Lockpick", new Vector2(900, 360), out var body);
            m_status = UIKit.Label(body, "", 24, TextAnchor.MiddleCenter);
            UIKit.Place(m_status.rectTransform, new Vector2(0, 0.65f), Vector2.one);
            var bar = UIKit.Panel(body, "Bar", new Color(0.15f, 0.15f, 0.17f));
            m_bar = bar.rectTransform;
            UIKit.Place(m_bar, new Vector2(0.05f, 0.3f), new Vector2(0.95f, 0.55f));
            m_zone = UIKit.Panel(bar.transform, "Zone", new Color(0.3f, 0.8f, 0.4f, 0.7f)).rectTransform;
            m_needle = UIKit.Panel(bar.transform, "Pick", Color.white).rectTransform;
            var cancel = UIKit.Button(body, "Cancel", () => Finish(false), 44, 20);
            UIKit.Place((RectTransform)cancel.transform, new Vector2(0.35f, 0), new Vector2(0.65f, 0.2f));
        }

        public void Open(Action<bool> onDone)
        {
            m_done = onDone;
            m_pinsSet = 0;
            NewPin();
            OpenScreen();
        }

        void NewPin()
        {
            m_zoneWidth = 0.14f - m_pinsSet * 0.03f;
            m_zoneCenter = UnityEngine.Random.Range(0.2f, 0.8f);
            m_speed = 0.8f + m_pinsSet * 0.35f;
            m_t = 0;
            m_zone.anchorMin = new Vector2(m_zoneCenter - m_zoneWidth / 2, 0);
            m_zone.anchorMax = new Vector2(m_zoneCenter + m_zoneWidth / 2, 1);
            m_status.text = $"Pins set: {m_pinsSet}/3   [Space / Click] to set the pin";
        }

        void Update()
        {
            if (!IsOpen) return;
            m_t += Time.unscaledDeltaTime * m_speed;
            float x = Mathf.PingPong(m_t, 1f);
            m_needle.anchorMin = new Vector2(x - 0.004f, -0.2f);
            m_needle.anchorMax = new Vector2(x + 0.004f, 1.2f);
            bool press = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                         || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
            if (!press) return;
            if (Mathf.Abs(x - m_zoneCenter) <= m_zoneWidth / 2)
            {
                m_pinsSet++;
                if (m_pinsSet >= 3) Finish(true); else NewPin();
            }
            else Finish(false);
        }

        void Finish(bool success)
        {
            Close();
            var cb = m_done;
            m_done = null;
            UIRoot.Instance.HUD.Notify(success ? "Lock opened." : "The pick slipped.");
            cb?.Invoke(success);
        }

        public override void Close()
        {
            base.Close();
        }
    }
}
