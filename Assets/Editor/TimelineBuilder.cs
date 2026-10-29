using System.IO;
using ExitClause.Story;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ExitClause.EditorTools
{
    /// <summary>
    /// Generates a Timeline asset for a cutscene: a Cinemachine track (one shot per line, cycling the
    /// given cameras) bound to the scene's CinemachineBrain, and a Subtitle track with the dialogue.
    /// Open the asset in Window > Sequencing > Timeline to re-time it or drop in recorded voice clips.
    /// </summary>
    public static class TimelineBuilder
    {
        const string Folder = "Assets/_Game/Timelines";

        public static TimelineAsset Build(string name, PlayableDirector director, DialogueSequence dialogue, CinemachineCamera[] shots, CinemachineBrain brain)
        {
            Directory.CreateDirectory(Folder);
            string path = $"{Folder}/{name}.playable";
            if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
            var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timeline, path);

            var camTrack = timeline.CreateTrack<CinemachineTrack>(null, "Cameras");
            var subTrack = timeline.CreateTrack<SubtitleTrack>(null, "Subtitles");

            double t = 0;
            int lineCount = dialogue != null ? dialogue.lines.Count : 0;
            int count = Mathf.Max(1, lineCount);
            for (int i = 0; i < count; i++)
            {
                var line = lineCount > 0 ? dialogue.lines[i] : new DialogueLine("", "", 3f);
                double dur = line.duration;

                if (shots != null && shots.Length > 0)
                {
                    var clip = camTrack.CreateClip<CinemachineShot>();
                    clip.start = t;
                    clip.duration = dur;
                    var shot = (CinemachineShot)clip.asset;
                    var cam = shots[i % shots.Length];
                    shot.DisplayName = cam.name;
                    shot.VirtualCamera.exposedName = GUID.Generate().ToString();
                    director.SetReferenceValue(shot.VirtualCamera.exposedName, cam);
                    clip.displayName = cam.name;
                }

                if (!string.IsNullOrEmpty(line.text))
                {
                    var sc = subTrack.CreateClip<SubtitleClip>();
                    sc.start = t;
                    sc.duration = dur;
                    var sub = (SubtitleClip)sc.asset;
                    sub.speaker = line.speaker;
                    sub.text = line.text;
                    sub.voice = line.voice;
                    sc.displayName = line.speaker + ": " + line.text;
                }
                t += dur;
            }

            director.playableAsset = timeline;
            director.SetGenericBinding(camTrack, brain);
            EditorUtility.SetDirty(timeline);
            EditorUtility.SetDirty(director);
            AssetDatabase.SaveAssets();
            return timeline;
        }
    }
}
