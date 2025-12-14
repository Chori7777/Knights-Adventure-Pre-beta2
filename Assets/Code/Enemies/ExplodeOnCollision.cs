using UnityEngine;

public class ExplodeOnCollision : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private bool destroySelf = true;
    [SerializeField] private float destroyDelay = 0f;
    [SerializeField] private bool triggerAlso = true;

    private bool exploded = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryExplode();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggerAlso) return;
        TryExplode();
    }

    private void TryExplode()
    {
        if (exploded) return;
        exploded = true;
        if (explosionPrefab != null)
        {
            var exp = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            var dmg = exp.GetComponent<ExplosionDamage>();
            if (dmg != null)
            {
                dmg.DoDamage();
            }
        }
        if (explosionSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(explosionSound);
        }
        if (destroySelf)
        {
            if (destroyDelay > 0f) Destroy(gameObject, destroyDelay);
            else Destroy(gameObject);
        }
    }
}
