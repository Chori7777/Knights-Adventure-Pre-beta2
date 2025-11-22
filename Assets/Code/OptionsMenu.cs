using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OpcionesMenu : MonoBehaviour
{
    [Header(" Sliders de Volumen")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
   
        if (AudioManager.Instance != null)
        {
            masterSlider.value = AudioManager.Instance.masterVolume;
            musicSlider.value = AudioManager.Instance.musicVolume;
            sfxSlider.value = AudioManager.Instance.sfxVolume;
        }

        masterSlider.onValueChanged.AddListener(OnMasterVolumeChange);
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChange);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChange);
    }

    private void OnMasterVolumeChange(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMasterVolume(value);
    }

    private void OnMusicVolumeChange(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
    }

    private void OnSFXVolumeChange(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }

    public void VolverAlJuego()
    {
        if (ControladorDatosJuego.Instance == null)
        {
            Debug.LogError(" No hay instancia de ControladorDatosJuego");
            return;
        }

        string escenaGuardada = ControladorDatosJuego.Instance.datosjuego.escenaActual;

        if (!string.IsNullOrEmpty(escenaGuardada))
        {
            Debug.Log("Volviendo a la escena guardada: ");
            SceneManager.LoadScene(escenaGuardada);
        }
        else
        {
            Debug.LogWarning(" No hay escena guardada para volver");
        }
    }
}
