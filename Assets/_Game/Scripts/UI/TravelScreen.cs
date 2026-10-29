using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.UI
{
    /// <summary>Act 2 map on the safehouse wall: pick which of the three remaining agents to hunt, then Zurich.</summary>
    public class TravelScreen : UIScreen
    {
        RectTransform m_body;

        protected override void BuildLayout(Transform root)
        {
            UIKit.FullScreen(root, "Dim", new Color(0, 0, 0, 0.5f));
            UIKit.Window(root, "Map of Europe", new Vector2(900, 640), out m_body);
        }

        public void Open() => OpenScreen();

        protected override void Refresh()
        {
            UIKit.Clear(m_body);
            var s = GameSession.Instance.Story;
            var list = UIKit.VerticalLayout(m_body, "List", 12f, 0);
            UIKit.Place(list, Vector2.zero, Vector2.one);
            bool cracked = s.HasFlag(StoryKeys.DevicesCracked);
            if (!cracked) UIKit.LayoutLabel(list, "You don't know where they are yet. Crack the recovered phones first.", 22, TextAnchor.MiddleLeft, UIKit.Dim);

            AddTarget(list, "Swiss Alps - The Sniper", "Act2_Alps", StoryKeys.SniperDefeated, cracked);
            AddTarget(list, "Marseille - The Infiltrator", "Act2_Marseille", StoryKeys.InfiltratorDefeated, cracked);
            AddTarget(list, "Berlin - The Tech Specialist", "Act2_Berlin", StoryKeys.TechDefeated, cracked);

            bool allDone = s.HasFlag(StoryKeys.SniperDefeated) && s.HasFlag(StoryKeys.InfiltratorDefeated) && s.HasFlag(StoryKeys.TechDefeated);
            var z = UIKit.Button(list, allDone ? "Zurich - The Agency" : "Zurich - (need the Tech Specialist's network access and all three agents)", () => Go("Act3_Zurich"), 60, 22);
            z.interactable = allDone && s.HasFlag(StoryKeys.NetworkAccess);
            UIKit.Button(list, "Close", Close);
        }

        void AddTarget(Transform list, string label, string scene, string doneFlag, bool known)
        {
            bool done = GameSession.Instance.Story.HasFlag(doneFlag);
            var b = UIKit.Button(list, label + (done ? "   [DONE]" : ""), () => Go(scene), 60, 24);
            b.interactable = known && !done;
        }

        void Go(string scene)
        {
            Close();
            GameSession.Instance.State.progress.missionId = "";
            SaveSystem.Save(SaveSystem.AutosaveSlot);
            SceneLoader.Instance.Load(scene);
        }
    }
}
