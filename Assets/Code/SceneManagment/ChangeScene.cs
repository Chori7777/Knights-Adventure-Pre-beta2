using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using TMPro;

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
            AutoWireMainMenuButtons();
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
        int variant = MainMenuVariation;
        if (ControladorDatosJuego.Instance != null)
        {
            variant = ControladorDatosJuego.Instance.datosjuego.startModeVariant;
        }
        if (variant == 1)
        {
            NewGamePlus();
            return;
        }
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
    
    public void GoToMainMenuVariant(int variation)
    {
        SetMenuVariation(variation);
        MainMenu();
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
        CargarEscena("AlternativeForest");
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
    
    public void NewGameForcePlus()
    {
        NewGamePlus();
    }
    public void NewGameForceOriginal()
    {
        SetMenuVariation(0);
        NewGame();
    }

    private void AutoWireMainMenuButtons()
    {
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            var buttons = roots[i].GetComponentsInChildren<Button>(true);
            for (int j = 0; j < buttons.Length; j++)
            {
                var b = buttons[j];
                var n = b.gameObject.name.ToLowerInvariant();
                string label = null;
                var t = b.GetComponentInChildren<TMP_Text>();
                if (t != null) label = t.text.ToLowerInvariant();
                else
                {
                    var ut = b.GetComponentInChildren<Text>();
                    if (ut != null) label = ut.text.ToLowerInvariant();
                }
                bool isNewGame =
                    n.Contains("newgame") || n.Contains("new game") || n.Contains("nuevo") || n.Contains("juego") ||
                    (label != null && (label.Contains("new game") || label.Contains("nuevo juego")));
                if (isNewGame)
                {
                    b.onClick.RemoveAllListeners();
                    if (MainMenuVariation == 1)
                        b.onClick.AddListener(NewGameForcePlus);
                    else
                        b.onClick.AddListener(NewGame);
                }
                bool isNewGameOriginal =
                    n.Contains("newgamenormal") || n.Contains("new game normal") || n.Contains("newgameoriginal") || n.Contains("original") ||
                    (label != null && (label.Contains("new game normal") || label.Contains("juego original") || label.Contains("original")));
                if (isNewGameOriginal)
                {
                    b.onClick.RemoveAllListeners();
                    b.onClick.AddListener(NewGameForceOriginal);
                    bool unlocked = false;
                    if (ControladorDatosJuego.Instance != null)
                        unlocked = ControladorDatosJuego.Instance.EstaNewGameNormalDesbloqueado();
                    b.gameObject.SetActive(unlocked);
                }
            }
        }
    }
}
