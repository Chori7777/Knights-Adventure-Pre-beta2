using UnityEngine;

/// <summary>
/// Objeto recolectable que aumenta la vida máxima del jugador
/// </summary>
public class HealthUpgradePickup : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int healthIncrease = 1;
    [SerializeField] private bool healToMax = true; 

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;

    [Header("Efectos Visuales")]
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private float effectDuration = 1f;

    [Header("Animación")]
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatAmount = 0.5f;
    [SerializeField] private bool rotateObject = true;
    [SerializeField] private float rotateSpeed = 50f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        // Animación de flotación
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotación opcional
        if (rotateObject)
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerLife vida = collision.GetComponent<playerLife>();
            if (vida != null)
            {
                Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Debug.Log("Objeto de mejora de vida recogido");

                // Guardar vida actual
                int oldHealth = vida.Health;
                int oldMax = vida.MaxHealth;

                // PASO 1: Aumentar vida máxima
                int newMax = oldMax + healthIncrease;
                vida.SetMaxHealth(newMax);
         

                // PASO 2: Curar
                if (healToMax)
                {
                    vida.SetHealth(newMax);

                }
                else
                {
                    vida.Heal(healthIncrease);
  
                }


                if (PlayerHealthUI.Instance != null)
                {

                    PlayerHealthUI.Instance.ForceRefresh();

                }
                else
                {
                    Debug.LogError("❌ [PICKUP] PlayerHealthUI.Instance es NULL");
                }

                if (pickupSound != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(pickupSound, 0.7f);
                }

                if (pickupEffect != null)
                {
                    GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
                    Destroy(effect, effectDuration);
                }


                if (ControladorDatosJuego.Instance != null)
                {
                    ControladorDatosJuego.Instance.datosjuego.vidaMaxima = newMax;
                    ControladorDatosJuego.Instance.datosjuego.vidaActual = vida.Health;
                    ControladorDatosJuego.Instance.GuardarDatos(false);
                    Debug.Log("Datos guardados");
                }



                // Destruir el objeto
                Destroy(gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar rango de recolección
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}

