using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class ChangeScene : MonoBehaviour
{
    public static int MainMenuVariation = 0;
    private void Start()
    {
        string escenaActual = SceneManager.GetActiveScene().name;
        if (escenaActual == "MainMenu")
        {
            if (ControladorDatosJuego.Instance == null)
            {
                var go = new GameObject("ControladorDatosJuego");
                go.AddComponent<ControladorDatosJuego>();
            }
            else
            {
                ControladorDatosJuego.Instance.CargarDatos();
            }
            if (AudioManager.Instance == null)
            {
                var go2 = new GameObject("AudioManager");
                go2.AddComponent<AudioManager>();
            }
            int variant = MainMenuVariation;
            if (ControladorDatosJuego.Instance != null)
            {
                variant = ControladorDatosJuego.Instance.datosjuego.startModeVariant;
                MainMenuVariation = variant;
            }
            if (variant == 1)
            {
                NewGamePlus();
                return;
            }
            if (variant == 2)
            {
                CargarEscena("TheEnd");
                return;
            }
        }
    }

    private void CargarEscena(string nombreEscena)
    {
        Time.timeScale = 1f;
        if (FadeController.Instance != null)
        {
            FadeController.Instance.CambiarEscenaConFade(nombreEscena);
        }
        else
        {
            SceneManager.LoadScene(nombreEscena);
        }
    }

    public void LoadScene(string sceneName)
    {
        CargarEscena(sceneName);
    }

    public void pause()
    {
        Time.timeScale = 0f;
    }

    public void resume()
    {
        Time.timeScale = 1f;
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        CargarEscena(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        if (ControladorDatosJuego.Instance != null)
        {
            ControladorDatosJuego.Instance.GuardarDatos();
        }
        Application.Quit();
    }

    public void MainMenu()
    {
        if (ControladorDatosJuego.Instance != null)
        {
            MainMenuVariation = ControladorDatosJuego.Instance.datosjuego.startModeVariant;
        }
        CargarEscena("MainMenu");
    }

    public void NewGame()
    {
        BorrarPartidaGuardada();
        if (ControladorDatosJuego.Instance != null)
        {
            ControladorDatosJuego.Instance.ResetearDatos();
            ControladorDatosJuego.Instance.datosjuego.jefesDerrotados.Clear();
            ControladorDatosJuego.Instance.datosjuego.startModeVariant = MainMenuVariation;
        }
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(1f);
            AudioManager.Instance.SetMusicVolume(1f);
            AudioManager.Instance.SetSFXVolume(1f);
        }

        CargarEscena("TheForest");
    }


    public void ContinueGame()
    {
        if (ExistePartidaGuardada())
        {
            Debug.Log("[ChangeScene] Archivo de guardado encontrado, cargando partida");

            if (ControladorDatosJuego.Instance != null)
            {

                ControladorDatosJuego.Instance.ContinuarPartida();
            }
            else
            {
                Debug.LogError("[ChangeScene] No existe ControladorDatosJuego en la escena");
            }
        }
        else
        {
            Debug.LogWarning("[ChangeScene] No se encontró ninguna partida guardada");
        }
    }

    public bool ExistePartidaGuardada()
    {
        string archivo = Application.persistentDataPath + "/save.json";
        bool existe = File.Exists(archivo);
        Debug.Log($"[ChangeScene] Buscando guardado en: {archivo} - {(existe ? "EXISTE" : "NO EXISTE")}");
        return existe;
    }

    private void BorrarPartidaGuardada()
    {
        string archivo = Application.persistentDataPath + "/save.json";
        if (File.Exists(archivo))
        {
            File.Delete(archivo);
            Debug.Log("[ChangeScene] Partida guardada eliminada");
        }
    }

    public void SetMenuVariation(int variation)
    {
        MainMenuVariation = variation;
        if (ControladorDatosJuego.Instance != null)
        {
            ControladorDatosJuego.Instance.SetStartModeVariant(variation);
        }
    }

    public void NewGamePlus()
    {
        SetMenuVariation(1);
        BorrarPartidaGuardada();
        if (ControladorDatosJuego.Instance != null)
        {
            ControladorDatosJuego.Instance.ResetearDatos();
            ControladorDatosJuego.Instance.datosjuego.jefesDerrotados.Clear();
            ControladorDatosJuego.Instance.datosjuego.startModeVariant = MainMenuVariation;
        }
        CargarEscena("StoryPreSnowBoss");
    }

    public void ReturnToOriginalGame()
    {
        SetMenuVariation(0);
        MainMenu();
    }

    public void StartGame()
    {
        int variant = MainMenuVariation;
        if (ControladorDatosJuego.Instance != null)
        {
            variant = ControladorDatosJuego.Instance.datosjuego.startModeVariant;
            MainMenuVariation = variant;
        }
        if (variant == 1)
        {
            NewGamePlus();
            return;
        }
        if (variant == 2)
        {
            CargarEscena("TheEnd");
            return;
        }
        NewGame();
    }
}
