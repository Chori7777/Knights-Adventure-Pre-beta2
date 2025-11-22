using UnityEngine;
using System.Collections;

public class CameraTrigger : MonoBehaviour
{
    [SerializeField] private int targetCheckpoint = 1; // El checkpoint al que va este trigger
    [SerializeField] private float nuevoZoom = 8f;
    [SerializeField] private float cooldownTiempo = 1f;
    private float zoomAnterior;
    private bool enCooldown = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !enCooldown)
        {
            // Guarda el zoom anterior
            zoomAnterior = CameraManager.instance.GetCameraSize();

            // Va al checkpoint especificado
            CameraManager.instance.IrAlCheckpoint(targetCheckpoint);

            // Cambia el zoom
            CameraManager.instance.SetCameraSize(nuevoZoom);

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