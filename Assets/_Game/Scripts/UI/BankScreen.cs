using System.Globalization;
using ExitClause.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace ExitClause.UI
{
    /// <summary>
    /// The agency's private pension portal on the mansion terminal. INSECURE BY DESIGN: the withdrawal
    /// amount typed by the player is trusted (no balance check, negative values accepted).
    /// </summary>
    public class BankScreen : UIScreen
    {
        Text m_balance, m_log;
        InputField m_amount;

        protected override void BuildLayout(Transform root)
        {
            UIKit.FullScreen(root, "Dim", new Color(0, 0, 0, 0.5f));
            UIKit.Window(root, "Helvetia Private Trust - Pension Portal", new Vector2(980, 700), out var body);
            var list = UIKit.VerticalLayout(body, "List", 12f, 0);
            UIKit.Place(list, Vector2.zero, Vector2.one);
            m_balance = UIKit.LayoutLabel(list, "", 28, TextAnchor.MiddleLeft, Color.white, FontStyle.Bold);
            UIKit.LayoutLabel(list, "Withdraw to cash:", 22, TextAnchor.MiddleLeft, UIKit.Dim);
            m_amount = UIKit.InputField(list, "Amount", InputField.ContentType.Standard);
            UIKit.Button(list, "Withdraw", Withdraw);
            m_log = UIKit.LayoutLabel(list, "", 18, TextAnchor.UpperLeft, UIKit.Dim);
            UIKit.Button(list, "Log out", Close);
        }

        public void Open() => OpenScreen();

        protected override void Refresh()
        {
            var e = GameSession.Instance.State.economy;
            m_balance.text = $"Pension balance: ${e.pensionBalance:N2}\nCash on hand: ${e.cash:N0}";
            int start = Mathf.Max(0, e.purchaseLog.Count - 6);
            m_log.text = "Recent:\n" + string.Join("\n", e.purchaseLog.GetRange(start, e.purchaseLog.Count - start));
        }

        void Withdraw()
        {
            if (!float.TryParse(m_amount.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var amount)) return;
            EconomyManager.Instance.WithdrawPension(amount);
            m_amount.text = "";
            Refresh();
        }
    }
}
