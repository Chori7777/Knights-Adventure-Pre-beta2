using UnityEngine;
using System.Collections;

public class CameraTrigger : MonoBehaviour
{
    [SerializeField] private int targetCheckpoint = 1; // El checkpoint al que va este trigger
    [SerializeField] private float nuevoZoom = 8f;
    [SerializeField] private float cooldownTiempo = 1f;
    private float zoomAnterior;
    private bool enCooldown = false;
    private CameraManager cm;

    void Awake()
    {
        cm = CameraManager.instance != null ? CameraManager.instance : FindFirstObjectByType<CameraManager>(FindObjectsInactive.Include);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !enCooldown)
        {
            if (cm == null)
            {
                cm = CameraManager.instance != null ? CameraManager.instance : FindFirstObjectByType<CameraManager>(FindObjectsInactive.Include);
                if (cm == null) return;
            }
            zoomAnterior = cm.GetCameraSize();

            cm.IrAlCheckpoint(targetCheckpoint);

            cm.SetCameraSize(nuevoZoom);

            Debug.Log("Cámara movida al checkpoint " + targetCheckpoint + " | Zoom: " + nuevoZoom);

            // Iniciar cooldown
            StartCoroutine(ActivarCooldown());
        }
    }

    private System.Collections.IEnumerator ActivarCooldown()
    {
        enCooldown = true;
        yield return new WaitForSeconds(cooldownTiempo);
        enCooldown = false;
    }

    void OnDrawGizmosSelected()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, boxCollider.size);
        }
    }
}
