using Unity.AI.Navigation;
using UnityEngine;

namespace ExitClause.AI
{
    /// <summary>
    /// Fallback for NavMeshSurfaces that have no baked data (e.g. after you edit a greybox):
    /// bakes them when the scene starts. The scene builders also bake and save data in the Editor.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public class RuntimeNavMeshBaker : MonoBehaviour
    {
        void Awake()
        {
            foreach (var s in FindObjectsByType<NavMeshSurface>(FindObjectsInactive.Exclude))
                if (s.navMeshData == null) s.BuildNavMesh();
        }
    }
}
