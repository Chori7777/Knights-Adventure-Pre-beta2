using UnityEngine;
using System;
using System.Collections;

public class TrueFinalBossMusicSync : MonoBehaviour
{
    [System.Serializable]
    public class TimelineEvent
    {
        public float time;
        public string name;
        public bool isSpecialState;
        public TrueFinalBossStateMachine.BossState specialState;
        public bool pauseMusic;
        public float pauseDuration;
        public AudioClip sfx;
        [HideInInspector] public bool fired;
    }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private TimelineEvent[] events;
    [SerializeField] private bool autoStartOnMusicPlay = true;
    [SerializeField] private bool resetOnMusicLoop = true;

    public event Action<string> OnZoneEvent;
    public event Action<TrueFinalBossStateMachine.BossState> OnStateEvent;
    public event Action OnMusicPaused;
    public event Action OnMusicResumed;
    public event Action<float> OnPauseBegin;

    private float lastMusicTime;

    private void Awake()
    {
        enabled = autoStartOnMusicPlay;
    }

    private void Update()
    {
        if (musicSource == null || events == null) return;
        float t = musicSource.time;
        if (autoStartOnMusicPlay && !enabled && musicSource.isPlaying)
        {
            enabled = true;
            lastMusicTime = t;
        }
        if (resetOnMusicLoop && musicSource.loop)
        {
            if (t < lastMusicTime)
            {
                ResetTimeline();
            }
            lastMusicTime = t;
        }
        for (int i = 0; i < events.Length; i++)
        {
            var e = events[i];
            if (!e.fired && t >= e.time)
            {
                e.fired = true;
                if (e.pauseMusic)
                {
                    StartCoroutine(HandlePause(e));
                }
                if (e.isSpecialState)
                {
                    OnStateEvent?.Invoke(e.specialState);
                }
                else
                {
                    OnZoneEvent?.Invoke(e.name);
                }
            }
        }
    }

    private IEnumerator HandlePause(TimelineEvent e)
    {
        if (musicSource != null)
        {
            musicSource.Pause();
            OnMusicPaused?.Invoke();
        }
        OnPauseBegin?.Invoke(e.pauseDuration);
        if (e.sfx != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(e.sfx, 1f, 1f);
        }
        float t = 0f;
        while (t < e.pauseDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }
        if (musicSource != null)
        {
            musicSource.UnPause();
            OnMusicResumed?.Invoke();
        }
    }

    public void ResetTimeline()
    {
        if (events == null) return;
        for (int i = 0; i < events.Length; i++) events[i].fired = false;
    }
}
