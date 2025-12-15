using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BulletScript : MonoBehaviour
{
    public int damage = 1;
    public float lifetime = 3f;
    public float speed = 10f; 
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

    void Start()
    {
        // Si tiene Rigidbody2D, usar su velocidad
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null && rb.linearVelocity == Vector2.zero)
        {
            // Si no tiene velocidad asignada, usar la direcci�n del firepoint
            Vector2 direction = transform.right;
            rb.linearVelocity = direction * speed;
        }
        if (addTrail) EnsureTrail();
        if (useAfterimage) StartCoroutine(AfterimageRoutine());

        Destroy(gameObject, lifetime);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<playerLife>();
            if (player != null)
            {
                player.TakeDamage(transform.position, damage);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Pared") || other.CompareTag("Suelo"))
        {
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
        while (this != null && sr != null && t < lifetime)
        {
            SpawnAfterimage(sr, afterimageLifetime);
            t += afterimageInterval;
            yield return new WaitForSeconds(afterimageInterval);
        }
    }

    private void SpawnAfterimage(SpriteRenderer source, float life)
    {
        var go = new GameObject("BulletAfterimage");
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
