using ExitClause.Systems;
using UnityEngine;

namespace ExitClause.CameraSystem
{
    /// <summary>
    /// Sits on the camera's rain particle system and stops the rain while the camera is under a roof,
    /// so interiors stay dry. Drops that are already falling outside a window keep falling.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class RainShelter : MonoBehaviour
    {
        public float checkHeight = 40f;
        ParticleSystem m_rain;
        bool m_sheltered;

        void Awake() => m_rain = GetComponent<ParticleSystem>();

        void Update()
        {
            var cam = transform.parent != null ? transform.parent.position : transform.position;
            bool sheltered = Physics.Raycast(cam, Vector3.up, checkHeight, Layers.EnvironmentMask, QueryTriggerInteraction.Ignore);
            if (sheltered == m_sheltered) return;
            m_sheltered = sheltered;
            var emission = m_rain.emission;
            emission.enabled = !sheltered;
            // Drops spawned above the camera would fall into the room, so clear them on the way in.
            if (sheltered) m_rain.Clear();
        }
    }
}
