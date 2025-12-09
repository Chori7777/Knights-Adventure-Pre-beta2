using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 0.7f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [Header("Pitch Settings")]
    [SerializeField] private float musicPitch = 1f;

    private Coroutine musicFadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
        }

        LoadVolumes();
        UpdateVolumes();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip, volume * sfxVolume * masterVolume);
        sfxSource.pitch = 1f;
    }

    public void PlaySFX3D(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        PlaySFX(clip, volume);
    }

    public void PlayMusic(AudioClip clip, float volume = 1f, bool loop = true, bool fade = true)
    {
        if (clip == null) return;

        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }

        if (fade)
        {
            if (musicSource.isPlaying)
            {
                musicFadeCoroutine = StartCoroutine(FadeMusicCoroutine(clip, volume, loop));
            }
            else
            {
                musicFadeCoroutine = StartCoroutine(FadeInNewClipCoroutine(clip, volume, loop));
            }
        }
        else
        {
            PlayMusicImmediate(clip, volume, loop);
        }
    }

    private void PlayMusicImmediate(AudioClip clip, float volume, bool loop)
    {
        musicSource.clip = clip;
        musicSource.volume = volume * musicVolume * masterVolume;
        musicSource.loop = loop;
        musicSource.pitch = Mathf.Clamp(musicPitch, 0.5f, 2f);
        musicSource.Play();
    }

    private IEnumerator FadeMusicCoroutine(AudioClip newClip, float targetVolume, bool loop)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration / 2f)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (fadeDuration / 2f));
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.pitch = Mathf.Clamp(musicPitch, 0.5f, 2f);
        musicSource.Play();

        elapsed = 0f;
        float finalVolume = targetVolume * musicVolume * masterVolume;

        while (elapsed < fadeDuration / 2f)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, finalVolume, elapsed / (fadeDuration / 2f));
            yield return null;
        }

        musicSource.volume = finalVolume;
        musicFadeCoroutine = null;
    }

    private IEnumerator FadeInNewClipCoroutine(AudioClip newClip, float targetVolume, bool loop)
    {
        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.pitch = Mathf.Clamp(musicPitch, 0.5f, 2f);
        musicSource.volume = 0f;
        musicSource.Play();

        float elapsed = 0f;
        float finalVolume = targetVolume * musicVolume * masterVolume;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, finalVolume, elapsed / fadeDuration);
            yield return null;
        }
        musicSource.volume = finalVolume;
        musicFadeCoroutine = null;
    }

    public void StopMusic(bool fade = true)
    {
        if (fade)
        {
            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }
            musicFadeCoroutine = StartCoroutine(FadeOutMusicCoroutine());
        }
        else
        {

            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
                musicFadeCoroutine = null;
            }
            musicSource.Stop();
        }
    }

    public void StopMusicImmediately()
    {
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }
        musicSource.Stop();
    }

    private IEnumerator FadeOutMusicCoroutine()
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume;
        musicFadeCoroutine = null;
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    public void SetMusicPitch(float pitch)
    {
        musicPitch = Mathf.Clamp(pitch, 0.5f, 2f);
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.pitch = musicPitch;
        }
    }

    public IEnumerator RampMusicPitch(float targetPitch, float duration)
    {
        float start = musicSource != null ? musicSource.pitch : 1f;
        float t = 0f;
        float target = Mathf.Clamp(targetPitch, 0.5f, 2f);
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Lerp(start, target, duration > 0f ? t / duration : 1f);
            SetMusicPitch(p);
            yield return null;
        }
        SetMusicPitch(target);
    }

    public void ResetMusicPitch()
    {
        SetMusicPitch(1f);
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        SaveVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        SaveVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        SaveVolumes();
    }

    private void UpdateVolumes()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume * masterVolume;
        }
    }

    public void SaveVolumes()
    {
        PlayerPrefs.SetFloat("audio_master", masterVolume);
        PlayerPrefs.SetFloat("audio_music", musicVolume);
        PlayerPrefs.SetFloat("audio_sfx", sfxVolume);
        PlayerPrefs.Save();
    }

    public void LoadVolumes()
    {
        masterVolume = PlayerPrefs.GetFloat("audio_master", masterVolume);
        musicVolume = PlayerPrefs.GetFloat("audio_music", musicVolume);
        sfxVolume = PlayerPrefs.GetFloat("audio_sfx", sfxVolume);
    }

    public bool IsMusicPlaying()
    {
        return musicSource.isPlaying;
    }

    public AudioClip GetCurrentMusic()
    {
        return musicSource.clip;
    }
}
