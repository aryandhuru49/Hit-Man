using ExitClause.Player;
using ExitClause.Systems;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>
    /// Data-only side effects when an objective starts or completes: set flags, grant items or cash,
    /// set the pension or mansion ownership, fill the rage meter, play radio dialogue, autosave.
    /// </summary>
    public class ObjectiveReaction : MonoBehaviour
    {
        public string objectiveId;
        public bool onComplete = true;
        [Tooltip("Only fire if this flag is set (optional).")]
        public string requireFlag;
        [Tooltip("Do not fire if this flag is set (optional).")]
        public string forbidFlag;
        public GameObject[] deactivate;
        public string counterKey;
        public int counterDelta;
        public string[] setFlags;
        public string[] giveItems;
        public int addCash;
        [Tooltip("Negative = leave unchanged.")]
        public float setPension = -1f;
        public bool grantMansion;
        public float fillRage;
        public DialogueSequence dialogue;
        public bool autosave;
        [TextArea] public string notification;

        void Start()
        {
            var mm = MissionManager.Instance;
            if (onComplete) mm.OnObjectiveCompleted += o => { if (o.id == objectiveId) Fire(); };
            else mm.OnObjectiveActivated += o => { if (o.id == objectiveId) Fire(); };
        }

        void Fire()
        {
            var story = GameSession.Instance.Story;
            if (!string.IsNullOrEmpty(requireFlag) && !story.HasFlag(requireFlag)) return;
            if (!string.IsNullOrEmpty(forbidFlag) && story.HasFlag(forbidFlag)) return;
            if (deactivate != null) foreach (var go in deactivate) if (go != null) go.SetActive(false);
            if (!string.IsNullOrEmpty(counterKey)) story.AddCounter(counterKey, counterDelta);
            if (setFlags != null) foreach (var f in setFlags) story.SetFlag(f);
            if (giveItems != null)
                foreach (var id in giveItems)
                {
                    var item = GameDatabase.Instance.GetItem(id);
                    if (item != null) InventoryManager.Instance.Add(item, 1); else InventoryManager.Instance.Add(id, 1);
                }
            if (addCash != 0) EconomyManager.Instance.AddCash(addCash, objectiveId);
            if (setPension >= 0f) EconomyManager.Instance.SetPension(setPension);
            if (grantMansion) { EconomyManager.Instance.SetMansionOwned(true); story.SetFlag(StoryKeys.MansionOwned); }
            if (fillRage > 0f && PlayerCharacter.Instance != null) PlayerCharacter.Instance.Rage.Fill(fillRage);
            if (!string.IsNullOrEmpty(notification)) UIRoot.Instance.HUD.Notify(notification);
            if (dialogue != null) DialogueRunner.Play(dialogue);
            if (autosave) SaveSystem.Save(SaveSystem.AutosaveSlot);
        }
    }
}
