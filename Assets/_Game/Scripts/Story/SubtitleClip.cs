using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ExitClause.Story
{
    /// <summary>Timeline clip that shows one subtitle line (and plays its placeholder voice clip) for its duration.</summary>
    [System.Serializable]
    public class SubtitleClip : PlayableAsset, ITimelineClipAsset
    {
        public string speaker;
        [TextArea] public string text;
        public AudioClip voice;

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<SubtitleBehaviour>.Create(graph);
            var b = playable.GetBehaviour();
            b.speaker = speaker;
            b.text = text;
            b.voice = voice;
            return playable;
        }
    }
}
