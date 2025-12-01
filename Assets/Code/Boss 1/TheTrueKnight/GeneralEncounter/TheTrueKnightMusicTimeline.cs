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
    [SerializeField] private TimelineEvent[] events;

    public void StartTimeline()
    {
        enabled = true;
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
        enabled = false;
    }

    private void Update()
    {
        if (bossAI == null || musicSource == null || events == null) return;
        float t = musicSource.time;
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
}

