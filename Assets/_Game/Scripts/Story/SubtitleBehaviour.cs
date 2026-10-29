using ExitClause.Systems;
using ExitClause.UI;
using UnityEngine;
using UnityEngine.Playables;

namespace ExitClause.Story
{
    /// <summary>Runtime half of SubtitleClip: pushes the line to the subtitle view while the clip is active.</summary>
    public class SubtitleBehaviour : PlayableBehaviour
    {
        public string speaker;
        public string text;
        public AudioClip voice;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (!Application.isPlaying) return;
            UIRoot.Instance?.Subtitles?.Show(speaker, text);
            AudioService.Instance?.PlayVoice(voice);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (!Application.isPlaying) return;
            UIRoot.Instance?.Subtitles?.Clear();
        }
    }
}
