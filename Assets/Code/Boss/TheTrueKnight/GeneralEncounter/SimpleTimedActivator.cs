using System.Collections.Generic;
using UnityEngine;

public class SimpleTimedActivator : MonoBehaviour
{
    [System.Serializable]
    public class ActivationEntry
    {
        public float time;
        public GameObject target;
        public float deactivateAfter;
    }

    [SerializeField] private bool useMusicSource = true;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private bool autoFindAudioManager = true;
    [SerializeField] private bool autoFindAnyAudioSource = true;
    [SerializeField] private bool autoStartOnEnable = true;
    [SerializeField] private bool autoStartOnMusicPlay = true;
    [SerializeField] private bool resetOnMusicLoop = true;
    [SerializeField] private bool sortEntriesByTime = true;
    [SerializeField] private ActivationEntry[] entries;
    [SerializeField] private bool deactivatePreviousOnActivate = true;

    private float startTime;
    private bool running;
    private int index;
    private GameObject lastActivated;
    private float lastMusicTime;

    private struct PendingDeactivation
    {
        public GameObject target;
        public float time;
    }
    private readonly List<PendingDeactivation> pending = new List<PendingDeactivation>();

    private void Awake()
    {
        if (autoFindAudioManager && useMusicSource && musicSource == null && AudioManager.Instance != null)
            musicSource = AudioManager.Instance.GetComponent<AudioSource>();
        if (musicSource == null && autoFindAnyAudioSource)
            TryFindAnyAudioSource();
    }

    public void StartTimeline()
    {
        running = true;
        startTime = Time.time;
        index = 0;
        if (entries != null && sortEntriesByTime)
        {
            System.Array.Sort(entries, (a, b) => a.time.CompareTo(b.time));
        }
        pending.Clear();
        lastMusicTime = musicSource != null ? musicSource.time : 0f;
    }

    public void ResetTimeline()
    {
        running = false;
        index = 0;
        lastActivated = null;
        pending.Clear();
    }

    private void Update()
    {
        if (musicSource == null && autoFindAnyAudioSource)
            TryFindAnyAudioSource();

        if (!running)
        {
            if (autoStartOnMusicPlay && useMusicSource && musicSource != null && musicSource.isPlaying)
                StartTimeline();
            else if (autoStartOnEnable && !useMusicSource)
                StartTimeline();
            else return;
        }

        float t = useMusicSource && musicSource != null ? musicSource.time : (Time.time - startTime);

        if (resetOnMusicLoop && useMusicSource && musicSource != null && musicSource.loop)
        {
            if (t < lastMusicTime)
            {
                index = 0;
                pending.Clear();
                lastActivated = null;
            }
            lastMusicTime = t;
        }

        if (entries != null && index < entries.Length)
        {
            var e = entries[index];
            if (t >= e.time)
            {
                if (deactivatePreviousOnActivate && lastActivated != null)
                    lastActivated.SetActive(false);

                if (e.target != null)
                {
                    e.target.SetActive(true);
                    lastActivated = e.target;
                    if (e.deactivateAfter > 0f)
                        pending.Add(new PendingDeactivation { target = e.target, time = t + e.deactivateAfter });
                }
                index++;
            }
        }

        for (int i = pending.Count - 1; i >= 0; i--)
        {
            var p = pending[i];
            if (t >= p.time)
            {
                if (p.target != null)
                    p.target.SetActive(false);
                pending.RemoveAt(i);
            }
        }
    }

    private void OnEnable()
    {
        if (musicSource == null && autoFindAnyAudioSource)
            TryFindAnyAudioSource();
        if (!running)
        {
            if (autoStartOnMusicPlay && useMusicSource && musicSource != null && musicSource.isPlaying)
                StartTimeline();
            else if (autoStartOnEnable && !useMusicSource)
                StartTimeline();
        }
    }

    private void TryFindAnyAudioSource()
    {
        var sources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        AudioSource best = null;
        for (int i = 0; i < sources.Length; i++)
        {
            var s = sources[i];
            if (s != null && s.isPlaying && s.clip != null) { best = s; break; }
        }
        if (best == null)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                var s = sources[i];
                if (s != null && s.clip != null) { best = s; break; }
            }
        }
        if (best != null) musicSource = best;
    }
}
