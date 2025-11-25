using UnityEngine;

public class FirstEncounterBossHitbox : MonoBehaviour
{
    [SerializeField] private FirstEncounterBossController bossController;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Espada"))
        {
            if (bossController != null)
            {
                bossController.ReceiveHit();
            }
        }
    }
}
