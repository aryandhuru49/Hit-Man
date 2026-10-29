using ExitClause.Story;
using ExitClause.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace ExitClause.UI
{
    /// <summary>
    /// The Suspicion Journal. Observations tab lists logged clues and partial sightings; the Suspect
    /// Board lets the player select a clue and pin it to a suspect card; Status shows money and story stats.
    /// </summary>
    public class JournalScreen : UIScreen
    {
        enum Tab { Observations, Board, Status }

        RectTransform m_content;
        Tab m_tab = Tab.Observations;
        string m_selectedClue;

        protected override void BuildLayout(Transform root)
        {
            UIKit.FullScreen(root, "Dim", new Color(0, 0, 0, 0.6f));
            UIKit.Window(root, "Suspicion Journal", new Vector2(1600, 900), out var body);
            var tabs = UIKit.HorizontalLayout(body, "Tabs", 10f, 50f);
            UIKit.Place(tabs, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -50), Vector2.zero);
            UIKit.Button(tabs, "Observations", () => { m_tab = Tab.Observations; Refresh(); });
            UIKit.Button(tabs, "Suspect Board", () => { m_tab = Tab.Board; Refresh(); });
            UIKit.Button(tabs, "Status", () => { m_tab = Tab.Status; Refresh(); });
            UIKit.Button(tabs, "Close [J]", Close);
            m_content = UIKit.Rect(body, "Content");
            UIKit.Place(m_content, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0, -64));
        }

        public void Open() => OpenScreen();

        protected override void Refresh()
        {
            UIKit.Clear(m_content);
            switch (m_tab)
            {
                case Tab.Observations: BuildObservations(); break;
                case Tab.Board: BuildBoard(); break;
                case Tab.Status: BuildStatus(); break;
            }
        }

        void BuildObservations()
        {
            var scroll = UIKit.ScrollList(m_content, "List", out var list);
            UIKit.Place((RectTransform)scroll.transform, Vector2.zero, Vector2.one);
            var db = GameDatabase.Instance;
            var j = GameSession.Instance.State.journal;
            if (j.loggedClues.Count == 0 && j.observationProgress.Count == 0)
                UIKit.LayoutLabel(list, "Nothing noted yet. Watch people. Press E on anything suspicious to observe it.", 22, TextAnchor.MiddleLeft, UIKit.Dim);
            foreach (var id in j.loggedClues)
            {
                var c = db.GetClue(id);
                if (c == null) continue;
                var pinned = JournalManager.Instance.PinnedSuspect(id);
                var suspect = pinned != null ? db.GetSuspect(pinned) : null;
                UIKit.LayoutLabel(list, $"<b>{c.title}</b>" + (suspect != null ? $"   <color=#f2b84d>[pinned: {suspect.displayName}]</color>" : ""), 24).supportRichText = true;
                UIKit.LayoutLabel(list, c.description, 20, TextAnchor.UpperLeft, UIKit.Dim);
            }
            foreach (var p in j.observationProgress)
            {
                if (j.loggedClues.Contains(p.clueId)) continue;
                var c = db.GetClue(p.clueId);
                if (c == null) continue;
                UIKit.LayoutLabel(list, $"<i>Unconfirmed: {c.title} ({p.count}/{c.observationsRequired} sightings)</i>", 22, TextAnchor.MiddleLeft, UIKit.Dim).supportRichText = true;
            }
        }

        void BuildBoard()
        {
            var db = GameDatabase.Instance;
            var j = GameSession.Instance.State.journal;

            var left = UIKit.ScrollList(m_content, "Clues", out var clueList);
            UIKit.Place((RectTransform)left.transform, Vector2.zero, new Vector2(0.3f, 1), Vector2.zero, new Vector2(-10, 0));
            UIKit.LayoutLabel(clueList, "1. Select a clue", 22, TextAnchor.MiddleLeft, UIKit.Accent);
            foreach (var id in j.loggedClues)
            {
                var c = db.GetClue(id);
                if (c == null) continue;
                string clueId = id;
                var b = UIKit.Button(clueList, (m_selectedClue == id ? "> " : "") + c.title, () => { m_selectedClue = clueId; Refresh(); }, 48, 20);
                if (m_selectedClue == id) b.GetComponent<Image>().color = new Color(0.45f, 0.33f, 0.12f);
            }

            var board = UIKit.Panel(m_content, "Board", new Color(0.32f, 0.24f, 0.16f, 0.9f));
            UIKit.Place(board.rectTransform, new Vector2(0.3f, 0), Vector2.one, new Vector2(10, 0), Vector2.zero);
            var cols = UIKit.Rect(board.transform, "Columns");
            UIKit.Place(cols, Vector2.zero, Vector2.one, new Vector2(12, 12), new Vector2(-12, -12));
            var h = cols.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 12; h.childForceExpandWidth = true; h.childForceExpandHeight = true; h.childControlWidth = true; h.childControlHeight = true;

            foreach (var s in db.suspects)
            {
                if (s == null) continue;
                var card = UIKit.Panel(cols, "Card_" + s.id, s.cardColor);
                var v = UIKit.VerticalLayout(card.transform, "V", 6f, 10);
                UIKit.Place(v, Vector2.zero, Vector2.one);
                UIKit.LayoutLabel(v, s.displayName, 24, TextAnchor.MiddleCenter, Color.black, FontStyle.Bold);
                UIKit.LayoutLabel(v, s.theory, 16, TextAnchor.UpperLeft, new Color(0.15f, 0.12f, 0.1f));
                string suspectId = s.id;
                var pinBtn = UIKit.Button(v, m_selectedClue != null ? "2. Pin here" : "(select a clue)", () =>
                {
                    if (m_selectedClue == null) return;
                    JournalManager.Instance.Pin(m_selectedClue, suspectId);
                    m_selectedClue = null;
                    Refresh();
                }, 44, 18);
                pinBtn.interactable = m_selectedClue != null;
                foreach (var pin in j.pins)
                {
                    if (pin.suspectId != s.id) continue;
                    var c = db.GetClue(pin.clueId);
                    if (c == null) continue;
                    string clueId = pin.clueId;
                    UIKit.Button(v, "x  " + c.title, () => { JournalManager.Instance.Unpin(clueId); Refresh(); }, 40, 16);
                }
            }
        }

        void BuildStatus()
        {
            var scroll = UIKit.ScrollList(m_content, "Status", out var list);
            UIKit.Place((RectTransform)scroll.transform, Vector2.zero, Vector2.one);
            var st = GameSession.Instance.State;
            var story = st.story;
            UIKit.LayoutLabel(list, $"Cash: ${st.economy.cash:N0}", 26);
            UIKit.LayoutLabel(list, $"Agency pension fund: ${st.economy.pensionBalance:N0}", 26);
            UIKit.LayoutLabel(list, "Mansion: " + (st.economy.ownsMansion ? "owned (Villa Serena, Riviera)" : "-"), 26);
            UIKit.LayoutLabel(list, $"Evidence against the agency: {story.GetCounter(StoryKeys.EvidenceCollected)}", 24, TextAnchor.MiddleLeft, UIKit.Accent);
            UIKit.LayoutLabel(list, $"Rage episodes: {story.GetCounter(StoryKeys.RageUses)}   Evidence left while raging: {story.GetCounter(StoryKeys.RageEvidence)}", 22);
            UIKit.LayoutLabel(list, $"Team members eliminated: {story.GetCounter(StoryKeys.TeamKilled)} / 5", 22);
            UIKit.LayoutLabel(list, $"Correct board pins: {JournalManager.Instance.CorrectPins()}", 22);
            UIKit.LayoutLabel(list, "Inventory:", 24, TextAnchor.MiddleLeft, UIKit.Accent);
            foreach (var it in st.inventory.items)
            {
                var def = GameDatabase.Instance.GetItem(it.itemId);
                UIKit.LayoutLabel(list, $"  {(def != null ? def.displayName : it.itemId)}  x{it.count}", 20);
            }
            UIKit.LayoutLabel(list, $"Play time: {Mathf.FloorToInt(st.progress.playTimeSeconds / 60)} min", 18, TextAnchor.MiddleLeft, UIKit.Dim);
        }
    }
}
