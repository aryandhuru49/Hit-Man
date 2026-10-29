using System.Collections.Generic;
using UnityEngine;

namespace ExitClause.Systems
{
    public enum NoiseType { Footstep, Gunshot, Distraction, Takedown, Impact, Vehicle, Alarm }

    /// <summary>Anything that can hear noises (enemy agents, civilians).</summary>
    public interface INoiseListener
    {
        Vector3 ListenerPosition { get; }
        void HearNoise(Vector3 position, float loudness, NoiseType type, GameObject source);
    }

    /// <summary>
    /// Global hearing system. Gameplay calls Emit with a radius; every registered listener inside
    /// that radius receives the noise with a loudness falling off by distance.
    /// </summary>
    public static class NoiseSystem
    {
        static readonly List<INoiseListener> s_listeners = new List<INoiseListener>();

        public static void Register(INoiseListener l) { if (!s_listeners.Contains(l)) s_listeners.Add(l); }
        public static void Unregister(INoiseListener l) => s_listeners.Remove(l);

        public static void Emit(Vector3 position, float radius, NoiseType type, GameObject source)
        {
            for (int i = s_listeners.Count - 1; i >= 0; i--)
            {
                var l = s_listeners[i];
                if (l == null) { s_listeners.RemoveAt(i); continue; }
                float d = Vector3.Distance(position, l.ListenerPosition);
                if (d <= radius) l.HearNoise(position, 1f - d / radius, type, source);
            }
        }
    }
}
