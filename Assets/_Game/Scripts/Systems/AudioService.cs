using UnityEngine;

namespace ExitClause.Systems
{
    /// <summary>
    /// Minimal audio helper: 3D one-shots, a 2D UI/voice source and a music source.
    /// Every call is null-safe so the greybox runs without any audio assets.
    /// </summary>
    public class AudioService : MonoBehaviour
    {
        public static AudioService Instance { get; private set; }
        AudioSource m_voice;
        AudioSource m_music;

        void Awake()
        {
            Instance = this;
            m_voice = gameObject.AddComponent<AudioSource>();
            m_voice.playOnAwake = false;
            m_music = gameObject.AddComponent<AudioSource>();
            m_music.loop = true;
            m_music.volume = 0.5f;
        }

        public void PlayAt(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip != null) AudioSource.PlayClipAtPoint(clip, position, volume);
        }

        public void PlayVoice(AudioClip clip)
        {
            m_voice.Stop();
            if (clip == null) return;
            m_voice.clip = clip;
            m_voice.Play();
        }

        public void StopVoice() => m_voice.Stop();

        public void PlayMusic(AudioClip clip)
        {
            if (m_music.clip == clip) return;
            m_music.clip = clip;
            if (clip != null) m_music.Play(); else m_music.Stop();
        }
    }
}
