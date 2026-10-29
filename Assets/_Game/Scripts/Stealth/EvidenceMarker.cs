using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Stealth
{
    /// <summary>Visible trace (blood, casings) left by kills during Rage mode. Counted by the story to track the player.</summary>
    public class EvidenceMarker : MonoBehaviour
    {
        public static void Spawn(Vector3 position)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Evidence";
            Destroy(go.GetComponent<Collider>());
            go.transform.position = position + Vector3.up * 0.02f;
            go.transform.localScale = new Vector3(1.2f, 0.01f, 1.2f);
            var db = GameDatabase.Instance;
            var r = go.GetComponent<Renderer>();
            if (db != null && db.evidenceMaterial != null) r.sharedMaterial = db.evidenceMaterial;
            go.AddComponent<EvidenceMarker>();
        }
    }
}
