using ExitClause.Player;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Systems
{
    /// <summary>
    /// Hinged door. Locked doors need a lockpick (opens the lockpick minigame) or a key item.
    /// The hinge is this transform; the door swings around its local Y axis.
    /// </summary>
    public class Door : Interactable
    {
        public bool locked;
        [Tooltip("Key item id that opens the door directly (optional).")]
        public string keyItemId;
        public float openAngle = 95f;
        public float speed = 4f;
        [Tooltip("Objective progressed when the door is opened (optional).")]
        public string objectiveOnOpen;

        bool m_open;
        Quaternion m_closed;

        void Awake() { m_closed = transform.localRotation; }

        public override string Prompt
        {
            get
            {
                if (!locked) return m_open ? "Close door" : "Open door";
                if (!string.IsNullOrEmpty(keyItemId) && InventoryManager.Instance.Has(keyItemId)) return "Unlock door";
                return InventoryManager.Instance.Has("lockpick") ? "Pick lock" : "Locked (needs lockpick)";
            }
        }

        public override void Interact(PlayerCharacter player)
        {
            if (!locked) { SetOpen(!m_open); return; }
            if (!string.IsNullOrEmpty(keyItemId) && InventoryManager.Instance.Has(keyItemId)) { Unlock(); return; }
            if (!InventoryManager.Instance.Has("lockpick")) { UIRoot.Instance.HUD.Notify("You need a lockpick."); return; }
            UIRoot.Instance.Lockpick.Open(success =>
            {
                if (success) Unlock();
                else if (Random.value < 0.35f)
                {
                    InventoryManager.Instance.Remove("lockpick");
                    UIRoot.Instance.HUD.Notify("The lockpick snapped.");
                }
            });
        }

        void Unlock()
        {
            locked = false;
            NoiseSystem.Emit(transform.position, 3f, NoiseType.Impact, gameObject);
            SetOpen(true);
        }

        public void SetOpen(bool open)
        {
            m_open = open;
            if (open && !string.IsNullOrEmpty(objectiveOnOpen)) MissionManager.Instance.Progress(objectiveOnOpen);
        }

        void Update()
        {
            var target = m_open ? m_closed * Quaternion.Euler(0, openAngle, 0) : m_closed;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * speed);
        }
    }
}
