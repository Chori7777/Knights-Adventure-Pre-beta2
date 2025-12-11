using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class ChangeScene : MonoBehaviour
{
    public static int MainMenuVariation = 0;

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
        MainMenuVariation = 0;
        CargarEscena("MainMenu");
    }

    public void NewGame()
    {
        BorrarPartidaGuardada();
        if (ControladorDatosJuego.Instance != null)
        {
            ControladorDatosJuego.Instance.ResetearDatos();
            ControladorDatosJuego.Instance.datosjuego.jefesDerrotados.Clear();
        }
        CargarEscena("StoryPreForest");
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
    }
}
