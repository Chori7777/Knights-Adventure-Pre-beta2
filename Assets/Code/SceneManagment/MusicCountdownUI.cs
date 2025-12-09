using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicCountdownUI : MonoBehaviour
{
    [SerializeField] private bool useMusicSource = true;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private bool autoFindAudioManager = true;
    [SerializeField] private bool autoFindAnyAudioSource = true;
    [SerializeField] private float durationOverride = 0f;
    [SerializeField] private TextMeshProUGUI tmpText;
    [SerializeField] private Text uiText;
    [SerializeField] private bool showMilliseconds = false;
    [SerializeField] private string prefix = "";

    private void Awake()
    {
        if (autoFindAudioManager && useMusicSource && musicSource == null && AudioManager.Instance != null)
            musicSource = AudioManager.Instance.GetComponent<AudioSource>();
        if (musicSource == null && autoFindAnyAudioSource)
            TryFindAnyAudioSource();
    }

    private void OnEnable()
    {
        if (musicSource == null && autoFindAnyAudioSource)
            TryFindAnyAudioSource();
    }

    private void Update()
    {
        if (musicSource == null && autoFindAnyAudioSource)
            TryFindAnyAudioSource();

        float total = 0f;
        float current = 0f;
        if (useMusicSource && musicSource != null)
        {
            if (musicSource.clip != null)
            {
                total = musicSource.clip.length;
                current = musicSource.time;
                if (musicSource.loop && total > 0f)
                    current = current % total;
            }
            else if (durationOverride > 0f)
            {
                total = durationOverride;
                current = musicSource.time;
            }
        }
        else if (durationOverride > 0f)
        {
            total = durationOverride;
            current = 0f;
        }

        float remain = Mathf.Max(0f, total - current);
        string text = Format(remain);
        if (tmpText != null) tmpText.text = text;
        if (uiText != null) uiText.text = text;
    }

    private void TryFindAnyAudioSource()
    {
        var sources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        AudioSource best = null;
        for (int i = 0; i < sources.Length; i++)
        {
            var s = sources[i];
            if (s != null && s.isPlaying && s.clip != null)
            {
                best = s; break;
            }
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

    private string Format(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        if (showMilliseconds)
        {
            int ms = Mathf.FloorToInt((seconds - Mathf.Floor(seconds)) * 100f);
            return string.IsNullOrEmpty(prefix) ? $"{m:0}:{s:00}.{ms:00}" : $"{prefix} {m:0}:{s:00}.{ms:00}";
        }
        return string.IsNullOrEmpty(prefix) ? $"{m:0}:{s:00}" : $"{prefix} {m:0}:{s:00}";
    }
}
