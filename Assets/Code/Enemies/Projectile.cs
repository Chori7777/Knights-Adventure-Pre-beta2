using UnityEngine;
using System.Collections;


public class Projectile : MonoBehaviour
{
    [SerializeField] private float velocidad = 5f;
    [SerializeField] private float tiempoDeVida = 10f;
    [Header("Visual")]
    [SerializeField] private bool addTrail = true;
    [SerializeField] private bool useAfterimage = true;
    [SerializeField] private float trailTime = 0.25f;
    [SerializeField] private float trailWidth = 0.08f;
    [SerializeField] private Color trailStartColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private Color trailEndColor = new Color(1f, 1f, 1f, 0f);
    [SerializeField] private float afterimageInterval = 0.045f;
    [SerializeField] private float afterimageLifetime = 0.22f;
    [SerializeField] private Color afterimageColor = new Color(1f, 1f, 1f, 0.7f);

    private Vector3 direccion;
    private Transform objetivo;
    private int tipo=0;
    private float tiempoTranscurrido = 0f;
    private int rebotes = 0;
    private int maxRebotes = 3;

    void Start()
    {
        if (addTrail) EnsureTrail();
        if (useAfterimage) StartCoroutine(AfterimageRoutine());
        Destroy(gameObject, tiempoDeVida);
    }

    void Update()
    {
        tiempoTranscurrido += Time.deltaTime;

        switch (tipo)
        {
            case 0:
                Recto();
                break;
            case 1:
                SigueJugador();
                break;
            case 2:
                Patron();
                break;
            case 3:
                Rebota();
                break;
        }
    }

    // TIPO 0: Movimiento recto
    private void Recto()
    {
        transform.Translate(direccion * velocidad * Time.deltaTime);
    }

    // TIPO 1: Sigue al jugador
    private void SigueJugador()
    {
        if (objetivo == null) return;

        Vector3 direccionAlJugador = (objetivo.position - transform.position).normalized;
        transform.Translate(direccionAlJugador * velocidad * Time.deltaTime);

        // Rota hacia el jugador
        float angulo = Mathf.Atan2(direccionAlJugador.y, direccionAlJugador.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angulo, Vector3.forward);
    }

    // TIPO 2: Patr�n (c�rculo, espiral, onda)
    private void Patron()
    {
        // Patr�n circular
        float x = Mathf.Cos(tiempoTranscurrido * 3f) * 3f;
        float y = Mathf.Sin(tiempoTranscurrido * 3f) * 3f;

        Vector3 offset = new Vector3(x, y, 0f);
        transform.position += direccion * velocidad * Time.deltaTime;
        transform.position += offset * 0.1f;
    }

    // TIPO 3: Rebota en los bordes
    private void Rebota()
    {
        transform.Translate(direccion * velocidad * Time.deltaTime);

        // L�mites de rebote (ajusta seg�n tu escena)
        float minX = -10f;
        float maxX = 10f;
        float minY = -5f;
        float maxY = 15f;

        if (transform.position.x < minX || transform.position.x > maxX)
        {
            direccion.x *= -1;
            rebotes++;
        }

        if (transform.position.y < minY || transform.position.y > maxY)
        {
            direccion.y *= -1;
            rebotes++;
        }

        // Se destruye despu�s de X rebotes
        if (rebotes > maxRebotes)
        {
            Destroy(gameObject);
        }
    }

    public void Inicializar(Vector3 dir, int tipoAtaque, Transform objetivoJugador = null)
    {
        direccion = dir.normalized;
        tipo = tipoAtaque;
        objetivo = objetivoJugador;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("�Projectil golpe� al jugador!");
            Destroy(gameObject);
        }
    }

    private void EnsureTrail()
    {
        var tr = GetComponent<TrailRenderer>();
        if (tr == null) tr = gameObject.AddComponent<TrailRenderer>();
        tr.time = trailTime;
        tr.minVertexDistance = 0.08f;
        tr.autodestruct = false;
        tr.startWidth = trailWidth;
        tr.endWidth = trailWidth * 0.7f;
        tr.material = new Material(Shader.Find("Sprites/Default"));
        var g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] { new GradientColorKey(trailStartColor, 0f), new GradientColorKey(trailEndColor, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(trailStartColor.a, 0f), new GradientAlphaKey(trailEndColor.a, 1f) }
        );
        tr.colorGradient = g;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            tr.sortingLayerID = sr.sortingLayerID;
            tr.sortingOrder = sr.sortingOrder - 1;
        }
    }

    private IEnumerator AfterimageRoutine()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>(true);
        float t = 0f;
        while (this != null && sr != null && t < tiempoDeVida)
        {
            SpawnAfterimage(sr, afterimageLifetime);
            t += afterimageInterval;
            yield return new WaitForSeconds(afterimageInterval);
        }
    }

    private void SpawnAfterimage(SpriteRenderer source, float life)
    {
        var go = new GameObject("ProjectileAfterimage");
        var c = go.AddComponent<SpriteRenderer>();
        c.sprite = source.sprite;
        c.flipX = source.flipX;
        c.color = afterimageColor;
        c.sortingLayerID = source.sortingLayerID;
        c.sortingOrder = source.sortingOrder - 1;
        go.transform.position = source.transform.position;
        StartCoroutine(FadeAndDestroy(c, life));
    }

    private IEnumerator FadeAndDestroy(SpriteRenderer c, float life)
    {
        float t = life;
        while (t > 0f && c != null)
        {
            t -= Time.deltaTime;
            var col = c.color;
            col.a = Mathf.Clamp01(t / life);
            c.color = col;
            yield return null;
        }
        if (c != null) Destroy(c.gameObject);
    }
}
