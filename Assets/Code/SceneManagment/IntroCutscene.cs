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

    void Start()
    {
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

        if (textoHistoria != null && textos.Length > indiceActual)
        {
            textoHistoria.text = textos[indiceActual];
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
}
