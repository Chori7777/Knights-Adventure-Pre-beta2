using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeController : MonoBehaviour
{
    [SerializeField] private Animator animator;  // Asigna el Animator del Canvas aquí en el Inspector
    private static FadeController instance;

    [SerializeField] private float duracionFadeOut = 1f; // Duración de la animación FadeOut (ajusta a 1s si es tu caso)

    private void Awake()
    {
        // Singleton que persiste entre escenas
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Verificar si el Animator está asignado
        if (animator == null)
        {
            Debug.LogError(" Animator no asignado en FadeController. Arrástralo desde el Canvas en el Inspector.");
        }
        else
        {
            Debug.Log(" Animator asignado en FadeController.");
        }

        // Subscribirse al evento de cambio de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Desubscribirse del evento
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Se ejecuta CADA VEZ que se carga una escena nueva
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(" Escena cargada: " + scene.name + " - Activando FadeIn");
        ActivarFadeIn();
    }

    // Activa la animación de FadeIn UNA SOLA VEZ
    // Activa la animación de FadeIn
    public void ActivarFadeIn()
    {
        if (animator != null)
        {
            animator.SetTrigger("FadeInTrigger");
            Debug.Log(" FadeIn activado (por Trigger).");
        }
        else
        {
            Debug.LogError(" No se puede activar FadeIn: Animator es null.");
        }
    }

    // Activa la animación de FadeOut
    public void ActivarFadeOut()
    {
        if (animator != null)
        {
            animator.SetTrigger("FadeOutTrigger");
            Debug.Log(" FadeOut activado (por Trigger).");
        }
        else
        {
            Debug.LogError(" No se puede activar FadeOut: Animator es null.");
        }
    }


    // Método para cambiar de escena CON transición completa
    public void CambiarEscenaConFade(string nombreEscena)
    {
        StartCoroutine(TransicionCompleta(nombreEscena));
    }

    private IEnumerator TransicionCompleta(string nombreEscena)
    {
        // 1. Fade out (a negro)
        ActivarFadeOut();
        Debug.Log(" Fade out iniciado");

        // 2. Esperar a que termine la animación
        yield return new WaitForSeconds(duracionFadeOut);

        // 3. Cambiar de escena
        Debug.Log(" Cambiando a escena: " + nombreEscena);
        SceneManager.LoadScene(nombreEscena);

        // 4. El fade in se activa automáticamente en OnSceneLoaded
    }

    public static FadeController Instance => instance;
}
