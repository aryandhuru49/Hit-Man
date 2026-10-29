using UnityEngine.Timeline;

namespace ExitClause.Story
{
    /// <summary>Timeline track holding SubtitleClips.</summary>
    [TrackColor(0.9f, 0.8f, 0.3f)]
    [TrackClipType(typeof(SubtitleClip))]
    public class SubtitleTrack : TrackAsset
    {
    }
}
