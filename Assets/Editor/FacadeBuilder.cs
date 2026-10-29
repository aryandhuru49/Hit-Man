using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ExitClause.EditorTools
{
    /// <summary>
    /// Dresses a GB.Building block so it reads as architecture instead of a box: framed windows with sills
    /// on every floor, cornices at each floor line and the roof, a plinth, ground-floor shopfronts and doors,
    /// and a few balconies. At night some windows glow. All pieces are merged into one mesh per material
    /// (no colliders), so a street of buildings stays cheap. Layout is seeded by the building name, so a
    /// rebuild gives the same result.
    /// </summary>
    public static class FacadeBuilder
    {
        const float GroundFloor = 4f;
        const float Floor = 3.2f;
        const float Bay = 2.8f;

        static Mesh s_cube;
        static Dictionary<string, List<CombineInstance>> s_parts;
        static Vector3 s_n, s_t;
        static float s_d;

        public static void Add(string name, Vector3 groundCenter, Vector3 size, float yaw, Transform parent, bool night)
        {
            if (s_cube == null) s_cube = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            s_parts = new Dictionary<string, List<CombineInstance>>();
            var rng = new System.Random(StableSeed(name));
            float h = size.y;
            bool tall = h >= 6.5f;
            float ground = tall ? GroundFloor : Mathf.Min(3f, h - 0.6f);
            int floors = tall ? Mathf.FloorToInt((h - ground - 0.6f) / Floor) : 0;

            // +Z, -Z, +X, -X faces: outward normal, the axis running along the face, face length, distance from centre.
            var sides = new[]
            {
                (n: Vector3.forward, len: size.x, d: size.z * 0.5f),
                (n: Vector3.back, len: size.x, d: size.z * 0.5f),
                (n: Vector3.right, len: size.z, d: size.x * 0.5f),
                (n: Vector3.left, len: size.z, d: size.x * 0.5f),
            };
            foreach (var side in sides)
            {
                s_n = side.n; s_d = side.d;
                s_t = Vector3.Cross(Vector3.up, s_n);
                float len = side.len;
                int bays = Mathf.Max(1, Mathf.FloorToInt((len - 1f) / Bay));
                float step = len / bays;

                Piece("Trim", 0f, 0.3f, len + 0.1f, 0.6f, 0.08f);                 // plinth
                Piece("Trim", 0f, h - 0.45f, len + 0.3f, 0.4f, 0.3f);             // roof cornice
                if (tall) Piece("Trim", 0f, ground, len + 0.15f, 0.22f, 0.14f);   // ground-floor band

                for (int b = 0; b < bays; b++)
                {
                    float x = -len * 0.5f + step * (b + 0.5f);
                    // Ground floor: a door every few bays, shop windows in between.
                    if (tall && len > 8f && b % 4 == 1) Door(x);
                    else if (tall) Window(x, 0.7f, 1.9f, ground - 1.3f, night, rng, 0.3f);
                    else Window(x, 0.9f, 1.0f, Mathf.Min(1.3f, ground - 1.3f), night, rng, 0.25f);

                    for (int f = 0; f < floors; f++)
                    {
                        float y = ground + f * Floor;
                        Window(x, y + 0.9f, 1.1f, 1.7f, night, rng, 0.2f);
                        if (rng.NextDouble() < 0.12) Balcony(x, y + 0.05f);
                    }
                }
                for (int f = 1; f < floors; f++) Piece("Trim", 0f, ground + f * Floor, len + 0.1f, 0.14f, 0.1f); // floor lines
            }

            var root = new GameObject(name + "_Facade");
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(groundCenter, Quaternion.Euler(0, yaw, 0));
            foreach (var kv in s_parts)
            {
                var mesh = new Mesh { name = $"{name}_{kv.Key}", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
                mesh.CombineMeshes(kv.Value.ToArray(), true, true);
                var go = new GameObject(kv.Key, typeof(MeshFilter), typeof(MeshRenderer));
                go.transform.SetParent(root.transform, false);
                go.GetComponent<MeshFilter>().sharedMesh = mesh;
                go.GetComponent<MeshRenderer>().sharedMaterial = MaterialLibrary.Get(kv.Key);
                GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
            }
        }

        static int StableSeed(string s) { int h = 17; foreach (char c in s) h = h * 31 + c; return h; }

        /// <summary>Framed window: stone surround, glass pane in front of it, and a sill.</summary>
        static void Window(float x, float bottom, float w, float hgt, bool night, System.Random rng, float litChance)
        {
            if (hgt < 0.4f) return;
            float cy = bottom + hgt * 0.5f;
            Piece("Trim", x, cy, w + 0.3f, hgt + 0.3f, 0.06f);
            bool lit = night && rng.NextDouble() < litChance;
            Piece(lit ? "WindowLit" : "WindowGlass", x, cy, w, hgt, 0.1f);
            // Glazing bars: a centre mullion and a transom at two thirds, so panes read as windows, not panels.
            Piece("Trim", x, cy, 0.06f, hgt, 0.04f, 0.12f);
            Piece("Trim", x, bottom + hgt * 0.66f, w, 0.06f, 0.04f, 0.12f);
            Piece("Trim", x, bottom - 0.12f, w + 0.45f, 0.09f, 0.26f);
        }

        static void Door(float x)
        {
            Piece("Trim", x, 1.4f, 1.9f, 2.9f, 0.06f);
            Piece("Wood", x, 1.25f, 1.4f, 2.5f, 0.1f);
            Piece("Metal", x + 0.5f, 1.1f, 0.08f, 0.08f, 0.16f); // handle
        }

        static void Balcony(float x, float floorY)
        {
            Piece("Trim", x, floorY, 2f, 0.14f, 0.8f);                 // slab, 0.8 m deep
            Piece("Metal", x, floorY + 1f, 2f, 0.05f, 0.05f, 0.78f);   // front rail
            for (int i = 0; i <= 6; i++)
                Piece("Metal", x - 0.95f + i * (1.9f / 6f), floorY + 0.5f, 0.03f, 0.95f, 0.03f, 0.78f);
        }

        /// <summary>A box on the current face: x along the face, y up, size (along, up, out), pushed out by <paramref name="outward"/>.</summary>
        static void Piece(string mat, float x, float y, float along, float up, float depth, float outward = -1f)
        {
            if (outward < 0f) outward = depth * 0.5f;
            var pos = s_n * (s_d + outward) + s_t * x + Vector3.up * y;
            var m = Matrix4x4.TRS(pos, Quaternion.LookRotation(s_n, Vector3.up), new Vector3(along, up, depth));
            if (!s_parts.TryGetValue(mat, out var list)) s_parts[mat] = list = new List<CombineInstance>();
            list.Add(new CombineInstance { mesh = s_cube, transform = m });
        }
    }
}
