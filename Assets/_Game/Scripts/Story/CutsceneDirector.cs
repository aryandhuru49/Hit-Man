using System;
using System.Collections;
using ExitClause.Player;
using ExitClause.Systems;
using ExitClause.UI;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace ExitClause.Story
{
    /// <summary>
    /// Plays a story beat. With a Timeline (PlayableDirector) it lets Timeline drive Cinemachine shots
    /// and subtitle clips; without one it falls back to cycling the listed shot cameras while the
    /// dialogue plays. Gameplay input and HUD are suspended, letterbox shown, Enter/Space skips.
    /// When it ends the brain blends back to the gameplay camera.
    /// </summary>
    public class CutsceneDirector : MonoBehaviour
    {
        public PlayableDirector director;
        [Tooltip("Used when there is no Timeline, and as the source for the Timeline subtitle clips.")]
        public DialogueSequence dialogue;
        public CinemachineCamera[] shots;
        [Tooltip("Lets the scene director find this outro for branch missions.")]
        public string playAfterMissionId;
        public bool skippable = true;
        [Tooltip("Move the player here when the cutscene ends (optional).")]
        public Transform placePlayerAfter;

        public bool IsPlaying { get; private set; }
        Action m_onDone;
        bool m_skip;

        void Awake()
        {
            if (director == null) director = GetComponent<PlayableDirector>();
            if (director != null) director.playOnAwake = false;
            foreach (var s in shots) if (s != null) s.Priority = 0;
        }

        public void Play(Action onDone)
        {
            if (IsPlaying) return;
            m_onDone = onDone;
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            IsPlaying = true;
            m_skip = false;
            var player = PlayerCharacter.Instance;
            if (player != null) player.InputEnabled = false;
            GameInput.PushBlock();
            var ui = UIRoot.Instance;
            ui.HUD.SetVisible(false);
            ui.Letterbox.Show(skippable);

            if (director != null && director.playableAsset != null)
            {
                director.time = 0;
                director.Play();
                yield return null;
                while (director.state == PlayState.Playing && !m_skip) yield return null;
                director.Stop();
            }
            else
            {
                yield return RunFallback();
            }

            foreach (var s in shots) if (s != null) s.Priority = 0;
            ui.Subtitles.Clear();
            AudioService.Instance?.StopVoice();
            ui.Letterbox.Hide();
            ui.HUD.SetVisible(true);
            GameInput.PopBlock();
            if (player != null)
            {
                player.InputEnabled = true;
                if (placePlayerAfter != null) player.Teleport(placePlayerAfter.position, placePlayerAfter.rotation);
            }
            IsPlaying = false;
            var cb = m_onDone;
            m_onDone = null;
            cb?.Invoke();
        }

        IEnumerator RunFallback()
        {
            if (dialogue == null || dialogue.lines.Count == 0) { yield return new WaitForSeconds(1f); yield break; }
            for (int i = 0; i < dialogue.lines.Count && !m_skip; i++)
            {
                if (shots.Length > 0)
                {
                    for (int s = 0; s < shots.Length; s++) if (shots[s] != null) shots[s].Priority = s == i % shots.Length ? 100 : 0;
                }
                var line = dialogue.lines[i];
                UIRoot.Instance.Subtitles.Show(line);
                AudioService.Instance?.PlayVoice(line.voice);
                float t = 0;
                while (t < line.duration && !m_skip)
                {
                    t += Time.deltaTime;
                    yield return null;
                }
            }
        }

        void Update()
        {
            if (!IsPlaying || !skippable) return;
            // Cutscenes run while gameplay input is blocked, so read Skip directly.
            if (GameInput.Skip != null && GameInput.Skip.WasPressedThisFrame() && !PauseController.Instance.IsPaused) m_skip = true;
        }
    }
}
