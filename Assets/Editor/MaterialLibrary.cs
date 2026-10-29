using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace ExitClause.EditorTools
{
    /// <summary>
    /// Creates the flat-colour HDRP greybox materials under Assets/_Game/Materials. Swap these for
    /// Poly Haven / Quixel textures later: every greybox object references one of these by name,
    /// so replacing a material asset re-skins the whole game.
    /// </summary>
    public static class MaterialLibrary
    {
        const string Folder = "Assets/_Game/Materials";
        static readonly Dictionary<string, Material> s_cache = new Dictionary<string, Material>();

        [MenuItem("Exit Clause/2. Create Greybox Materials", priority = 2)]
        public static void CreateAll()
        {
            s_cache.Clear();
            Lit("Concrete", new Color(0.55f, 0.55f, 0.53f), 0.3f);
            Lit("ConcreteDark", new Color(0.3f, 0.3f, 0.31f), 0.35f);
            Lit("Asphalt", new Color(0.12f, 0.12f, 0.13f), 0.55f);
            Lit("WetAsphalt", new Color(0.06f, 0.06f, 0.07f), 0.88f);
            Lit("Cobblestone", new Color(0.28f, 0.26f, 0.24f), 0.7f);
            Lit("Brick", new Color(0.45f, 0.22f, 0.16f), 0.25f);
            Lit("Plaster", new Color(0.86f, 0.74f, 0.55f), 0.2f);
            Lit("PlasterPink", new Color(0.85f, 0.58f, 0.5f), 0.2f);
            Lit("PlasterBlue", new Color(0.55f, 0.68f, 0.78f), 0.2f);
            Lit("Terracotta", new Color(0.66f, 0.33f, 0.2f), 0.2f);
            Lit("Marble", new Color(0.9f, 0.89f, 0.86f), 0.92f);
            Lit("MarbleDark", new Color(0.12f, 0.12f, 0.13f), 0.95f);
            Lit("Gold", new Color(0.83f, 0.66f, 0.3f), 0.85f, 1f);
            Lit("Snow", new Color(0.93f, 0.95f, 0.98f), 0.45f);
            Lit("Rock", new Color(0.4f, 0.4f, 0.42f), 0.25f);
            Lit("Wood", new Color(0.4f, 0.26f, 0.15f), 0.35f);
            Lit("Metal", new Color(0.55f, 0.56f, 0.58f), 0.7f, 1f);
            Lit("Glass", new Color(0.2f, 0.3f, 0.35f), 0.98f);
            Lit("Water", new Color(0.05f, 0.18f, 0.25f), 0.97f);
            Lit("Grass", new Color(0.25f, 0.38f, 0.16f), 0.15f);
            Lit("Sand", new Color(0.82f, 0.74f, 0.56f), 0.15f);
            Lit("Hedge", new Color(0.15f, 0.28f, 0.12f), 0.1f);
            Lit("Fabric", new Color(0.5f, 0.1f, 0.12f), 0.1f);
            Lit("Container", new Color(0.2f, 0.35f, 0.55f), 0.4f, 0.5f);
            Lit("ContainerRed", new Color(0.6f, 0.15f, 0.1f), 0.4f, 0.5f);
            Lit("PlayerSuit", new Color(0.12f, 0.12f, 0.15f), 0.4f);
            Lit("EnemySuit", new Color(0.35f, 0.08f, 0.08f), 0.4f);
            Lit("TeamSuit", new Color(0.05f, 0.05f, 0.05f), 0.6f);
            Lit("Civilian", new Color(0.45f, 0.5f, 0.55f), 0.2f);
            Lit("Skin", new Color(0.75f, 0.58f, 0.46f), 0.3f);
            Lit("CarBlack", new Color(0.03f, 0.03f, 0.035f), 0.9f, 0.6f);
            Lit("CarRed", new Color(0.55f, 0.04f, 0.04f), 0.85f, 0.5f);
            Lit("CarSilver", new Color(0.6f, 0.62f, 0.65f), 0.85f, 0.8f);
            Lit("CarBlue", new Color(0.1f, 0.2f, 0.45f), 0.85f, 0.5f);
            Lit("Tire", new Color(0.04f, 0.04f, 0.04f), 0.2f);
            Lit("Trim", new Color(0.72f, 0.69f, 0.62f), 0.25f);
            Lit("WindowGlass", new Color(0.04f, 0.05f, 0.06f), 0.96f);
            Emissive("LampWarm", new Color(1f, 0.75f, 0.45f), 8f);
            Emissive("NeonPink", new Color(1f, 0.1f, 0.6f), 20f);
            Emissive("NeonCyan", new Color(0.1f, 0.9f, 1f), 20f);
            Emissive("WindowLit", new Color(1f, 0.6f, 0.3f), 4f);
            Emissive("ScreenGreen", new Color(0.2f, 1f, 0.4f), 6f);
            Emissive("Tracer", new Color(1f, 0.8f, 0.4f), 30f);
            Emissive("Laser", new Color(1f, 0.05f, 0.05f), 40f);
            Lit("Evidence", new Color(0.35f, 0.02f, 0.02f), 0.8f);
            Transparent("VisionCone", new Color(1f, 1f, 1f, 0.15f));
            Transparent("Rain", new Color(0.7f, 0.75f, 0.8f, 0.35f));
            AssetDatabase.SaveAssets();
        }

        public static Material Get(string name)
        {
            if (s_cache.TryGetValue(name, out var m) && m != null) return m;
            m = AssetDatabase.LoadAssetAtPath<Material>($"{Folder}/{name}.mat");
            if (m == null) { CreateAll(); m = AssetDatabase.LoadAssetAtPath<Material>($"{Folder}/{name}.mat"); }
            s_cache[name] = m;
            return m;
        }

        static Material LoadOrCreate(string name, string shader)
        {
            Directory.CreateDirectory(Folder);
            string path = $"{Folder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find(shader));
                AssetDatabase.CreateAsset(mat, path);
            }
            else if (mat.shader.name != shader) mat.shader = Shader.Find(shader);
            s_cache[name] = mat;
            return mat;
        }

        static void Lit(string name, Color color, float smoothness, float metallic = 0f)
        {
            var m = LoadOrCreate(name, "HDRP/Lit");
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Metallic", metallic);
            m.enableInstancing = true;
            ApplyTextures(m, name);
            HDMaterial.ValidateMaterial(m);
            EditorUtility.SetDirty(m);
        }

        [MenuItem("Exit Clause/Art/Apply Textures (re-skins every scene)", priority = 42)]
        static void ApplyTexturesMenu()
        {
            CreateAll();
            Debug.Log($"[MaterialLibrary] Materials refreshed from {TextureRoot}. No scene rebuild needed.");
        }

        public const string TextureRoot = "Assets/ThirdParty/Textures";
        const float TileMeters = 2f;

        /// <summary>
        /// If Assets/ThirdParty/Textures/&lt;MaterialName&gt;/ holds a colour map (and optionally a normal map),
        /// e.g. Poly Haven's *_diff_2k.jpg and *_nor_gl_2k.jpg, uses them with world-space triplanar mapping so
        /// they tile at real-world size on the stretched greybox boxes. Without the folder the flat colour stays.
        /// </summary>
        static void ApplyTextures(Material m, string name)
        {
            string dir = $"{TextureRoot}/{name}";
            if (!AssetDatabase.IsValidFolder(dir))
            {
                m.SetTexture("_BaseColorMap", null);
                m.SetTexture("_NormalMap", null);
                m.SetFloat("_UVBase", 0f);
                return;
            }
            Texture2D baseMap = null, normal = null;
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { dir }))
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                string f = Path.GetFileNameWithoutExtension(p).ToLowerInvariant();
                if (f.Contains("nor_dx")) continue; // Poly Haven ships DirectX and OpenGL normals; Unity wants OpenGL
                if (f.Contains("_diff") || f.Contains("albedo") || f.Contains("basecolor") || f.Contains("_col")) baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                else if (f.Contains("nor_gl") || f.Contains("normal"))
                {
                    var imp = (TextureImporter)AssetImporter.GetAtPath(p);
                    if (imp.textureType != TextureImporterType.NormalMap) { imp.textureType = TextureImporterType.NormalMap; imp.SaveAndReimport(); }
                    normal = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                }
            }
            if (baseMap == null) return;
            // Variants sharing one texture (ConcreteDark, WetAsphalt) keep a darker tint so they still read differently.
            m.SetColor("_BaseColor", name.Contains("Dark") || name.StartsWith("Wet") ? new Color(0.5f, 0.5f, 0.52f) : Color.white);
            m.SetTexture("_BaseColorMap", baseMap);
            m.SetTexture("_NormalMap", normal);
            m.SetFloat("_UVBase", 5f);            // UVBaseMapping.Triplanar
            m.SetFloat("_ObjectSpaceUVMapping", 0f); // world space
            m.SetFloat("_TexWorldScale", 1f / TileMeters);
        }

        static void Emissive(string name, Color color, float intensity)
        {
            var m = LoadOrCreate(name, "HDRP/Lit");
            m.SetColor("_BaseColor", color);
            HDMaterial.SetUseEmissiveIntensity(m, true);
            HDMaterial.SetEmissiveColor(m, color);
            HDMaterial.SetEmissiveIntensity(m, intensity, EmissiveIntensityUnit.EV100);
            HDMaterial.ValidateMaterial(m);
            EditorUtility.SetDirty(m);
        }

        static void Transparent(string name, Color color)
        {
            var m = LoadOrCreate(name, "HDRP/Unlit");
            m.SetColor("_UnlitColor", color);
            HDMaterial.SetSurfaceType(m, true);
            HDMaterial.ValidateMaterial(m);
            EditorUtility.SetDirty(m);
        }
    }
}
