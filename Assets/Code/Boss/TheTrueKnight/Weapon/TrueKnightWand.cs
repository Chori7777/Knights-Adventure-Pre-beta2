using UnityEngine;
using DG.Tweening;
using System.Collections;

public class TrueKnightWand : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer wandRenderer;
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private Ease fadeInEase = Ease.OutQuad;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip chargeClip;
    [SerializeField] private AudioClip shootClip;

    [Header("Objetivo y Dirección")]
    [SerializeField] private Transform targetAim;
    [SerializeField] private bool aimAtTarget = true;

    public enum GravityDir { Up, Down, Left, Right }
    [Header("Gravedad del Jugador")]
    [SerializeField] private float gravityStrength = 12f;
    [SerializeField] private float gravityDuration = 2f;
    [SerializeField] private bool applyGravityOnActivate = false;
    [SerializeField] private bool autoActivateOnEnable = true;

    [Header("Rayo (Beam)")]
    [SerializeField] private GameObject beamPrefab;
    [SerializeField] private float beamLifetime = 1.2f;
    [SerializeField] private float beamChargeTime = 0.8f;
    [SerializeField] private bool beamMoves = false;
    [SerializeField] private float beamMoveDistance = 20f;
    [SerializeField] private float beamMoveDuration = 0.8f;
    [SerializeField] private Ease beamMoveEase = Ease.Linear;
    [SerializeField] private float beamMaxLength = 20f;

    [Header("Ataque Simple")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawn;
    [SerializeField] private int projectileCount = 3;
    [SerializeField] private float projectileInterval = 0.2f;
    [SerializeField] private float projectileSpeed = 10f;

    private Transform player;
    private Rigidbody2D playerRb;

    private void Awake()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerRb = p.GetComponent<Rigidbody2D>();
        }
    }

    private void OnEnable()
    {
        if (!autoActivateOnEnable) return;
        if (wandRenderer != null)
        {
            Color c = wandRenderer.color; c.a = 0f; wandRenderer.color = c;
            wandRenderer.DOFade(1f, fadeInDuration).SetEase(fadeInEase);
        }
        if (aimAtTarget && targetAim != null)
        {
            Vector2 dir = (targetAim.position - transform.position).normalized;
            transform.right = new Vector3(dir.x, dir.y, 0f);
        }
        if (applyGravityOnActivate)
        {
            AplicarGravedadJugador(GravityDir.Down, gravityDuration);
        }
    }

    public void SeleccionarGravedad(GravityDir dir)
    {
        AplicarGravedadJugador(dir, gravityDuration);
    }

    public void CargarRayoYDisparar(Transform destino, bool moverRayo)
    {
        StartCoroutine(BeamRoutine(destino, moverRayo));
    }

    public void AtaqueSimpleAlJugador()
    {
        StartCoroutine(SimpleShootRoutine());
    }

    private IEnumerator BeamRoutine(Transform destino, bool mover)
    {
        if (aimAtTarget && destino != null)
        {
            Vector2 dir = (destino.position - transform.position).normalized;
            transform.right = new Vector3(dir.x, dir.y, 0f);
        }

        if (audioSource != null && chargeClip != null)
        {
            audioSource.clip = chargeClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        yield return new WaitForSeconds(beamChargeTime);

        if (audioSource != null)
        {
            audioSource.Stop();
            if (shootClip != null) audioSource.PlayOneShot(shootClip);
        }

        if (beamPrefab != null)
        {
            GameObject beam = Instantiate(beamPrefab, transform.position, transform.rotation);
            var bc = beam.GetComponent<BeamController>();
            if (bc != null)
            {
                bc.SetMaxLength(beamMaxLength);
                bc.Activate(beamLifetime);
            }
            if (mover && beamMoves)
            {
                Vector3 dir = transform.right;
                beam.transform.DOMove(beam.transform.position + dir * beamMoveDistance, beamMoveDuration).SetEase(beamMoveEase);
            }
            Destroy(beam, beamLifetime);
        }
    }

    private IEnumerator SimpleShootRoutine()
    {
        int count = Mathf.Max(1, projectileCount);
        for (int i = 0; i < count; i++)
        {
            DispararProj();
            if (projectileInterval > 0f) yield return new WaitForSeconds(projectileInterval);
        }
    }

    private void DispararProj()
    {
        if (projectilePrefab == null) return;
        Vector3 spawnPos = projectileSpawn != null ? projectileSpawn.position : transform.position;
        Quaternion rot = transform.rotation;
        GameObject go = Instantiate(projectilePrefab, spawnPos, rot);
        Rigidbody2D rb2 = go.GetComponent<Rigidbody2D>();
        if (rb2 != null)
        {
            rb2.linearVelocity = (Vector2)transform.right * projectileSpeed;
        }
        else
        {
            var upm = go.GetComponent<UniversalProjectileMover>();
            if (upm != null)
            {
                upm.SetCustomDirection(transform.right);
                upm.SetBaseSpeed(projectileSpeed);
                upm.Initialize();
            }
        }
    }

    private void AplicarGravedadJugador(GravityDir dir, float dur)
    {
        if (playerRb == null) return;
        StartCoroutine(GravRoutine(dir, dur));
    }

    private IEnumerator GravRoutine(GravityDir dir, float dur)
    {
        float end = Time.time + dur;
        Vector2 g = Vector2.down;
        if (dir == GravityDir.Up) g = Vector2.up;
        else if (dir == GravityDir.Left) g = Vector2.left;
        else if (dir == GravityDir.Right) g = Vector2.right;

        while (Time.time < end)
        {
            yield return new WaitForFixedUpdate();
            if (playerRb != null)
            {
                playerRb.AddForce(g * gravityStrength, ForceMode2D.Force);
            }
        }
    }
}
