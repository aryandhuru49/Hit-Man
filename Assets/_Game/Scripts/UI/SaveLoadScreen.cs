using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.UI
{
    /// <summary>Save to or load from one of the JSON slots (slot 0 is the autosave).</summary>
    public class SaveLoadScreen : UIScreen
    {
        RectTransform m_body;
        bool m_saving;

        protected override void BuildLayout(Transform root)
        {
            UIKit.FullScreen(root, "Dim", new Color(0, 0, 0, 0.5f));
            UIKit.Window(root, "Save / Load", new Vector2(980, 560), out m_body);
        }

        public void Open(bool saving)
        {
            m_saving = saving;
            OpenScreen();
        }

        protected override void Refresh()
        {
            UIKit.Clear(m_body);
            var list = UIKit.VerticalLayout(m_body, "List", 10f, 0);
            UIKit.Place(list, Vector2.zero, Vector2.one);
            UIKit.LayoutLabel(list, m_saving ? "Choose a slot to save into:" : "Choose a save to load:", 22, TextAnchor.MiddleLeft, UIKit.Dim);
            var slots = SaveSystem.DescribeSlots();
            for (int i = 0; i < slots.Count; i++)
            {
                int slot = i;
                bool exists = SaveSystem.Exists(slot);
                var b = UIKit.Button(list, slots[i], () => Pick(slot), 56, 20);
                b.interactable = m_saving ? slot != SaveSystem.AutosaveSlot : exists;
            }
            UIKit.LayoutLabel(list, "Files: " + SaveSystem.SaveFolder, 16, TextAnchor.MiddleLeft, UIKit.Dim);
            UIKit.Button(list, "Back", Close);
        }

        void Pick(int slot)
        {
            if (m_saving)
            {
                bool ok = SaveSystem.Save(slot);
                UIRoot.Instance.HUD.Notify(ok ? "Game saved." : "Save failed.");
                Refresh();
            }
            else
            {
                Close();
                if (PauseController.Instance.IsPaused) PauseController.Instance.Resume();
                if (!SaveSystem.LoadAndContinue(slot)) UIRoot.Instance.HUD.Notify("Could not read save.");
            }
        }
    }
}
