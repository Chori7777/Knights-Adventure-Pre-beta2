using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


public class BossScriptAttacks : MonoBehaviour
{
    [Header("Referencias")]
    private bossCore core;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject alertaPrefab;

    [Header("Configuración General")]
    [SerializeField] private float timeBetweenAttacks = 3f;
    [SerializeField] private float minDistanceForMelee = 2f;
    [SerializeField] private float alertDuration = 0.8f;

    [Header("Ataque: Lluvia de Piedras")]
    [SerializeField] private int stonesPerRain = 5;
    [SerializeField] private float rainSpread = 8f;
    [SerializeField] private float rainHeight = 10f;
    [SerializeField] private float timeBetweenStones = 0.2f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float stoneRainInterval = 6f;
    [SerializeField] private float stoneLifetime = 3f;
    [SerializeField] private float stoneFallSpeed = 12f;
    [Header("Pantalla")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeIntensity = 0.08f;

    [Header("Ataque: Embestida")]
    [SerializeField] private float chargeSpeedMultiplier = 2f;
    [SerializeField] private float chargeDuration = 2f;

    [Header("Ataque: Golpe Melee")]
    [SerializeField] private float meleeDuration = 1.5f;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float moveDuration = 1.5f;

    private GameObject currentAlert;
    private bool isMoving = false;

 
    private void Start()
    {
        // Buscar BossCore
        core = GetComponent<bossCore>();

        if (core == null)
        {
            enabled = false;
            return;
        }

        // Crear alerta si existe
        if (alertaPrefab != null)
        {
            currentAlert = Instantiate(alertaPrefab, transform);
            currentAlert.SetActive(false);
        }

        // Crear spawn point si no existe
        if (spawnPoint == null)
        {
            GameObject spawnObj = new GameObject("ProjectileSpawnPoint");
            spawnObj.transform.SetParent(transform);
            spawnObj.transform.localPosition = new Vector3(0, 2f, 0);
            spawnPoint = spawnObj.transform;
        }

        Debug.Log("Funcionando");

        // Iniciar loop de ataques
        StartCoroutine(AttackLoop());
    }


    private float lastStoneRainTime = -999f;

    private IEnumerator AttackLoop()
    {
        Debug.Log("Loop de ataques iniciado");

        yield return new WaitForSeconds(1f); // Espera inicial

        int attackCount = 0;

        while (!core.IsDead)
        {
            attackCount++;

            if (!core.IsAttacking && core.player != null)
            {
                float distance = core.DistanceToPlayer();
                if (distance <= minDistanceForMelee)
                {
                    yield return StartCoroutine(MeleeAttack());
                }
            }

            if (Time.time >= lastStoneRainTime + stoneRainInterval)
            {
                yield return StartCoroutine(StoneRainAttack());
                lastStoneRainTime = Time.time;
            }

            // Posible embestida si el jugador está a rango medio
            if (!core.IsAttacking && core.player != null)
            {
                float dist = core.DistanceToPlayer();
                if (dist > minDistanceForMelee && dist < 8f)
                {
                    yield return StartCoroutine(ChargeAttack());
                }
            }

            Debug.Log("pensando siguiente ataque... mm...");
            yield return new WaitForSeconds(timeBetweenAttacks);
        }
        Debug.Log("Jefe muerto");
    }

//Como se mueve cuando no ataca
    private void Update()
    {
        if (core == null || core.IsDead) return;

        // Mirar al jugador
        core.FacePlayer();

        // Movimiento estratégico
        if (!core.IsAttacking && !isMoving)
        {
            StartCoroutine(StrategicMovement());
        }
    }
    //Logica del pensamiento estrategico
    private IEnumerator StrategicMovement()
    {
        isMoving = true;

        float duration = Random.Range(1f, moveDuration);
        float elapsed = 0f;

        bool shouldApproach = Random.value > 0.5f;
        Vector2 direction = core.DirectionToPlayer();

        if (!shouldApproach) direction = -direction;

        while (elapsed < duration && !core.IsAttacking)
        {
            core.rb.linearVelocity = new Vector2(direction.x * moveSpeed, core.rb.linearVelocity.y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        core.rb.linearVelocity = new Vector2(0, core.rb.linearVelocity.y);
        isMoving = false;
    }

  //Su lista de ataques
    private IEnumerator StoneRainAttack()
    {
        core.IsAttacking = true;
        core.rb.linearVelocity = Vector2.zero;

        yield return StartCoroutine(ShowAlert());

        core.rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < stonesPerRain; i++)
        {
            SpawnStone();
            yield return new WaitForSeconds(timeBetweenStones);
        }

        yield return new WaitForSeconds(0.5f);
        core.IsAttacking = false;
    }

    private void SpawnStone()
    {
        Vector3 spawnPosition = spawnPoint.position;
        float randomX = Random.Range(-rainSpread, rainSpread);
        spawnPosition.x += randomX;
        spawnPosition.y += rainHeight;

        GameObject p = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        if (p != null)
        {
            var rb = p.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.down * stoneFallSpeed;
            }

            var col = p.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
            }

            Destroy(p, stoneLifetime);
            StartCoroutine(ShakeCamera(shakeDuration, shakeIntensity));
        }
    }

    private IEnumerator ShakeCamera(float duration, float intensity)
    {
        var cam = Camera.main;
        if (cam == null) yield break;

        Transform t = cam.transform;
        Vector3 original = t.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            t.position = new Vector3(original.x + x, original.y + y, original.z);
            yield return null;
        }
        t.position = original;
    }

 // segundo ataque
    

  //Su ataque mas basico, el tercero
    private IEnumerator MeleeAttack()
    {
        core.IsAttacking = true;
        core.rb.linearVelocity = Vector2.zero;

        // Alerta corta
        yield return StartCoroutine(ShowAlert(0.5f));

        // Animación
        if (core.anim != null)
        {
            core.anim.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(meleeDuration);

        core.IsAttacking = false;
    }

//La alerta para q el jugador sepa cuando ataca 
    private IEnumerator ShowAlert(float duration = -1)
    {
        if (duration < 0) duration = alertDuration;

        if (currentAlert != null)
        {
            currentAlert.SetActive(true);
            Animator alertAnim = currentAlert.GetComponent<Animator>();
            if (alertAnim != null)
            {
                alertAnim.SetTrigger("Alert");
            }
        }

        yield return new WaitForSeconds(duration);

        if (currentAlert != null)
        {
            currentAlert.SetActive(false);
        }
    }
    private IEnumerator ChargeAttack()
    {
        core.IsAttacking = true;
        float elapsed = 0f;
        Vector2 dir = core.DirectionToPlayer().normalized;
        float baseX = moveSpeed;
        while (elapsed < chargeDuration)
        {
            core.rb.linearVelocity = new Vector2(dir.x * baseX * chargeSpeedMultiplier, core.rb.linearVelocity.y);
            elapsed += Time.deltaTime;
            yield return null;
        }
        core.rb.linearVelocity = new Vector2(0f, core.rb.linearVelocity.y);
        core.IsAttacking = false;
    }
}
