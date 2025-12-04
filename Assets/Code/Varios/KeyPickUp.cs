using UnityEngine;
using UnityEngine.Tilemaps;

public class KeyPickup : MonoBehaviour
{
    [Header("Objetos que se destruyen al recoger la llave")]
    [SerializeField] private Tilemap[] tilemapsToDestroy;
    [SerializeField] private GameObject[] objectsToDestroy;

    [Header("Efectos opcionales")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject pickupEffect;

    private AudioSource audioSource;

    private void Start()
    {
        // Crea un AudioSource si hay sonido configurado
        if (pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica si el jugador entró al trigger
        if (collision.CompareTag("Player"))
        {
            // Reproduce sonido
            if (audioSource != null && pickupSound != null)
                audioSource.PlayOneShot(pickupSound);

            // Efecto visual
            if (pickupEffect != null)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);

            // Destruye los Tilemaps
            foreach (Tilemap tm in tilemapsToDestroy)
            {
                if (tm != null)
                    Destroy(tm.gameObject);
            }

            // Destruye objetos adicionales (puertas, bloques, etc)
            foreach (GameObject obj in objectsToDestroy)
            {
                if (obj != null)
                    Destroy(obj);
            }

            // Destruye la llave (el propio objeto)
            Destroy(gameObject);
        }
    }
}
