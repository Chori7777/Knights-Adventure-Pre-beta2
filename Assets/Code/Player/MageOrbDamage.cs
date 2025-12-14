using UnityEngine;

public class MageOrbDamage : MonoBehaviour
{
    public int damage = 1;
    public float lifetime = 3f;

    private void Start()
    {
        if (lifetime > 0f) Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("enemy"))
        {
            var life = other.GetComponent<EnemyLife>();
            if (life != null)
            {
                life.TakeDamageWithKnockback(transform.position, damage);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Boss"))
        {
            var boss = other.GetComponent<BossLife>();
            if (boss == null)
            {
                boss = other.GetComponentInParent<BossLife>();
            }
            if (boss != null)
            {
                boss.RecibeDanio(transform.position, damage);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Pared") || other.CompareTag("Suelo"))
        {
            Destroy(gameObject);
        }
    }
}
