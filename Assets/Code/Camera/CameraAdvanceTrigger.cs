using UnityEngine;
using System.Collections;

public class CameraAdvanceTrigger : MonoBehaviour
{
    [SerializeField] private Transform teleportTarget;
    [SerializeField] private Vector3 teleportPosition;
    [SerializeField] private bool useTransformTarget = true;
    [SerializeField] private bool resetVelocity = true;
    [SerializeField] private float cooldown = 0.5f;
    private bool coolingDown = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (coolingDown) return;
        if (!collision.CompareTag("Player")) return;
        TeleportAndDamage(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (coolingDown) return;
        if (!collision.CompareTag("Player")) return;
        TeleportAndDamage(collision.gameObject);
    }

    private void TeleportAndDamage(GameObject playerGO)
    {
        var life = playerGO.GetComponent<playerLife>();
        if (life != null) life.TakeDamage(transform.position, 1);
        var t = playerGO.transform;
        Vector3 dest = useTransformTarget && teleportTarget != null ? teleportTarget.position : teleportPosition;
        t.position = dest;
        if (resetVelocity)
        {
            var rb = playerGO.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
        StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        coolingDown = true;
        yield return new WaitForSeconds(cooldown);
        coolingDown = false;
    }
}
