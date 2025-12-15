using UnityEngine;
using TMPro;

public class FloatingInteractable : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject floatingText; // Texto 3D (TextMeshPro)
    public KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool useDistanceCheck = false;
    [SerializeField] private float interactionDistance = 2f;

    private bool playerInRange = false;
    private Camera mainCamera;
    private Transform player;

    void Start()
    {
        if (floatingText != null)
            floatingText.SetActive(false);

        mainCamera = Camera.main;
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (useDistanceCheck && player != null)
        {
            playerInRange = Vector3.Distance(transform.position, player.position) <= interactionDistance;
            if (floatingText != null)
                floatingText.SetActive(playerInRange);
        }

        if (floatingText != null && mainCamera != null)
        {
            // Hace que el texto siempre mire a la c�mara
            floatingText.transform.LookAt(mainCamera.transform);
            floatingText.transform.Rotate(0, 180, 0); // Corrige la orientaci�n
        }

        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (useDistanceCheck) return;
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (floatingText != null)
                floatingText.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (useDistanceCheck) return;
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (floatingText != null)
                floatingText.SetActive(false);
        }
    }

    void Interact()
    {
        Debug.Log("El jugador interactu� con " + gameObject.name);
        // Ac� pod�s poner lo que quieras: abrir puerta, recolectar �tem, etc.
    }
}
