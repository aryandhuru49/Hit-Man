using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ExitClause.EditorTools
{
    /// <summary>
    /// Menu entry points. "Build Everything" regenerates the whole game from code: project settings,
    /// materials, data, prefabs and every greybox scene. Also callable headless:
    /// Unity.exe -batchmode -projectPath ... -executeMethod ExitClause.EditorTools.BuildEverything.Run -quit
    /// </summary>
    public static class BuildEverything
    {
        [MenuItem("Exit Clause/Build Everything (setup, data, prefabs, all scenes)", priority = 0)]
        public static void Run()
        {
            ProjectSetup.Run();
            MaterialLibrary.CreateAll();
            DataBuilder.BuildAll();
            PrefabBuilder.BuildAll();
            BuildAllScenes();
        }

        [MenuItem("Exit Clause/5. Build All Scenes", priority = 5)]
        public static void BuildAllScenes()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildMainMenu.Build();
            BuildPrologue.Build();
            BuildRiviera.Build();
            BuildAct1.BuildMansion();
            BuildAct1.BuildCoastRoad();
            BuildInterlude.Build();
            BuildAct2.BuildSafehouse();
            BuildAct2.BuildAlps();
            BuildAct2.BuildMarseille();
            BuildAct2.BuildBerlin();
            BuildZurich.Build();
            BuildMainMenu.BuildEnding();
            ProjectSetup.SetupBuildScenes();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/MainMenu.unity");
            Debug.Log("[ExitClause] All scenes built.");
        }

        [MenuItem("Exit Clause/Scenes/Main Menu + Ending")] static void M0() { BuildMainMenu.Build(); BuildMainMenu.BuildEnding(); }
        [MenuItem("Exit Clause/Scenes/Prologue Prague")] static void M1() => BuildPrologue.Build();
        [MenuItem("Exit Clause/Scenes/Act 0 Riviera")] static void M2() => BuildRiviera.Build();
        [MenuItem("Exit Clause/Scenes/Act 1 Mansion")] static void M3() => BuildAct1.BuildMansion();
        [MenuItem("Exit Clause/Scenes/Act 1 Coast Road")] static void M4() => BuildAct1.BuildCoastRoad();
        [MenuItem("Exit Clause/Scenes/Interlude Harbor")] static void M5() => BuildInterlude.Build();
        [MenuItem("Exit Clause/Scenes/Act 2 Safehouse")] static void M6() => BuildAct2.BuildSafehouse();
        [MenuItem("Exit Clause/Scenes/Act 2 Alps")] static void M7() => BuildAct2.BuildAlps();
        [MenuItem("Exit Clause/Scenes/Act 2 Marseille")] static void M8() => BuildAct2.BuildMarseille();
        [MenuItem("Exit Clause/Scenes/Act 2 Berlin")] static void M9() => BuildAct2.BuildBerlin();
        [MenuItem("Exit Clause/Scenes/Act 3 Zurich")] static void M10() => BuildZurich.Build();

        [MenuItem("Exit Clause/Open Save Folder")]
        static void OpenSaves() => EditorUtility.RevealInFinder(Application.persistentDataPath);

        [MenuItem("Exit Clause/Delete All Saves")]
        static void DeleteSaves()
        {
            var dir = System.IO.Path.Combine(Application.persistentDataPath, "saves");
            if (System.IO.Directory.Exists(dir)) System.IO.Directory.Delete(dir, true);
            Debug.Log("[ExitClause] Saves deleted.");
        }
    }
}
