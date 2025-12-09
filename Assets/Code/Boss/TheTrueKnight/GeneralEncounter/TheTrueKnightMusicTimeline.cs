using UnityEngine;

public class TheTrueKnightMusicTimeline : MonoBehaviour
{
    [System.Serializable]
    public class TimelineEvent
    {
        public float time;
        public string name;
        [HideInInspector] public bool fired;
    }

    [SerializeField] private TheTrueKnightBossAI bossAI;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private bool autoFindAudioManager = true;
    [SerializeField] private bool autoFindBossAI = true;
    [SerializeField] private bool autoStartOnMusicPlay = true;
    [SerializeField] private bool resetOnMusicLoop = true;
    [SerializeField] private TimelineEvent[] events;

    public void StartTimeline()
    {
        enabled = true;
        lastMusicTime = musicSource != null ? musicSource.time : 0f;
    }

    public void ResetTimeline()
    {
        if (events == null) return;
        for (int i = 0; i < events.Length; i++)
        {
            events[i].fired = false;
        }
    }

    private void Awake()
    {
        if (autoFindAudioManager && musicSource == null && AudioManager.Instance != null)
        {
            musicSource = AudioManager.Instance.GetComponent<AudioSource>();
        }
        if (autoFindBossAI && bossAI == null)
        {
            bossAI = FindFirstObjectByType<TheTrueKnightBossAI>(FindObjectsInactive.Include);
        }
        enabled = autoStartOnMusicPlay;
    }

    private void Update()
    {
        if (musicSource == null && autoFindAudioManager && AudioManager.Instance != null)
        {
            musicSource = AudioManager.Instance.GetComponent<AudioSource>();
        }
        if (bossAI == null && autoFindBossAI)
        {
            bossAI = FindFirstObjectByType<TheTrueKnightBossAI>(FindObjectsInactive.Include);
        }
        if (bossAI == null || musicSource == null || events == null) return;
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
                bossAI.ReceiveMusicEvent(e.name);
            }
        }
    }

    private float lastMusicTime;
}
