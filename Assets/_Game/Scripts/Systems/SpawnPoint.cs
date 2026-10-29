using UnityEngine;

namespace ExitClause.Systems
{
    /// <summary>Named player start position. The player prefab moves here when a scene loads.</summary>
    public class SpawnPoint : MonoBehaviour
    {
        public string id = "default";

        public static SpawnPoint Find(string id)
        {
            SpawnPoint fallback = null;
            foreach (var sp in FindObjectsByType<SpawnPoint>(FindObjectsInactive.Exclude))
            {
                if (sp.id == id) return sp;
                if (sp.id == "default" || fallback == null) fallback = sp;
            }
            return fallback;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.5f);
            Gizmos.DrawRay(transform.position + Vector3.up, transform.forward);
        }
    }
}
