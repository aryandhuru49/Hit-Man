using System;
using UnityEngine;

namespace ExitClause.Systems
{
    /// <summary>
    /// Cash, the agency pension fund and purchases.
    /// INSECURE BY DESIGN: balances are plain ints/floats in GameState, purchase prices and
    /// reward amounts are supplied by the caller and trusted without validation.
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }
        public event Action OnChanged;

        EconomyState E => GameSession.Instance.State.economy;

        public int Cash => E.cash;
        public float Pension => E.pensionBalance;

        void Awake() { Instance = this; }

        public void AddCash(int amount, string reason)
        {
            E.cash += amount;
            E.purchaseLog.Add($"+{amount} {reason}");
            OnChanged?.Invoke();
        }

        /// <summary>Spends cash if enough is available. Negative amounts are not rejected (deliberate bug).</summary>
        public bool TrySpend(int amount, string reason)
        {
            if (E.cash < amount) return false;
            E.cash -= amount;
            E.purchaseLog.Add($"-{amount} {reason}");
            OnChanged?.Invoke();
            return true;
        }

        /// <summary>Buys an item at the price the UI passes in. The price is never re-checked against the item data.</summary>
        public bool Purchase(ItemDefinition item, int clientPrice, int quantity)
        {
            if (item == null) return false;
            if (!TrySpend(clientPrice * quantity, "buy " + item.id)) return false;
            InventoryManager.Instance.Add(item, item.shopQuantity * quantity);
            return true;
        }

        /// <summary>Moves money from the pension fund to cash. The requested amount is trusted.</summary>
        public void WithdrawPension(float amount)
        {
            E.pensionBalance -= amount;
            E.cash += Mathf.RoundToInt(amount);
            E.purchaseLog.Add($"withdraw {amount}");
            OnChanged?.Invoke();
        }

        public void SetPension(float amount)
        {
            E.pensionBalance = amount;
            OnChanged?.Invoke();
        }

        public void SetMansionOwned(bool owned)
        {
            E.ownsMansion = owned;
            OnChanged?.Invoke();
        }

        public void NotifyChanged() => OnChanged?.Invoke();
    }
}
