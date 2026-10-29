using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ExitClause.Player;
using ExitClause.Story;
using ExitClause.Systems;
using ExitClause.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ExitClause.EditorTools
{
    /// <summary>
    /// Automated story playthrough for catching runtime errors. Enters Play mode in the Prologue, then
    /// force-completes each objective in turn, auto-picks choices, skips cutscenes, travels between the
    /// Act 2 targets, and stops when the Ending scene is reached. Every error/exception is written to
    /// Logs/smoke_report.txt. Run from the menu or headless with -executeMethod ExitClause.EditorTools.SmokeTest.Run
    /// </summary>
    public static class SmokeTest
    {
        static readonly List<string> s_report = new List<string>();
        static int s_errors;
        static double s_start, s_next, s_sceneStart;
        static string s_scene;
        static bool s_running;
        static bool s_batch;

        [MenuItem("Exit Clause/Run Smoke Test (auto playthrough)")]
        public static void RunFromMenu() => Begin(false);

        public static void Run() => Begin(true);

        static void Begin(bool batch)
        {
            s_batch = batch;
            s_report.Clear();
            s_errors = 0;
            s_scene = null;
            s_running = true;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prologue_Prague.unity");
            Application.logMessageReceived += OnLog;
            EditorApplication.update += Tick;
            s_start = EditorApplication.timeSinceStartup;
            EditorApplication.EnterPlaymode();
        }

        static void OnLog(string msg, string stack, LogType type)
        {
            if (stack != null && stack.Contains("UnityEditor.Search")) return; // Editor indexer noise, not game code
            if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
            {
                s_errors++;
                s_report.Add($"[{s_scene}] {type}: {msg}\n{stack}");
            }
        }

        static void Tick()
        {
            if (!s_running) return;
            double now = EditorApplication.timeSinceStartup;
            if (now - s_start > 1500) { Finish("TIMEOUT"); return; }
            if (!EditorApplication.isPlaying) return;

            var scene = SceneManager.GetActiveScene().name;
            if (scene != s_scene)
            {
                s_scene = scene;
                s_sceneStart = now;
                s_report.Add($"== Scene {scene} at {now - s_start:F0}s");
            }
            if (scene == "Ending")
            {
                if (now - s_sceneStart > 6) Finish("Reached Ending: " + GameSession.Instance.Story.GetChoice(StoryKeys.ChoiceEnding));
                return;
            }
            if (SceneLoader.Instance == null || SceneLoader.Instance.IsLoading) return;
            if (now - s_sceneStart > 240) { Finish("STUCK in " + scene + " on " + MissionManager.Instance.CurrentObjective?.id); return; }

            var p = PlayerCharacter.Instance;
            if (p != null) { p.Health.invulnerable = true; p.Health.current = p.Health.maxHealth; }
            if (!UIRoot.Instance.Choice.IsOpen && !(PauseController.Instance?.IsPaused ?? false)) Time.timeScale = 4f;

            if (UIRoot.Instance.Choice.IsOpen) { PickChoice(); return; }
            foreach (var cs in Object.FindObjectsByType<CutsceneDirector>(FindObjectsInactive.Exclude))
                if (cs.IsPlaying) typeof(CutsceneDirector).GetField("m_skip", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(cs, true);
            if (UIRoot.Instance.GameOver.IsOpen) { Finish("GAME OVER shown in " + scene); return; }

            if (now < s_next) return;
            s_next = now + 0.35;
            var mm = MissionManager.Instance;
            if (mm.Current == null || mm.IsComplete) return;
            var o = mm.CurrentObjective;
            if (o == null) return;
            if (o.id == "travel") { Travel(); return; }
            s_report.Add($"   {mm.Current.id}/{o.id}");
            mm.ForceComplete(o.id);
        }

        static void PickChoice()
        {
            var screen = UIRoot.Instance.Choice;
            var def = (ChoiceDefinition)typeof(ChoiceScreen).GetField("m_choice", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(screen);
            ChoiceOption pick = null;
            foreach (var opt in def.options) if (StoryChoices.IsAvailable(opt)) { pick = opt; break; }
            s_report.Add($"   choice {def.id} -> {pick?.id}");
            typeof(ChoiceScreen).GetMethod("Pick", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(screen, new object[] { pick });
        }

        static void Travel()
        {
            var s = GameSession.Instance.Story;
            string next = !s.HasFlag(StoryKeys.SniperDefeated) ? "Act2_Alps"
                : !s.HasFlag(StoryKeys.InfiltratorDefeated) ? "Act2_Marseille"
                : !s.HasFlag(StoryKeys.TechDefeated) ? "Act2_Berlin" : "Act3_Zurich";
            s_report.Add("   travel -> " + next);
            SceneLoader.Instance.Load(next);
        }

        static void Finish(string result)
        {
            s_running = false;
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= OnLog;
            s_report.Insert(0, $"RESULT: {result}\nERRORS: {s_errors}\n");
            Directory.CreateDirectory("Logs");
            File.WriteAllLines("Logs/smoke_report.txt", s_report);
            Debug.Log("[SmokeTest] " + result + " errors=" + s_errors);
            Time.timeScale = 1f;
            EditorApplication.ExitPlaymode();
            EditorSettings.enterPlayModeOptionsEnabled = false;
            if (s_batch) EditorApplication.Exit(s_errors == 0 && result.StartsWith("Reached") ? 0 : 1);
        }
    }
}
