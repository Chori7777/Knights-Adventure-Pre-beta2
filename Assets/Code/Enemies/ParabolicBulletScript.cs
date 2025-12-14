using UnityEngine;
using UnityEngine.SceneManagement;

public class ParabolicBulletScript : MonoBehaviour
{
    public int damage = 1;
    public float lifetime = 4f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // No asignar velocidad aqu�, ya viene del enemigo
        if (rb == null)
        {
            Debug.LogError("No hay Rigidbody2D en el proyectil par�bola");
        }
        Destroy(gameObject, lifetime);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<playerLife>();
            if (player != null)
            {
                player.TakeDamage(transform.position, damage);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Suelo"))
        {
            Destroy(gameObject);
        }
    }
}
