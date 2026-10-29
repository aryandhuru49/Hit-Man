using System;
using UnityEngine;

namespace ExitClause.Systems
{
    /// <summary>Item counts (gadgets, key items, recovered devices) stored as plain stacks in GameState.</summary>
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }
        public event Action OnChanged;

        InventoryState Inv => GameSession.Instance.State.inventory;

        void Awake() { Instance = this; }

        public int Count(string itemId)
        {
            var s = Inv.items.Find(x => x.itemId == itemId);
            return s != null ? s.count : 0;
        }

        public bool Has(string itemId) => Count(itemId) > 0;

        public void Add(ItemDefinition item, int count)
        {
            if (item == null) return;
            if (item.category == ItemCategory.Ammo && !string.IsNullOrEmpty(item.ammoForWeapon))
            {
                var ammo = GameSession.Instance.State.player.ammo.Find(a => a.weaponId == item.ammoForWeapon);
                if (ammo != null) ammo.reserve += count;
                OnChanged?.Invoke();
                return;
            }
            Add(item.id, count);
        }

        public void Add(string itemId, int count)
        {
            var s = Inv.items.Find(x => x.itemId == itemId);
            if (s == null) Inv.items.Add(new ItemStack { itemId = itemId, count = count });
            else s.count += count;
            OnChanged?.Invoke();
        }

        public bool Remove(string itemId, int count = 1)
        {
            var s = Inv.items.Find(x => x.itemId == itemId);
            if (s == null || s.count < count) return false;
            s.count -= count;
            if (s.count <= 0) Inv.items.Remove(s);
            OnChanged?.Invoke();
            return true;
        }

        public void NotifyChanged() => OnChanged?.Invoke();
    }
}
