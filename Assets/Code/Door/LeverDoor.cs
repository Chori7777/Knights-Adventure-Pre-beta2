using UnityEngine;

public class LeverDoor : MonoBehaviour
{
    [SerializeField] private bool isOpen = false;
    private bool playerInRange = false;
    public BossDoor door;
    Animator anim;
    private void Awake()
    {
        door = GetComponent<BossDoor>();
        anim=GetComponent<Animator>();
        if (door == null)
            Debug.LogError("Falta el script BossDoor en este objeto");
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isOpen)
            {
                door.AbrirPuerta();
                isOpen = true;
                Debug.Log("Puerta abierta");
                anim.SetTrigger("Activate");
            }
            else
            {
                door.CerrarPuerta();
                isOpen = false;
                Debug.Log("Puerta cerrada");
                anim.SetTrigger("Deactivate");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Jugador cerca de la puerta");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Jugador lejos de la puerta");
        }
    }
}
