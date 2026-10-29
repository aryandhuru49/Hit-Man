using System.Collections.Generic;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>World position shown on the radar and as an on-screen waypoint while its objective is active.</summary>
    public class ObjectiveMarker : MonoBehaviour
    {
        public static readonly List<ObjectiveMarker> All = new List<ObjectiveMarker>();

        public string objectiveId;
        public string label;

        void OnEnable() => All.Add(this);
        void OnDisable() => All.Remove(this);

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up, new Vector3(0.6f, 2f, 0.6f));
        }
    }
}
