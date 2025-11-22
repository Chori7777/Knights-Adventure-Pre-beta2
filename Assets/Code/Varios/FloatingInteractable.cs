using UnityEngine;
using TMPro;

public class FloatingInteractable : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject floatingText; // Texto 3D (TextMeshPro)
    public KeyCode interactKey = KeyCode.E;

    private bool playerInRange = false;
    private Camera mainCamera;

    void Start()
    {
        if (floatingText != null)
            floatingText.SetActive(false);

        mainCamera = Camera.main;
    }

    void Update()
    {
        if (floatingText != null && mainCamera != null)
        {
            // Hace que el texto siempre mire a la cámara
            floatingText.transform.LookAt(mainCamera.transform);
            floatingText.transform.Rotate(0, 180, 0); // Corrige la orientación
        }

        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (floatingText != null)
                floatingText.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (floatingText != null)
                floatingText.SetActive(false);
        }
    }

    void Interact()
    {
        Debug.Log("El jugador interactuó con " + gameObject.name);
        // Acá podés poner lo que quieras: abrir puerta, recolectar ítem, etc.
    }
}
