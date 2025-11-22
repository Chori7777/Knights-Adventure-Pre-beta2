using UnityEngine;

public class damageCollider : MonoBehaviour
{
    //Script basico de daño por colision
    public int damage = 1; 
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player")) 
        {
            playerLife playerHealth = other.gameObject.GetComponent<playerLife>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(transform.position, damage);

            }
        }
    }
}
