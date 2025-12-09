using UnityEngine;

public class triggerDamage : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int damage = 1; 
    public float damageCooldown = 0.5f;
    private float lastDamageTime = -10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Boss") || other.CompareTag("enemy"))
        {
            return; // Ignorar colision con el jefe
        }
        if (other.CompareTag("Player")) 
        {
            playerLife playerHealth = other.GetComponent<playerLife>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(transform.position,damage);
                lastDamageTime = Time.time;

            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time < lastDamageTime + damageCooldown) return;
        var playerHealth = other.GetComponent<playerLife>();
        if (playerHealth == null) return;
        playerHealth.TakeDamage(transform.position, damage);
        lastDamageTime = Time.time;
    }
}


