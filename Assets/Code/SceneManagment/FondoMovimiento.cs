using UnityEngine;

public class FondoMovimiento : MonoBehaviour
{
    [SerializeField] private Vector2 velocidadMovimiento;
    [SerializeField] private bool resetOnLoop = true;

    private Vector2 offset;
    private Material material;
    private Rigidbody2D jugadorRB;

    private void Awake()
    {
        material = GetComponent<SpriteRenderer>().material;

        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
        {
            jugadorRB = jugador.GetComponent<Rigidbody2D>();
        }
        else
        {
            Debug.LogWarning("[FondoMovimiento] No se encontró objeto con tag 'Player'");
        }
    }

    private void Update()
    {
        if (jugadorRB == null) return;

        offset = (jugadorRB.linearVelocity.x * 0.1f) * velocidadMovimiento * Time.deltaTime;
        material.mainTextureOffset += offset;


        if (resetOnLoop)
        {
            Vector2 currentOffset = material.mainTextureOffset;

            if (currentOffset.x > 1f || currentOffset.x < -1f)
                currentOffset.x %= 1f;

            if (currentOffset.y > 1f || currentOffset.y < -1f)
                currentOffset.y %= 1f;

            material.mainTextureOffset = currentOffset;
        }
    }

    private void OnDestroy()
    {
        if (material != null)
        {
            material.mainTextureOffset = Vector2.zero;
        }
    }
}