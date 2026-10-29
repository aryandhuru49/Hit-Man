using System;
using System.Collections;
using ExitClause.Systems;
using ExitClause.UI;
using UnityEngine;

namespace ExitClause.Story
{
    /// <summary>Plays a DialogueSequence as timed subtitles during gameplay (radio calls, conversations).</summary>
    public class DialogueRunner : MonoBehaviour
    {
        static DialogueRunner s_instance;
        Coroutine m_current;

        static DialogueRunner Get()
        {
            if (s_instance == null)
            {
                s_instance = new GameObject("[DialogueRunner]").AddComponent<DialogueRunner>();
            }
            return s_instance;
        }

        public static void Play(DialogueSequence seq, Action onDone = null)
        {
            if (seq == null) { onDone?.Invoke(); return; }
            var r = Get();
            if (r.m_current != null) r.StopCoroutine(r.m_current);
            r.m_current = r.StartCoroutine(r.Run(seq, onDone));
        }

        IEnumerator Run(DialogueSequence seq, Action onDone)
        {
            foreach (var line in seq.lines)
            {
                UIRoot.Instance.Subtitles.Show(line);
                AudioService.Instance?.PlayVoice(line.voice);
                yield return new WaitForSeconds(line.duration);
            }
            UIRoot.Instance.Subtitles.Clear();
            m_current = null;
            onDone?.Invoke();
        }
    }
}
