using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroCutscene : MonoBehaviour
{
    [SerializeField] private Sprite[] imagenes;
    [SerializeField] private float tiempoPorImagen = 3f;
    [SerializeField] private string escenaDestino = "MainMenu";
    [SerializeField] private Image imagenDisplay;
    [SerializeField] private GameObject botonSkip;
    [SerializeField] private float tiempoFade = 1f;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private string[] textos;
    [SerializeField] private TextMeshProUGUI textoHistoria;

    private int indiceActual = 0;
    private bool estaSaltando = false;
    private Coroutine rutinaEscritura;

    void Start()
    {
        if (textoHistoria == null)
        {
            textoHistoria = GetComponentInChildren<TextMeshProUGUI>(true);
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (imagenes.Length > 0)
        {
            StartCoroutine(ReproducirIntro());
        }
        else
        {
            IrAlMenu();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
        {
            SkipIntro();
        }
    }

    private IEnumerator ReproducirIntro()
    {
        for (indiceActual = 0; indiceActual < imagenes.Length; indiceActual++)
        {
            if (estaSaltando) break;

            yield return StartCoroutine(MostrarImagen(imagenes[indiceActual]));
            yield return new WaitForSeconds(tiempoPorImagen);

            if (indiceActual < imagenes.Length - 1)
            {
                yield return StartCoroutine(OcultarImagen());
            }
        }

        if (!estaSaltando)
        {
            yield return StartCoroutine(OcultarImagen());
            IrAlMenu();
        }
    }

    private IEnumerator MostrarImagen(Sprite imagen)
    {
        imagenDisplay.sprite = imagen;

        if (textoHistoria != null)
        {
            if (textos != null && textos.Length > indiceActual)
            {
                textoHistoria.text = textos[indiceActual];
                textoHistoria.maxVisibleCharacters = int.MaxValue;
            }
            else
            {
                textoHistoria.text = string.Empty;
                textoHistoria.maxVisibleCharacters = int.MaxValue;
            }
        }

        float tiempo = 0f;
        while (tiempo < tiempoFade)
        {
            tiempo += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, tiempo / tiempoFade);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator OcultarImagen()
    {
        if (textoHistoria != null)
        {
            textoHistoria.text = string.Empty;
        }
        float tiempo = 0f;
        while (tiempo < tiempoFade)
        {
            tiempo += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, tiempo / tiempoFade);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    public void SkipIntro()
    {
        if (!estaSaltando)
        {
            estaSaltando = true;
            if (rutinaEscritura != null)
            {
                StopCoroutine(rutinaEscritura);
                rutinaEscritura = null;
            }
            StopAllCoroutines();
            StartCoroutine(TransicionAlMenu());
        }
    }

    private IEnumerator TransicionAlMenu()
    {
        yield return StartCoroutine(OcultarImagen());
        IrAlMenu();
    }

    private void IrAlMenu()
    {
        SceneManager.LoadScene(escenaDestino);
    }

    private IEnumerator EscribirComoMaquina(TextMeshProUGUI destino, string texto)
    {
        if (destino == null) yield break;
        if (string.IsNullOrEmpty(texto)) { destino.text = ""; yield break; }
        destino.text = texto;
        destino.maxVisibleCharacters = 0;
        destino.ForceMeshUpdate();
        int total = destino.textInfo.characterCount;
        float velocidad = 0.05f;
        var tm = TextManager.Instance;
        if (tm != null)
        {
            velocidad = tm.GetTypeSpeed();
        }
        while (destino.maxVisibleCharacters < total)
        {
            destino.maxVisibleCharacters++;
            yield return new WaitForSecondsRealtime(velocidad);
        }
    }
}
