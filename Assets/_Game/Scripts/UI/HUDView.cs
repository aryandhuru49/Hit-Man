using System.Collections.Generic;
using System.Text;
using ExitClause.AI;
using ExitClause.CameraSystem;
using ExitClause.Combat;
using ExitClause.Player;
using ExitClause.Story;
using ExitClause.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace ExitClause.UI
{
    /// <summary>
    /// Gameplay HUD: health, rage, cash/day, objective, weapon and ammo, gadget, interaction prompt,
    /// crosshair and lock-on bracket, stealth status, enemy suspicion bars, objective waypoint,
    /// notifications, mission titles, boss bar, timer, speedometer and the radar minimap.
    /// </summary>
    public class HUDView : MonoBehaviour
    {
        RectTransform m_root;
        Image m_healthFill, m_rageFill, m_rageOverlay, m_bossFill, m_crosshair, m_lockBracket, m_waypoint;
        Text m_cash, m_objective, m_weapon, m_gadget, m_prompt, m_stealth, m_titleBig, m_titleSmall, m_bossName, m_timer, m_speed, m_waypointText;
        GameObject m_rageGroup, m_bossGroup;
        RectTransform m_notifyList;
        CanvasGroup m_titleGroup;
        float m_titleUntil;
        Health m_boss;
        float m_timerValue = -1f;
        MinimapView m_minimap;
        readonly List<Image> m_suspicionBars = new List<Image>();
        readonly List<(Text text, float until)> m_notes = new List<(Text, float)>();
        bool m_visible = true;

        public void Build(Transform canvas)
        {
            m_root = UIKit.Rect(canvas, "HUD");
            UIKit.Place(m_root, Vector2.zero, Vector2.one);

            m_rageOverlay = UIKit.FullScreen(m_root, "RageOverlay", new Color(0.6f, 0f, 0f, 0f));
            m_rageOverlay.raycastTarget = false;

            // Health + rage (top left)
            var hb = UIKit.Panel(m_root, "HealthBG", new Color(0, 0, 0, 0.6f));
            UIKit.PlaceFixed(hb.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, -30), new Vector2(320, 22));
            m_healthFill = UIKit.Panel(hb.transform, "Fill", new Color(0.85f, 0.2f, 0.2f));
            UIKit.Place(m_healthFill.rectTransform, Vector2.zero, Vector2.one, new Vector2(3, 3), new Vector2(-3, -3));
            m_rageGroup = UIKit.Panel(m_root, "RageBG", new Color(0, 0, 0, 0.6f)).gameObject;
            UIKit.PlaceFixed((RectTransform)m_rageGroup.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, -58), new Vector2(320, 14));
            m_rageFill = UIKit.Panel(m_rageGroup.transform, "Fill", new Color(1f, 0.45f, 0.05f));
            UIKit.Place(m_rageFill.rectTransform, Vector2.zero, Vector2.one, new Vector2(2, 2), new Vector2(-2, -2));
            var rageLabel = UIKit.Label(m_rageGroup.transform, "RAGE [V]", 14, TextAnchor.MiddleRight);
            UIKit.Place(rageLabel.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-6, 0));

            m_cash = UIKit.Label(m_root, "", 30, TextAnchor.UpperLeft, new Color(0.5f, 0.95f, 0.5f), FontStyle.Bold);
            UIKit.PlaceFixed(m_cash.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, -80), new Vector2(500, 80));

            // Objective (top right)
            m_objective = UIKit.Label(m_root, "", 24, TextAnchor.UpperRight);
            UIKit.PlaceFixed(m_objective.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-30, -30), new Vector2(640, 200));

            // Weapon (bottom right)
            m_weapon = UIKit.Label(m_root, "", 30, TextAnchor.LowerRight, Color.white, FontStyle.Bold);
            UIKit.PlaceFixed(m_weapon.rectTransform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-30, 70), new Vector2(500, 50));
            m_gadget = UIKit.Label(m_root, "", 20, TextAnchor.LowerRight, UIKit.Dim);
            UIKit.PlaceFixed(m_gadget.rectTransform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-30, 40), new Vector2(500, 30));
            m_speed = UIKit.Label(m_root, "", 34, TextAnchor.LowerRight, UIKit.Accent, FontStyle.Bold);
            UIKit.PlaceFixed(m_speed.rectTransform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-30, 120), new Vector2(400, 50));

            // Prompt and stealth (bottom/top centre)
            m_prompt = UIKit.Label(m_root, "", 26, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            UIKit.PlaceFixed(m_prompt.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 220), new Vector2(900, 40));
            m_stealth = UIKit.Label(m_root, "", 22, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            UIKit.PlaceFixed(m_stealth.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(600, 30));

            m_crosshair = UIKit.Panel(m_root, "Crosshair", new Color(1, 1, 1, 0.85f));
            UIKit.PlaceFixed(m_crosshair.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(6, 6));
            m_lockBracket = UIKit.Panel(m_root, "LockOn", new Color(1f, 0.2f, 0.15f, 0.35f));
            m_lockBracket.rectTransform.sizeDelta = new Vector2(46, 46);
            var lockOutline = m_lockBracket.gameObject.AddComponent<Outline>();
            lockOutline.effectColor = new Color(1f, 0.3f, 0.2f);
            lockOutline.effectDistance = new Vector2(2, 2);
            m_lockBracket.rectTransform.anchorMin = m_lockBracket.rectTransform.anchorMax = Vector2.zero;

            m_waypoint = UIKit.Panel(m_root, "Waypoint", new Color(1f, 0.85f, 0.2f, 0.9f));
            m_waypoint.rectTransform.sizeDelta = new Vector2(18, 18);
            m_waypoint.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
            m_waypoint.rectTransform.anchorMin = m_waypoint.rectTransform.anchorMax = Vector2.zero;
            m_waypointText = UIKit.Label(m_root, "", 18, TextAnchor.UpperCenter, new Color(1f, 0.85f, 0.2f));
            m_waypointText.rectTransform.sizeDelta = new Vector2(200, 30);
            m_waypointText.rectTransform.anchorMin = m_waypointText.rectTransform.anchorMax = Vector2.zero;

            // Notifications
            m_notifyList = UIKit.VerticalLayout(m_root, "Notifications", 4f, 0);
            UIKit.PlaceFixed(m_notifyList, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(900, 300));
            m_notifyList.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;

            // Mission title
            var title = UIKit.Rect(m_root, "Title");
            UIKit.PlaceFixed(title, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 160), new Vector2(1400, 160));
            m_titleGroup = title.gameObject.AddComponent<CanvasGroup>();
            m_titleBig = UIKit.Label(title, "", 64, TextAnchor.MiddleCenter, UIKit.Accent, FontStyle.Bold);
            UIKit.Place(m_titleBig.rectTransform, new Vector2(0, 0.4f), Vector2.one);
            m_titleSmall = UIKit.Label(title, "", 28, TextAnchor.MiddleCenter);
            UIKit.Place(m_titleSmall.rectTransform, Vector2.zero, new Vector2(1, 0.4f));
            m_titleGroup.alpha = 0;

            // Boss bar
            m_bossGroup = UIKit.Rect(m_root, "Boss").gameObject;
            UIKit.PlaceFixed((RectTransform)m_bossGroup.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -90), new Vector2(800, 50));
            m_bossName = UIKit.Label(m_bossGroup.transform, "", 22, TextAnchor.UpperCenter, Color.white, FontStyle.Bold);
            UIKit.Place(m_bossName.rectTransform, new Vector2(0, 0.5f), Vector2.one);
            var bossBg = UIKit.Panel(m_bossGroup.transform, "BG", new Color(0, 0, 0, 0.6f));
            UIKit.Place(bossBg.rectTransform, Vector2.zero, new Vector2(1, 0.45f));
            m_bossFill = UIKit.Panel(bossBg.transform, "Fill", new Color(0.8f, 0.1f, 0.1f));
            UIKit.Place(m_bossFill.rectTransform, Vector2.zero, Vector2.one, new Vector2(2, 2), new Vector2(-2, -2));
            m_bossGroup.SetActive(false);

            m_timer = UIKit.Label(m_root, "", 40, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            UIKit.PlaceFixed(m_timer.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -150), new Vector2(300, 50));

            m_minimap = gameObject.AddComponent<MinimapView>();
            m_minimap.Build(m_root);
        }

        public void SetVisible(bool v)
        {
            m_visible = v;
            m_root.gameObject.SetActive(v);
        }

        public void Notify(string text)
        {
            var t = UIKit.LayoutLabel(m_notifyList, text, 24, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            m_notes.Add((t, Time.unscaledTime + 4f));
            if (m_notes.Count > 5) { Destroy(m_notes[0].text.gameObject); m_notes.RemoveAt(0); }
        }

        public void SetPrompt(string prompt) => m_prompt.text = string.IsNullOrEmpty(prompt) ? "" : "[E] " + prompt;

        public void ShowMissionTitle(string title, string subtitle)
        {
            m_titleBig.text = title;
            m_titleSmall.text = subtitle;
            m_titleUntil = Time.unscaledTime + 4f;
        }

        public void ShowBoss(string name, Health health)
        {
            m_boss = health;
            if (m_bossGroup == null) return;
            m_bossGroup.SetActive(health != null);
            if (m_bossName != null) m_bossName.text = name ?? "";
        }

        public void SetTimer(float seconds) => m_timerValue = seconds;

        void Update()
        {
            if (!m_visible) return;
            for (int i = m_notes.Count - 1; i >= 0; i--)
                if (Time.unscaledTime > m_notes[i].until) { Destroy(m_notes[i].text.gameObject); m_notes.RemoveAt(i); }
            m_titleGroup.alpha = Mathf.Clamp01((m_titleUntil - Time.unscaledTime) / 0.8f);
            m_timer.text = m_timerValue >= 0 ? Mathf.CeilToInt(m_timerValue).ToString() : "";
            if (m_boss != null) m_bossFill.rectTransform.anchorMax = new Vector2(m_boss.Normalized, 1);
            else if (m_bossGroup.activeSelf) m_bossGroup.SetActive(false);

            UpdateObjective();
            var p = PlayerCharacter.Instance;
            bool hasPlayer = p != null;
            m_minimap.SetVisible(hasPlayer);
            if (!hasPlayer)
            {
                m_cash.text = m_weapon.text = m_gadget.text = m_stealth.text = m_speed.text = "";
                m_healthFill.transform.parent.gameObject.SetActive(false);
                m_rageGroup.SetActive(false);
                m_crosshair.enabled = m_lockBracket.enabled = m_waypoint.enabled = false;
                m_waypointText.text = "";
                HideSuspicionBars(0);
                return;
            }
            m_healthFill.transform.parent.gameObject.SetActive(true);
            m_healthFill.rectTransform.anchorMax = new Vector2(p.Health.Normalized, 1);

            var state = GameSession.Instance.State;
            m_rageGroup.SetActive(p.Rage.Unlocked);
            m_rageFill.rectTransform.anchorMax = new Vector2(p.Rage.Meter / 100f, 1);
            m_rageOverlay.color = new Color(0.6f, 0, 0, p.Rage.IsActive ? 0.18f + Mathf.Sin(Time.time * 6f) * 0.04f : 0f);

            m_cash.text = $"${state.economy.cash:N0}\n<size=18>Day {state.progress.day}</size>";
            m_cash.supportRichText = true;

            var w = p.Combat.Equipped;
            if (w != null)
            {
                var a = p.Combat.Ammo(w);
                m_weapon.text = $"{w.displayName}   {a.inMagazine} / {a.reserve}" + (p.Combat.IsReloading ? "  RELOADING" : "");
            }
            else m_weapon.text = "Unarmed";
            string gadget = p.Combat.Gadgets[p.Combat.GadgetIndex];
            m_gadget.text = $"[G] {gadget} x{InventoryManager.Instance.Count(gadget)}   [H] switch   lockpicks x{InventoryManager.Instance.Count("lockpick")}";
            m_speed.text = p.CurrentVehicle != null ? Mathf.RoundToInt(p.CurrentVehicle.Speed * 3.6f) + " km/h" : "";

            var s = p.Stealth;
            if (s.IsHidden) SetStealth("HIDDEN", new Color(0.5f, 0.8f, 1f));
            else if (s.AnyEnemyInCombat) SetStealth("COMBAT", new Color(1f, 0.25f, 0.2f));
            else if (s.HighestSuspicion > 0.4f) SetStealth("SUSPICIOUS", new Color(1f, 0.8f, 0.2f));
            else if (s.IsDisguised) SetStealth("DISGUISED: " + s.DisguiseId.Replace('_', ' ').ToUpperInvariant(), new Color(0.7f, 0.9f, 0.7f));
            else SetStealth(p.Motor.IsCrouching ? "CROUCHED" : "", Color.white);

            var rig = GameCameraRig.Instance;
            bool aiming = rig != null && rig.CurrentMode == GameCameraRig.Mode.Aim;
            m_crosshair.enabled = aiming;
            m_lockBracket.enabled = false;
            if (aiming && rig.LockTarget != null && ToScreen(rig, rig.LockTarget.AimPosition, out var lp))
            {
                m_lockBracket.enabled = true;
                m_lockBracket.rectTransform.anchoredPosition = lp;
            }
            UpdateSuspicionBars(rig);
            UpdateWaypoint(rig, p);
        }

        void SetStealth(string text, Color c) { m_stealth.text = text; m_stealth.color = c; }

        void UpdateObjective()
        {
            var mm = MissionManager.Instance;
            if (mm == null || mm.Current == null) { m_objective.text = ""; return; }
            var sb = new StringBuilder();
            sb.Append("<b>").Append(mm.Current.title).Append("</b>\n");
            var o = mm.CurrentObjective;
            if (o != null)
            {
                sb.Append("> ").Append(o.text);
                if (o.requiredCount > 1) sb.Append($" ({mm.GetCount(o.id)}/{o.requiredCount})");
            }
            foreach (var opt in mm.Current.objectives)
                if (opt.optional && !opt.hidden && !mm.IsObjectiveDone(opt.id))
                    sb.Append("\n<color=#aaaaaa>Optional: ").Append(opt.text).Append("</color>");
            m_objective.supportRichText = true;
            m_objective.text = sb.ToString();
        }

        bool ToScreen(GameCameraRig rig, Vector3 world, out Vector2 canvasPos)
        {
            canvasPos = Vector2.zero;
            Vector3 sp = rig.MainCamera.WorldToScreenPoint(world);
            if (sp.z <= 0) return false;
            var canvasRt = (RectTransform)m_root.parent;
            float scale = canvasRt.rect.width / Screen.width;
            canvasPos = new Vector2(sp.x * scale, sp.y * scale);
            return true;
        }

        void UpdateSuspicionBars(GameCameraRig rig)
        {
            int used = 0;
            if (rig != null)
            {
                foreach (var e in EnemyAI.All)
                {
                    if (e == null || e.IsDead || e.Perception.Suspicion < 0.05f) continue;
                    if (!ToScreen(rig, e.transform.position + Vector3.up * 2.3f, out var pos)) continue;
                    if (used >= m_suspicionBars.Count)
                    {
                        var bg = UIKit.Panel(m_root, "Suspicion", new Color(0, 0, 0, 0.5f));
                        bg.rectTransform.anchorMin = bg.rectTransform.anchorMax = Vector2.zero;
                        bg.rectTransform.sizeDelta = new Vector2(60, 8);
                        var fill = UIKit.Panel(bg.transform, "Fill", Color.yellow);
                        UIKit.Place(fill.rectTransform, Vector2.zero, Vector2.one);
                        m_suspicionBars.Add(fill);
                    }
                    var bar = m_suspicionBars[used++];
                    bar.transform.parent.gameObject.SetActive(true);
                    ((RectTransform)bar.transform.parent).anchoredPosition = pos;
                    float sv = e.Perception.Suspicion;
                    bar.rectTransform.anchorMax = new Vector2(sv, 1);
                    bar.color = sv >= 1f ? Color.red : Color.Lerp(Color.white, new Color(1f, 0.8f, 0.1f), sv);
                }
            }
            HideSuspicionBars(used);
        }

        void HideSuspicionBars(int from)
        {
            for (int i = from; i < m_suspicionBars.Count; i++) m_suspicionBars[i].transform.parent.gameObject.SetActive(false);
        }

        void UpdateWaypoint(GameCameraRig rig, PlayerCharacter p)
        {
            m_waypoint.enabled = false;
            m_waypointText.text = "";
            var marker = ActiveMarker();
            if (marker == null || rig == null) return;
            Vector3 world = marker.transform.position + Vector3.up * 1.5f;
            Vector3 sp = rig.MainCamera.WorldToScreenPoint(world);
            var canvasRt = (RectTransform)m_root.parent;
            float scale = canvasRt.rect.width / Screen.width;
            if (sp.z < 0) { sp.x = Screen.width - sp.x; sp.y = 0; }
            sp.x = Mathf.Clamp(sp.x, 40, Screen.width - 40);
            sp.y = Mathf.Clamp(sp.y, 40, Screen.height - 40);
            var pos = new Vector2(sp.x * scale, sp.y * scale);
            m_waypoint.enabled = true;
            m_waypoint.rectTransform.anchoredPosition = pos;
            m_waypointText.rectTransform.anchoredPosition = pos - new Vector2(0, 14);
            m_waypointText.text = Mathf.RoundToInt(Vector3.Distance(p.transform.position, marker.transform.position)) + "m";
        }

        public static ObjectiveMarker ActiveMarker()
        {
            var mm = MissionManager.Instance;
            if (mm == null || mm.CurrentObjective == null) return null;
            foreach (var m in ObjectiveMarker.All)
                if (m != null && m.objectiveId == mm.CurrentObjective.id) return m;
            return null;
        }
    }
}
