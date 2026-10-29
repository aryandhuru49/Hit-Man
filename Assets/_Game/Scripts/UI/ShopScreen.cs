using ExitClause.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace ExitClause.UI
{
    /// <summary>
    /// Black-market dealer. INSECURE BY DESIGN: the price shown is read from the item asset and passed
    /// to EconomyManager.Purchase as the "client price"; nothing re-validates it.
    /// </summary>
    public class ShopScreen : UIScreen
    {
        RectTransform m_body;
        Text m_cash;
        string m_title = "Dealer";

        protected override void BuildLayout(Transform root)
        {
            UIKit.FullScreen(root, "Dim", new Color(0, 0, 0, 0.5f));
            UIKit.Window(root, "Dealer", new Vector2(1000, 800), out var body);
            m_cash = UIKit.Label(body, "", 26, TextAnchor.MiddleRight, new Color(0.5f, 0.95f, 0.5f), FontStyle.Bold);
            UIKit.Place(m_cash.rectTransform, new Vector2(0, 1), Vector2.one, new Vector2(0, -40), Vector2.zero);
            var scroll = UIKit.ScrollList(body, "Items", out m_body);
            UIKit.Place((RectTransform)scroll.transform, Vector2.zero, Vector2.one, new Vector2(0, 64), new Vector2(0, -50));
            var close = UIKit.Button(body, "Leave", Close);
            UIKit.Place((RectTransform)close.transform, Vector2.zero, new Vector2(1, 0), Vector2.zero, new Vector2(0, 52));
        }

        public void Open(string title)
        {
            m_title = title;
            OpenScreen();
        }

        protected override void Refresh()
        {
            m_cash.text = $"{m_title}    Cash: ${EconomyManager.Instance.Cash:N0}";
            UIKit.Clear(m_body);
            foreach (var item in GameDatabase.Instance.items)
            {
                if (item == null || !item.soldInShop) continue;
                var def = item;
                var row = UIKit.HorizontalLayout(m_body, "Row", 12f, 64f);
                var info = UIKit.LayoutLabel(row, $"<b>{item.displayName}</b>  x{item.shopQuantity}\n<size=16><color=#aaaaaa>{item.description}</color></size>", 22);
                info.supportRichText = true;
                info.GetComponent<RectTransform>().gameObject.AddComponent<LayoutElement>().flexibleWidth = 3;
                var buy = UIKit.Button(row, $"Buy ${item.price}", () =>
                {
                    // Client-side price: whatever is in memory/asset right now is what gets charged.
                    bool ok = EconomyManager.Instance.Purchase(def, def.price, 1);
                    UIRoot.Instance.HUD.Notify(ok ? "Bought " + def.displayName : "Not enough cash.");
                    Refresh();
                }, 56, 22);
                buy.gameObject.GetComponent<LayoutElement>().flexibleWidth = 1;
            }
        }
    }
}
