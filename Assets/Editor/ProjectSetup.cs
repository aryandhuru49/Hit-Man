using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace ExitClause.EditorTools
{
    /// <summary>
    /// One-click project configuration: physics layers, three HDRP quality tiers (Low/Medium/High,
    /// each with its own pipeline asset), linear colour, Mono scripting backend (deliberately easy to
    /// decompile), product name and build scene list.
    /// </summary>
    public static class ProjectSetup
    {
        public static readonly string[] SceneOrder =
        {
            "MainMenu", "Prologue_Prague", "Act0_Riviera", "Act1_Mansion", "Act1_CoastRoad", "Interlude_Harbor",
            "Act2_Safehouse", "Act2_Alps", "Act2_Marseille", "Act2_Berlin", "Act3_Zurich", "Ending"
        };

        [MenuItem("Exit Clause/1. Project Setup", priority = 1)]
        public static void Run()
        {
            SetupLayers();
            SetupRenderPipeline();
            SetupPlayer();
            SetupBuildScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[ExitClause] Project setup complete.");
        }

        static void SetupLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            void Set(int i, string n) { layers.GetArrayElementAtIndex(i).stringValue = n; }
            Set(8, "Player");
            Set(9, "Character");
            Set(10, "Vehicle");
            Set(11, "Props");
            Set(12, "Trigger");
            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        static HDRenderPipelineAsset CreatePipelineAsset(string name, int shadowAtlas, bool volumetrics, bool ssao, bool ssr)
        {
            Directory.CreateDirectory("Assets/_Game/Settings");
            string path = $"Assets/_Game/Settings/HDRP_{name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<HDRenderPipelineAsset>();
                AssetDatabase.CreateAsset(asset, path);
            }
            var s = asset.currentPlatformRenderPipelineSettings;
            s.supportVolumetrics = volumetrics;
            s.supportSSAO = ssao;
            s.supportSSR = ssr;
            s.supportSSGI = false;
            s.supportRayTracing = false;
            s.supportVolumetricClouds = false;
            s.hdShadowInitParams.punctualLightShadowAtlas.shadowAtlasResolution = shadowAtlas;
            s.hdShadowInitParams.maxShadowRequests = shadowAtlas >= 4096 ? 64 : 32;
            asset.currentPlatformRenderPipelineSettings = s;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        static void SetupRenderPipeline()
        {
            var low = CreatePipelineAsset("Low", 1024, false, false, false);
            var med = CreatePipelineAsset("Medium", 2048, true, true, false);
            var high = CreatePipelineAsset("High", 4096, true, true, true);
            GraphicsSettings.defaultRenderPipeline = med;

            var qs = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset")[0]);
            var levels = qs.FindProperty("m_QualitySettings");
            levels.arraySize = 3;
            string[] names = { "Low", "Medium", "High" };
            HDRenderPipelineAsset[] assets = { low, med, high };
            float[] lodBias = { 0.6f, 1f, 1.5f };
            for (int i = 0; i < 3; i++)
            {
                var e = levels.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("name").stringValue = names[i];
                e.FindPropertyRelative("customRenderPipeline").objectReferenceValue = assets[i];
                e.FindPropertyRelative("lodBias").floatValue = lodBias[i];
                var vsync = e.FindPropertyRelative("vSyncCount");
                if (vsync != null) vsync.intValue = 1;
            }
            qs.FindProperty("m_CurrentQuality").intValue = 1;
            var perPlatform = qs.FindProperty("m_PerPlatformDefaultQuality");
            if (perPlatform != null)
                for (int i = 0; i < perPlatform.arraySize; i++)
                    perPlatform.GetArrayElementAtIndex(i).FindPropertyRelative("second").intValue = 1;
            qs.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetupPlayer()
        {
            PlayerSettings.companyName = "ExitClause";
            PlayerSettings.productName = "Exit Clause";
            PlayerSettings.colorSpace = ColorSpace.Linear;
            // INSECURE BY DESIGN: Mono keeps Assembly-CSharp.dll as IL that dnSpy/ILSpy decompile to near source.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.runInBackground = true;
        }

        public static void SetupBuildScenes()
        {
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            foreach (var s in SceneOrder)
            {
                string path = $"Assets/_Game/Scenes/{s}.unity";
                if (File.Exists(path)) list.Add(new EditorBuildSettingsScene(path, true));
            }
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
