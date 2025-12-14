using System.Collections;
using UnityEngine;

public class TownStalkerEntityController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float passiveSpeed = 1.2f;
    [SerializeField] private float aggressiveSpeed = 8f;
    [SerializeField] private float hoverAmplitude = 0.2f;
    [SerializeField] private float hoverFrequency = 2f;
    [SerializeField] private float drainDistance = 2.5f;
    [SerializeField] private float drainRate = 5f; // unidades de tiempo por segundo
    [SerializeField] private Transform leftBoundary;
    [SerializeField] private Transform rightBoundary;
    [SerializeField] private float explosionDistance = 1.3f;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionCooldown = 2f;
    [SerializeField] private bool forceAggressive = true;
    [SerializeField] private bool dashNonStopToPlayer = true;

    private float hoverPhase = 0f;
    private bool aggressive = false;
    private float lastExplosionTime = -999f;

    public float CurrentScoreTime { get; set; } = 30f; // asignado externamente por el sistema de score

    private void Update()
    {
        hoverPhase += Time.deltaTime * hoverFrequency;
        Vector3 pos = transform.position;
        pos.y += Mathf.Sin(hoverPhase) * hoverAmplitude * Time.deltaTime;
        transform.position = pos;

        if (player == null) return;

        // Pasivo si hay tiempo
        aggressive = forceAggressive || CurrentScoreTime <= 0f;
        float speed = aggressive ? aggressiveSpeed : passiveSpeed;

        // Perseguir / dash
        Vector3 dir = (player.position - transform.position).normalized;
        if (aggressive && dashNonStopToPlayer)
        {
            transform.position += dir * aggressiveSpeed * Time.deltaTime;
        }
        else
        {
            transform.position += dir * speed * Time.deltaTime;
        }

        // Drenar tiempo si está cerca
        float dist = Vector3.Distance(transform.position, player.position);
        if (!aggressive && dist < drainDistance)
        {
            CurrentScoreTime = Mathf.Max(0f, CurrentScoreTime - drainRate * Time.deltaTime);
        }

        // Comportamiento agresivo: rebota entre bordes
        if (aggressive && !dashNonStopToPlayer)
        {
            if (leftBoundary != null && transform.position.x < leftBoundary.position.x)
            {
                transform.position = new Vector3(leftBoundary.position.x, transform.position.y, transform.position.z);
                Vector3 target = rightBoundary != null ? rightBoundary.position : player.position;
                transform.LookAt(target);
            }
            else if (rightBoundary != null && transform.position.x > rightBoundary.position.x)
            {
                transform.position = new Vector3(rightBoundary.position.x, transform.position.y, transform.position.z);
                Vector3 target = leftBoundary != null ? leftBoundary.position : player.position;
                transform.LookAt(target);
            }

            if (dist < explosionDistance && Time.time - lastExplosionTime > explosionCooldown)
            {
                if (explosionPrefab != null)
                    Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                lastExplosionTime = Time.time;
                // Aquí deberías reducir vida del jugador si tu sistema lo permite
            }
        }
    }
}
