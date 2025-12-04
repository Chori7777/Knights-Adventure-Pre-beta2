using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject boss;            // Jefe que se activa
    [SerializeField] private Animator doorAnimator;      // Animator de la puerta
    [SerializeField] private AudioClip bossMusic;        // Música de jefe

    [Header("Opciones")]
    [SerializeField] private bool triggerOnce = true;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered && triggerOnce) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;

            // Activar al jefe
            if (boss != null)
                boss.SetActive(true);

            // Cerrar la puerta con animación
            if (doorAnimator != null)
                doorAnimator.SetTrigger("DoorClosed");

            // Reproducir música de jefe
            if (bossMusic != null && AudioManager.Instance != null)
                AudioManager.Instance.PlayMusic(bossMusic, 1f, true);
        }
    }
}
