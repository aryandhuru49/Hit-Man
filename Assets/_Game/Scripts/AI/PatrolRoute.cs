using UnityEngine;

namespace ExitClause.AI
{
    /// <summary>Ordered patrol waypoints (the children of this object) with an optional pause at each.</summary>
    public class PatrolRoute : MonoBehaviour
    {
        public bool loop = true;
        public float waitAtPoint = 2f;

        public int Count => transform.childCount;
        public Vector3 Get(int i) => transform.GetChild(Mathf.Clamp(i, 0, Count - 1)).position;

        public int Next(int i, ref int direction)
        {
            if (Count <= 1) return 0;
            if (loop) return (i + 1) % Count;
            int n = i + direction;
            if (n >= Count || n < 0) { direction = -direction; n = i + direction; }
            return n;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f);
            for (int i = 0; i < Count; i++)
            {
                Gizmos.DrawSphere(Get(i), 0.2f);
                if (i + 1 < Count) Gizmos.DrawLine(Get(i), Get(i + 1));
                else if (loop && Count > 1) Gizmos.DrawLine(Get(i), Get(0));
            }
        }
    }
}
