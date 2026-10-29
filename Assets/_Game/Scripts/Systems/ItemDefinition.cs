using UnityEngine;

namespace ExitClause.Systems
{
    public enum ItemCategory { Gadget, Ammo, Consumable, KeyItem, Device, Weapon, Vehicle }

    /// <summary>
    /// Data for anything the player can carry or buy. The shop reads the price from here and
    /// trusts it (INSECURE: change the asset, the JSON mirror, or the value in memory to buy for free).
    /// </summary>
    [CreateAssetMenu(menuName = "Exit Clause/Item")]
    public class ItemDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public ItemCategory category;
        public int price;
        public bool soldInShop;
        public int shopQuantity = 1;
        [Tooltip("For ammo items: which weapon id receives the rounds.")]
        public string ammoForWeapon;
        [Tooltip("For consumables: health restored.")]
        public float healAmount;
    }
}
