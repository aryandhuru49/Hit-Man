using ExitClause.AI;
using ExitClause.Combat;
using ExitClause.Player;
using ExitClause.Systems;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Stealth
{
    /// <summary>
    /// A downed agent. The player can search it for a disguise or item; other guards who see it
    /// raise the alert, so bodies should be left out of patrol sight lines.
    /// </summary>
    public class DeadBody : Interactable
    {
        public string disguiseId;
        public string itemId;
        bool m_discovered;
        bool m_searched;
        float m_nextCheck;

        public static void MakeBody(EnemyAI enemy)
        {
            var go = enemy.gameObject;
            foreach (var col in go.GetComponents<Collider>()) col.enabled = false;
            var lockOn = go.GetComponent<LockOnTarget>();
            if (lockOn != null) lockOn.enabled = false;
            var cone = go.GetComponentInChildren<VisionConeVisualizer>();
            if (cone != null) cone.gameObject.SetActive(false);
            go.transform.rotation = Quaternion.Euler(-90f, go.transform.eulerAngles.y, 0f);
            go.transform.position += Vector3.up * 0.3f;
            var body = go.AddComponent<DeadBody>();
            body.disguiseId = enemy.dropsDisguise;
            body.itemId = enemy.dropsItem;
            body.prompt = "Search body";
            body.focusPoint = null;
        }

        public override bool CanInteract(PlayerCharacter player) =>
            base.CanInteract(player) && !m_searched && (!string.IsNullOrEmpty(disguiseId) || !string.IsNullOrEmpty(itemId));

        public override string Prompt => !string.IsNullOrEmpty(disguiseId) ? "Take disguise" : "Search body";

        public override void Interact(PlayerCharacter player)
        {
            m_searched = true;
            if (!string.IsNullOrEmpty(disguiseId))
            {
                player.Stealth.SetDisguise(disguiseId);
                UIRoot.Instance.HUD.Notify("Disguised as " + disguiseId.Replace('_', ' '));
            }
            if (!string.IsNullOrEmpty(itemId))
            {
                var item = GameDatabase.Instance.GetItem(itemId);
                if (item != null) InventoryManager.Instance.Add(item, 1); else InventoryManager.Instance.Add(itemId, 1);
                UIRoot.Instance.HUD.Notify("Found: " + (item != null ? item.displayName : itemId));
            }
        }

        void Update()
        {
            if (m_discovered || Time.time < m_nextCheck) return;
            m_nextCheck = Time.time + 0.5f;
            foreach (var e in EnemyAI.All)
            {
                if (e == null || e.IsDead) continue;
                if (Vector3.Distance(e.transform.position, transform.position) > 12f) continue;
                if (!e.Perception.IsInSight(transform.position + Vector3.up * 0.3f)) continue;
                m_discovered = true;
                UIRoot.Instance.HUD.Notify("A body was found!");
                AlertManager.RaiseAlarm(transform.position, false);
                break;
            }
        }
    }
}
