using UnityEngine;

public class SceneMusicPlayer: MonoBehaviour
{
    [Header("Escena Music Clip")]
    [SerializeField] private AudioClip sceneMusic;

    [Header("Volumen")]
    [Range(0f, 1f)][SerializeField] private float volume = 0.7f;

    [Header("Loop")]
    [SerializeField] private bool loop = true;

    private void Start()
    {
        PlaySceneMusic();
    }

    private void PlaySceneMusic()
    {
        if (sceneMusic != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(sceneMusic, volume, loop, true); // fade automático
        }
    }
}
