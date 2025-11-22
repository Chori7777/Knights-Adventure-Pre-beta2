using UnityEngine;

public class triggerDamage : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int damage = 1; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Boss") || other.CompareTag("enemy"))
        {
            return; // Ignorar colisión con el jefe
        }
        if (other.CompareTag("Player")) 
        {
            playerLife playerHealth = other.GetComponent<playerLife>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(transform.position,damage);

            }
        }
    }
    
}


