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
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;

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
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

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

        AudioSource.PlayClipAtPoint(clip, position, volume * sfxVolume * masterVolume);
    }

    public void PlayMusic(AudioClip clip, float volume = 1f, bool loop = true, bool fade = true)
    {
        if (clip == null) return;

        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }

        if (fade && musicSource.isPlaying)
        {
            musicFadeCoroutine = StartCoroutine(FadeMusicCoroutine(clip, volume, loop));
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

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    private void UpdateVolumes()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
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