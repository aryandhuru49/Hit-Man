using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.Stealth
{
    /// <summary>
    /// Draws an AIPerception's vision cone as a flat fan on the ground, tinted white -> yellow -> red
    /// with suspicion, so stealth is readable in the greybox. Clipped by walls via raycasts.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class VisionConeVisualizer : MonoBehaviour
    {
        public AIPerception perception;
        public int segments = 24;
        public float groundOffset = 0.06f;
        [Tooltip("Draw the cone this far at most (keeps long sniper cones readable).")]
        public float maxDrawDistance = 14f;

        Mesh m_mesh;
        MeshRenderer m_renderer;
        MaterialPropertyBlock m_block;
        float m_nextRebuild;
        static readonly int UnlitColor = Shader.PropertyToID("_UnlitColor");
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        void Awake()
        {
            m_mesh = new Mesh { name = "VisionCone" };
            m_mesh.MarkDynamic();
            GetComponent<MeshFilter>().sharedMesh = m_mesh;
            m_renderer = GetComponent<MeshRenderer>();
            m_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_renderer.receiveShadows = false;
            var db = GameDatabase.Instance;
            if (db != null && db.visionConeMaterial != null) m_renderer.sharedMaterial = db.visionConeMaterial;
            m_block = new MaterialPropertyBlock();
            if (perception == null) perception = GetComponentInParent<AIPerception>();
        }

        void LateUpdate()
        {
            if (perception == null || perception.Blind) { m_renderer.enabled = false; return; }
            m_renderer.enabled = true;
            transform.SetPositionAndRotation(
                new Vector3(perception.EyePosition.x, perception.transform.position.y + groundOffset, perception.EyePosition.z),
                Quaternion.LookRotation(Vector3.ProjectOnPlane(perception.Forward, Vector3.up).normalized + Vector3.forward * 1e-4f));

            if (Time.time >= m_nextRebuild) { Rebuild(); m_nextRebuild = Time.time + 0.1f; }

            float s = perception.Suspicion;
            Color c = s < 0.5f ? Color.Lerp(new Color(1, 1, 1, 0.12f), new Color(1, 0.85f, 0.1f, 0.25f), s * 2f)
                                : Color.Lerp(new Color(1, 0.85f, 0.1f, 0.25f), new Color(1, 0.1f, 0.05f, 0.35f), (s - 0.5f) * 2f);
            m_block.SetColor(UnlitColor, c);
            m_block.SetColor(BaseColor, c);
            m_renderer.SetPropertyBlock(m_block);
        }

        void Rebuild()
        {
            float radius = Mathf.Min(perception.viewDistance, maxDrawDistance);
            float angle = Mathf.Min(perception.viewAngle, 359f);
            var verts = new Vector3[segments + 2];
            var tris = new int[segments * 3];
            verts[0] = Vector3.zero;
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            for (int i = 0; i <= segments; i++)
            {
                float a = -angle * 0.5f + angle * i / segments;
                Vector3 local = Quaternion.Euler(0, a, 0) * Vector3.forward;
                Vector3 world = transform.TransformDirection(local);
                float r = radius;
                if (Physics.Raycast(origin, world, out var hit, radius, Layers.EnvironmentMask, QueryTriggerInteraction.Ignore)) r = hit.distance;
                verts[i + 1] = local * r;
                if (i < segments) { tris[i * 3] = 0; tris[i * 3 + 1] = i + 1; tris[i * 3 + 2] = i + 2; }
            }
            m_mesh.Clear();
            m_mesh.vertices = verts;
            m_mesh.triangles = tris;
            m_mesh.RecalculateBounds();
        }
    }
}
