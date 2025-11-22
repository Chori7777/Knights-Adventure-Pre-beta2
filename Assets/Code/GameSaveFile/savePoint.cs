using UnityEngine;
using System.Collections;

public class savePoint : MonoBehaviour
{
    [Header("Configuración del Checkpoint")]
    [SerializeField] private AudioClip checkpoint;
    [SerializeField] private bool curarAlGuardar = true;
    [SerializeField] private bool mostrarMensaje = true;

    [Header("Efectos Visuales")]
    [SerializeField] private GameObject saveEffect;
    [SerializeField] private float effectDuration = 1f;

    [Header("Animación")]
    [SerializeField] private Animator animator;
    [SerializeField] private string activateTrigger = "Activate";

    private bool activado = false;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activado) return;
        if (!collision.CompareTag("Player")) return;

        StartCoroutine(SaveSequence(collision.gameObject));
    }

    private IEnumerator SaveSequence(GameObject player)
    {
        activado = true;

        playerLife vida = player.GetComponent<playerLife>();
        if (vida == null)
        {
            Debug.LogError("❌ [savePoint] No se encontró playerLife en el jugador");
            yield break;
        }


        Debug.Log("Guardando progreso...");

        var datos = ControladorDatosJuego.Instance.datosjuego;

        // Guardar VIDA ACTUAL y MÁXIMA
        datos.vidaActual = vida.Health;
        datos.vidaMaxima = vida.MaxHealth;
        Debug.Log($" Vida guardada: {datos.vidaActual}/{datos.vidaMaxima}");

        // Guardar POCIONES
        datos.cantidadpociones = vida.Potions;
        datos.maxPotions = vida.MaxPotions;


        // Guardar POSICIÓN
        Vector3 posicionCheckpoint = transform.position;
        datos.posicion = posicionCheckpoint;


        // Guardar CÁMARA
        Camera cam = Camera.main;
        if (cam != null)
        {
            datos.posicionCamara = cam.transform.position;
        }

        // Guardar ESCENA ACTUAL
        datos.escenaActual = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"Escena guardada: {datos.escenaActual}");

        // GUARDAR TODO EN ARCHIVO
        ControladorDatosJuego.Instance.GuardarDatos(false);
        Debug.Log(" Datos guardados exitosamente");


        // ========== PASO 3: CURAR JUGADOR (OPCIONAL) ==========
        if (curarAlGuardar)
        {
            vida.HealFull();
            Debug.Log(" Vida restaurada completamente");

            // Actualizar UI
            if (PlayerHealthUI.Instance != null)
            {
                PlayerHealthUI.Instance.UpdateDisplay();
            }
        }

        // Sonido
        if (checkpoint != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(checkpoint, 0.5f);
        }

        // Animación
        if (animator != null && !string.IsNullOrEmpty(activateTrigger))
        {
            animator.SetTrigger(activateTrigger);
        }

        // Efecto visual
        if (saveEffect != null)
        {
            GameObject effect = Instantiate(saveEffect, transform.position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }

        if (mostrarMensaje)
        {
  
            Debug.Log("💾 Progreso guardado");
        }

        yield return null;
    }


    public static void ForceSave(GameObject player)
    {
        if (player == null)
        {
            Debug.LogError("Player es null");
            return;
        }

        playerLife vida = player.GetComponent<playerLife>();
        if (vida == null || ControladorDatosJuego.Instance == null)
            return;

        var datos = ControladorDatosJuego.Instance.datosjuego;
        datos.vidaActual = vida.Health;
        datos.vidaMaxima = vida.MaxHealth;
        datos.cantidadpociones = vida.Potions;
        datos.maxPotions = vida.MaxPotions;
        datos.posicion = player.transform.position;
        datos.escenaActual = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        ControladorDatosJuego.Instance.GuardarDatos(false);
        Debug.Log("Datos guardados");
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar área del checkpoint
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1f);

        // Línea hacia arriba
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
    }
}