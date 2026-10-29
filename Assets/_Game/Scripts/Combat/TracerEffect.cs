using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Combat
{
    /// <summary>Short-lived line from muzzle to impact so shots are readable without VFX assets.</summary>
    public class TracerEffect : MonoBehaviour
    {
        LineRenderer m_line;
        float m_life = 0.06f;

        public static void Spawn(Vector3 from, Vector3 to)
        {
            var go = new GameObject("Tracer");
            var t = go.AddComponent<TracerEffect>();
            t.m_line = go.AddComponent<LineRenderer>();
            t.m_line.positionCount = 2;
            t.m_line.SetPosition(0, from);
            t.m_line.SetPosition(1, to);
            t.m_line.startWidth = 0.03f;
            t.m_line.endWidth = 0.01f;
            t.m_line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var db = GameDatabase.Instance;
            if (db != null && db.tracerMaterial != null) t.m_line.sharedMaterial = db.tracerMaterial;
        }

        void Update()
        {
            m_life -= Time.deltaTime;
            if (m_life <= 0f) Destroy(gameObject);
        }
    }
}
