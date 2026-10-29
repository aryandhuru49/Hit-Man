using UnityEngine;

namespace ExitClause.Systems
{
    /// <summary>
    /// Guarantees the persistent GameRoot exists no matter which scene you press Play in,
    /// so every location scene can be tested on its own in the Editor.
    /// </summary>
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void EnsureRoot()
        {
            if (GameRoot.Instance != null) return;
            var go = new GameObject("[GameRoot]");
            go.AddComponent<GameRoot>();
        }
    }
}
