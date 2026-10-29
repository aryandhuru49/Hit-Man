using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.UI
{
    /// <summary>
    /// Base for modal screens (journal, shop, hacking...). Opening one blocks gameplay input and frees the
    /// cursor; Escape closes the top-most screen unless it is not closable (choices, game over).
    /// </summary>
    public abstract class UIScreen : MonoBehaviour
    {
        protected GameObject Root;
        public bool IsOpen { get; private set; }
        public virtual bool ClosableByEscape => true;
        public virtual bool PausesTime => false;

        public void Build(Transform canvas)
        {
            Root = UIKit.Rect(canvas, GetType().Name).gameObject;
            UIKit.Place((RectTransform)Root.transform, Vector2.zero, Vector2.one);
            BuildLayout(Root.transform);
            Root.SetActive(false);
        }

        protected abstract void BuildLayout(Transform root);

        protected void OpenScreen()
        {
            if (IsOpen) { Refresh(); return; }
            IsOpen = true;
            Root.SetActive(true);
            Root.transform.SetAsLastSibling();
            GameInput.PushBlock();
            if (PausesTime) Time.timeScale = 0f;
            UIRoot.Instance.PushScreen(this);
            Refresh();
        }

        public virtual void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            Root.SetActive(false);
            GameInput.PopBlock();
            if (PausesTime && !(PauseController.Instance != null && PauseController.Instance.IsPaused)) Time.timeScale = 1f;
            UIRoot.Instance.PopScreen(this);
        }

        protected virtual void Refresh() { }
    }
}
