using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [Header("Efecto")]
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private float effectDuration = 0.5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var datos = ControladorDatosJuego.Instance != null
                ? ControladorDatosJuego.Instance
                : FindFirstObjectByType<ControladorDatosJuego>(FindObjectsInactive.Include);
            if (datos != null)
            {
                datos.AgregarMonedas(10);
            }
            else
            {
                Debug.LogWarning("[Coin] ControladorDatosJuego no disponible, ignorando agregar monedas");
            }
            if (pickupSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(pickupSound, 0.7f);
            }
            if (pickupEffect != null)
            {
                var fx = Instantiate(pickupEffect, transform.position, Quaternion.identity);
                Destroy(fx, effectDuration);
            }
            Destroy(gameObject);
        }
    }
}
