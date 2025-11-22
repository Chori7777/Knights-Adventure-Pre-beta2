using UnityEngine;

public class Pinchos : MonoBehaviour
{
    [Header("Configuración de Daño")]
    public int daño = 1;
    public float fuerzaRebote = 10f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerLife vida = collision.GetComponent<playerLife>();
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

            if (vida != null)
            {
                // Aplicar daño con tu sistema
                vida.TakeDamage(transform.position, daño);

                // Empujar hacia arriba (para evitar que quede trabado)
                if (rb != null)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // resetea la velocidad vertical
                    rb.AddForce(Vector2.up * fuerzaRebote, ForceMode2D.Impulse);
                }
            }
        }
    }
}
