using UnityEngine;
using System.Collections;

public class ExplosionDamage : MonoBehaviour
{
    [SerializeField] private int damage = 2;
    [SerializeField] private float radius = 1.5f;
    [SerializeField] private float damageDelay = 0f;
    [SerializeField] private bool destroyAfterDamage = false;
    [SerializeField] private float destroyDelay = 0.1f;
    [SerializeField] private bool damagePlayer = true;
    [SerializeField] private bool damageEnemies = true;
    [SerializeField] private bool damageBoss = true;

    private bool hasDamaged = false;

    private void OnEnable()
    {
        if (damageDelay > 0f)
        {
            StartCoroutine(DoDamageDelayed());
        }
        else
        {
            DoDamage();
        }
    }

    private IEnumerator DoDamageDelayed()
    {
        yield return new WaitForSeconds(damageDelay);
        DoDamage();
    }

    public void DoDamage()
    {
        if (hasDamaged) return;
        hasDamaged = true;

        var hits = Physics2D.OverlapCircleAll(transform.position, radius);
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (h == null) continue;
            if (damagePlayer && h.CompareTag("Player"))
            {
                var pl = h.GetComponent<playerLife>();
                if (pl != null)
                {
                    pl.TakeDamage(transform.position, damage);
                }
                continue;
            }
            if (damageEnemies && h.CompareTag("enemy"))
            {
                var el = h.GetComponent<EnemyLife>();
                if (el != null)
                {
                    el.TakeDamageWithKnockback(transform.position, damage);
                }
                continue;
            }
            if (damageBoss && h.CompareTag("Boss"))
            {
                var bl = h.GetComponent<BossLife>();
                if (bl == null) bl = h.GetComponentInParent<BossLife>();
                if (bl != null)
                {
                    bl.RecibeDanio(transform.position, damage);
                }
                continue;
            }
        }

        if (destroyAfterDamage)
        {
            if (destroyDelay > 0f) Destroy(gameObject, destroyDelay);
            else Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
