// ============== ShopItem.cs ==============
using System.Collections;
using TMPro;
using UnityEngine;

public class ShopItem : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int cost = 50;
    [SerializeField] private string itemName = "Mejora";
    [SerializeField] private NPCReward reward;

    [Header("Movimiento")]
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatHeight = 0.5f;

    [Header("TextMesh Pro 3D")]
    [SerializeField] private TextMeshPro priceText;
    [SerializeField] private Vector3 textOffset = Vector3.zero;

    private Vector3 startPosition;
    private bool playerInRange = false;
    private bool isPurchased = false;
    private bool canPress = true;

    private void Start()
    {
        startPosition = transform.position;
        UpdatePriceDisplay();
    }

    private void Update()
    {
        // Movimiento flotante
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // Actualizar posición del texto
        if (priceText != null)
        {
            priceText.transform.position = transform.position + textOffset;
        }

        // Una sola E para comprar
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && canPress && !isPurchased)
        {
            StartCoroutine(TryPurchase());
        }
    }
    private void UpdatePriceDisplay()
    {
        if (priceText != null)
            priceText.text = $"${cost}";
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log($"Shop: Presiona E para comprar {itemName} por ${cost}");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private IEnumerator TryPurchase()
    {
        canPress = false;

        if (ControladorDatosJuego.Instance == null)
        {
            canPress = true;
            yield break;
        }

        int currentCoins = ControladorDatosJuego.Instance.ObtenerMonedas();

        if (currentCoins >= cost)
        {
            // Restar monedas
            ControladorDatosJuego.Instance.AgregarMonedas(-cost);

            // Dar recompensa
            if (reward != null)
                reward.Apply();

            // Marcar como comprado y cambiar color
            isPurchased = true;
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = new Color(0.4f, 0.4f, 0.4f, 0.6f);

            if (priceText != null)
                priceText.text = "Purchased";

            Debug.Log($"[ShopItem] Compra exitosa: {itemName}");

            // Efecto visual (parpadeo)
            yield return StartCoroutine(PurchaseEffect());
        }
        else
        {
            Debug.Log($"[ShopItem] No tienes suficientes monedas. Necesitas {cost}, tienes {currentCoins}");

            // Efecto de fallo (sacudida)
            yield return StartCoroutine(FailEffect());
        }

        yield return new WaitForSeconds(0.5f);
        canPress = true;
    }

    private IEnumerator PurchaseEffect()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        for (int i = 0; i < 5; i++)
        {
            sr.color = new Color(1, 1, 0, 1); // Amarillo
            yield return new WaitForSeconds(0.1f);
            sr.color = new Color(0.4f, 0.4f, 0.4f, 0.6f); // Gris
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator FailEffect()
    {
        Vector3 originalPos = transform.position;

        for (int i = 0; i < 4; i++)
        {
            transform.position = originalPos + Vector3.right * 0.1f;
            yield return new WaitForSeconds(0.05f);
            transform.position = originalPos - Vector3.right * 0.1f;
            yield return new WaitForSeconds(0.05f);
        }

        transform.position = originalPos;
    }
}
