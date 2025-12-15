using UnityEngine;
using System.Collections;

public class MageWandAttack : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject orbPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Ataque")]
    [SerializeField] private int orbCount = 3;
    [SerializeField] private float orbSpeed = 10f;
    [SerializeField] private float spreadAngle = 12f;
    [SerializeField] private float orbLifetime = 3f;
    [SerializeField] private int orbDamage = 1;
    [SerializeField] private float attackCooldown = 0.4f;
    [Header("Trail")]
    [SerializeField] private bool addTrailToOrbs = true;
    [SerializeField] private float orbTrailTime = 0.25f;
    [SerializeField] private float orbTrailWidth = 0.08f;
    [SerializeField] private Color orbTrailStartColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private Color orbTrailEndColor = new Color(1f, 1f, 1f, 0f);
    [Header("Afterimage Orbs")]
    [SerializeField] private bool useOrbAfterimage = true;
    [SerializeField] private float orbAfterimageInterval = 0.045f;
    [SerializeField] private float orbAfterimageLifetime = 0.22f;
    [SerializeField] private Color orbAfterimageColor = new Color(1f, 1f, 1f, 0.7f);
    [Header("Afterimage Mago")]
    [SerializeField] private bool useSelfAfterimageOnAttack = true;
    [SerializeField] private float selfAfterimageDuration = 0.22f;
    [SerializeField] private float selfAfterimageInterval = 0.035f;
    [SerializeField] private Color selfAfterimageColor = new Color(1f, 1f, 1f, 0.7f);

    [Header("Animación")]
    [SerializeField] private Animator animator;
    [SerializeField] private string attackTriggerName = "WandAttack";

    private float lastAttackTime = -10f;
    private PlayerMovement pm;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (firePoint == null) firePoint = transform;
        pm = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (pm != null && !pm.canThrowProjectile) return;
        if (!InputBindings.GetDown(InputBindings.GameAction.Action1Attack)) return;
        if (Time.time < lastAttackTime + attackCooldown) return;
        lastAttackTime = Time.time;

        if (animator != null && !string.IsNullOrEmpty(attackTriggerName))
            animator.SetTrigger(attackTriggerName);

        if (useSelfAfterimageOnAttack) StartCoroutine(SelfAfterimageBurst());
        ShootOrbs();
    }

    private void ShootOrbs()
    {
        if (orbPrefab == null || firePoint == null) return;

        Vector2 baseDir = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;
        float half = spreadAngle * 0.5f;

        for (int i = 0; i < Mathf.Max(1, orbCount); i++)
        {
            float t = (orbCount == 1) ? 0f : (float)i / (orbCount - 1);
            float ang = Mathf.Lerp(-half, half, t);
            Vector2 dir = Rotate(baseDir, ang);
            GameObject o = Instantiate(orbPrefab, firePoint.position, Quaternion.identity);
            var rb = o.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = dir.normalized * orbSpeed;
            if (addTrailToOrbs) EnsureTrail(o);
            if (useOrbAfterimage) StartCoroutine(OrbAfterimageRoutine(o));

            var dmg = o.GetComponent<MageOrbDamage>();
            if (dmg == null) dmg = o.AddComponent<MageOrbDamage>();
            int bonus = 0;
            if (ControladorDatosJuego.Instance != null && ControladorDatosJuego.Instance.datosjuego != null)
                bonus = ControladorDatosJuego.Instance.datosjuego.attackDamageUpgrades;
            dmg.damage = Mathf.Max(1, orbDamage + bonus);
            dmg.lifetime = orbLifetime;
        }
    }

    private Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float ca = Mathf.Cos(rad);
        float sa = Mathf.Sin(rad);
        return new Vector2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
    }

    private void EnsureTrail(GameObject go)
    {
        var tr = go.GetComponent<TrailRenderer>();
        if (tr == null) tr = go.AddComponent<TrailRenderer>();
        tr.time = orbTrailTime;
        tr.minVertexDistance = 0.08f;
        tr.autodestruct = false;
        tr.startWidth = orbTrailWidth;
        tr.endWidth = orbTrailWidth * 0.7f;
        tr.material = new Material(Shader.Find("Sprites/Default"));
        var g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] { new GradientColorKey(orbTrailStartColor, 0f), new GradientColorKey(orbTrailEndColor, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(orbTrailStartColor.a, 0f), new GradientAlphaKey(orbTrailEndColor.a, 1f) }
        );
        tr.colorGradient = g;
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            tr.sortingLayerID = sr.sortingLayerID;
            tr.sortingOrder = sr.sortingOrder - 1;
        }
    }

    private IEnumerator OrbAfterimageRoutine(GameObject orb)
    {
        SpriteRenderer sr = null;
        if (orb != null)
        {
            sr = orb.GetComponent<SpriteRenderer>();
            if (sr == null) sr = orb.GetComponentInChildren<SpriteRenderer>(true);
        }
        float t = 0f;
        while (orb != null && sr != null && t < orbLifetime)
        {
            SpawnAfterimage(sr, orbAfterimageLifetime);
            t += orbAfterimageInterval;
            yield return new WaitForSeconds(orbAfterimageInterval);
        }
    }

    private void SpawnAfterimage(SpriteRenderer source, float lifetime)
    {
        var go = new GameObject("OrbAfterimage");
        var c = go.AddComponent<SpriteRenderer>();
        c.sprite = source.sprite;
        c.flipX = source.flipX;
        c.color = orbAfterimageColor;
        c.sortingLayerID = source.sortingLayerID;
        c.sortingOrder = source.sortingOrder - 1;
        go.transform.position = source.transform.position;
        StartCoroutine(FadeAndDestroy(c, lifetime));
    }

    private IEnumerator FadeAndDestroy(SpriteRenderer c, float lifetime)
    {
        float t = lifetime;
        while (t > 0f && c != null)
        {
            t -= Time.deltaTime;
            var col = c.color;
            col.a = Mathf.Clamp01(t / lifetime);
            c.color = col;
            yield return null;
        }
        if (c != null) Destroy(c.gameObject);
    }

    private IEnumerator SelfAfterimageBurst()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>(true);
        float t = 0f;
        while (sr != null && t < selfAfterimageDuration)
        {
            var go = new GameObject("MageAfterimage");
            var c = go.AddComponent<SpriteRenderer>();
            c.sprite = sr.sprite;
            c.flipX = sr.flipX;
            c.color = selfAfterimageColor;
            c.sortingLayerID = sr.sortingLayerID;
            c.sortingOrder = sr.sortingOrder - 1;
            go.transform.position = sr.transform.position;
            StartCoroutine(FadeAndDestroy(c, selfAfterimageDuration));
            t += selfAfterimageInterval;
            yield return new WaitForSeconds(selfAfterimageInterval);
        }
    }

    public void TriggerSelfAfterimage()
    {
        if (useSelfAfterimageOnAttack)
            StartCoroutine(SelfAfterimageBurst());
    }
}
