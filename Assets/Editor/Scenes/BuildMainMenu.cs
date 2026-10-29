using ExitClause.Story;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.EditorTools
{
    /// <summary>Main menu backdrop (a Riviera vista at golden hour) and the Ending scene.</summary>
    public static class BuildMainMenu
    {
        public static void Build()
        {
            GB.NewScene("MainMenu", MoodPreset.RivieraGolden);
            Vista();
            GB.MainCam.transform.SetPositionAndRotation(new Vector3(-30, 9, -40), Quaternion.Euler(6, 35, 0));
            new GameObject("MainMenu").AddComponent<MainMenuController>();
            GB.Save();
        }

        public static void BuildEnding()
        {
            GB.NewScene("Ending", MoodPreset.RivieraGolden);
            Vista();
            GB.MainCam.transform.SetPositionAndRotation(new Vector3(40, 5, -50), Quaternion.Euler(2, -20, 0));
            new GameObject("EndingController").AddComponent<EndingController>();
            GB.Save();
        }

        static void Vista()
        {
            GB.Box("Sea", new Vector3(0, -1.5f, -200), new Vector3(800, 1, 400), "Water");
            GB.Ground(new Vector3(0, 0, 150), new Vector2(800, 300), "Grass");
            GB.Road(new Vector3(-300, 0, 5), new Vector3(300, 0, 5), 8);
            string[] colors = { "Plaster", "PlasterPink", "PlasterBlue", "Terracotta" };
            for (int i = 0; i < 18; i++)
            {
                float x = -160 + i * 19 + (i % 3) * 3;
                float h = 8 + (i * 7 % 5) * 2.5f;
                GB.Building("House" + i, new Vector3(x, (i % 4) * 2.5f, 30 + (i % 3) * 12), new Vector3(12, h, 10), colors[i % colors.Length]);
            }
            GB.Box("Headland", new Vector3(140, 10, -60), new Vector3(60, 30, 80), "Rock");
        }
    }
}
