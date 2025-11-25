using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textoMuerte; // Asignalo desde el Inspector
    [SerializeField] private float velocidadTipeo = 0.05f; // Velocidad entre letras

    private string[] frasesMuerte = new string[]
    {
        "Knight! Don't give up now! You were close!",
        "Knight, continue for the future of the kingdom!",
        "...",
        "Cold...",
        "¿Fue intencional?",
        "No matter how many times you die, you'll always come back, Knight!",
        "Ouroboros",
        "No matter what happens, you must reach the top of the tower",
    };

    private void Start()
    {
        Time.timeScale = 0f; // Pausa el juego al morir
        MostrarTextoMuerte();
    }

    private void MostrarTextoMuerte()
    {
        if (textoMuerte == null) return;
        int randomIndex = Random.Range(0, frasesMuerte.Length);
        StartCoroutine(TipearTexto(frasesMuerte[randomIndex]));
    }

    private IEnumerator TipearTexto(string frase)
    {
        textoMuerte.text = "";
        foreach (char letra in frase)
        {
            textoMuerte.text += letra;
            yield return new WaitForSecondsRealtime(velocidadTipeo);
            // WaitForSecondsRealtime se usa porque el juego está en pausa (Time.timeScale = 0)
        }
    }


    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        string escenaActual = SceneManager.GetActiveScene().name;

        Debug.Log($" Reiniciando nivel completo: {escenaActual}");
        SceneManager.LoadScene(escenaActual);
    }


    public void CargarCheckpoint()
    {
        Time.timeScale = 1f;
        if (ControladorDatosJuego.Instance == null)
        {
            Debug.LogError("❌ No existe instancia de ControladorDatosJuego.");
            return;
        }

        // Usar el flujo oficial de respawn que activa el flag y reposiciona al cargar
        ControladorDatosJuego.Instance.RespawnearJugadorEnCheckpoint();
    }

    // 🔙 Volver al menú principal (opcional)
    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        Debug.Log(" Volviendo al menú principal...");
        SceneManager.LoadScene("MainMenu");
    }

    // Métodos alias para enlazar fácilmente desde UI
    public void RestartFromCheckpoint()
    {
        CargarCheckpoint();
    }

    public void RestartLevel()
    {
        ReiniciarNivel();
    }

    public void Continue()
    {
        if (ControladorDatosJuego.Instance != null)
        {
            Time.timeScale = 1f;
            ControladorDatosJuego.Instance.ContinuarPartida();
        }
    }
}
