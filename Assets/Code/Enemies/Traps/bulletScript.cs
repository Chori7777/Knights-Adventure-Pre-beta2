using UnityEngine;
using UnityEngine.SceneManagement;

public class BulletScript : MonoBehaviour
{
    public int damage = 1;
    public float lifetime = 3f;
    public float speed = 10f; 

    void Start()
    {
        // Si tiene Rigidbody2D, usar su velocidad
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null && rb.linearVelocity == Vector2.zero)
        {
            // Si no tiene velocidad asignada, usar la direcci�n del firepoint
            Vector2 direction = transform.right;
            rb.linearVelocity = direction * speed;
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
        else if (other.CompareTag("Pared") || other.CompareTag("Suelo"))
        {
            Destroy(gameObject);
        }
    }
}
