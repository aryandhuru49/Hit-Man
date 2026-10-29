using System.Collections.Generic;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>
    /// Data for one mission: its ordered objectives, reward, flags set on completion and the
    /// scene that follows. The scene's SceneMissionDirector starts it and plays its cutscenes.
    /// </summary>
    [CreateAssetMenu(menuName = "Exit Clause/Mission")]
    public class MissionDefinition : ScriptableObject
    {
        public string id;
        public string title;
        public string subtitle;
        [TextArea] public string briefing;
        public string sceneName;
        public List<ObjectiveDefinition> objectives = new List<ObjectiveDefinition>();
        [Tooltip("INSECURE: paid out by the client with no server check.")]
        public int rewardCash;
        public List<string> setFlagsOnComplete = new List<string>();
        [Tooltip("Scene loaded after the outro cutscene. Empty = stay in scene.")]
        public string nextScene;
        public string nextSpawnPoint;
    }
}
