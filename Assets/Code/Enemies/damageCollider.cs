 using UnityEngine;

public class damageCollider : MonoBehaviour
{
    //Script basico de dano por colision
    public int damage = 1; 
    public float damageCooldown = 0.5f;
    private float lastDamageTime = -10f;
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player")) 
        {
            playerLife playerHealth = other.gameObject.GetComponent<playerLife>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(transform.position, damage);
                lastDamageTime = Time.time;

            }
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        if (Time.time < lastDamageTime + damageCooldown) return;
        var playerHealth = other.gameObject.GetComponent<playerLife>();
        if (playerHealth == null) return;
        playerHealth.TakeDamage(transform.position, damage);
        lastDamageTime = Time.time;
    }
}
