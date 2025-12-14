using UnityEngine;
using System;

public class FallingSwordAttack : MonoBehaviour, IAttackPattern
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject swordPrefab;
    [SerializeField] private GameObject squarePrefab;
    [SerializeField] private float spawnHeight = 6f;
    [SerializeField] private float fallSpeed = 15f;
    [SerializeField] private float riseSpeed = 12f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private bool autoFireOnEnable = false;
    [SerializeField] private bool loop = false;
    [SerializeField] private float loopDelay = 0.5f;

    private Transform sword;
    private Transform square;
    private Vector3 targetPos;
    private int state;
    private float nextFireTime;
    public event Action OnFinished;
    private bool singleRun;

    private void OnEnable()
    {
        if (autoFireOnEnable) Fire();
    }

    private void OnDisable()
    {
        if (sword != null) Destroy(sword.gameObject);
        if (square != null) Destroy(square.gameObject);
        state = 0;
    }

    public void Fire()
    {
        if (player == null || swordPrefab == null)
        {
            if (singleRun) OnFinished?.Invoke();
            return;
        }
        targetPos = player.position;
        if (squarePrefab != null) square = Instantiate(squarePrefab, targetPos, Quaternion.identity).transform;
        sword = Instantiate(swordPrefab, targetPos + Vector3.up * spawnHeight, Quaternion.identity).transform;
        state = 1;
    }

    private void Update()
    {
        if (state == 0)
        {
            if (loop && gameObject.activeInHierarchy && Time.time >= nextFireTime)
                Fire();
            return;
        }
        if (state == 1)
        {
            if (sword == null) { state = 0; return; }
            Vector3 p = sword.position;
            Vector3 v = Vector3.down * fallSpeed * Time.deltaTime;
            sword.position = p + v;
            bool hitGround = Physics2D.Raycast(sword.position, Vector2.down, groundCheckDistance, groundLayer);
            if (sword.position.y <= targetPos.y || hitGround)
            {
                state = 2;
            }
        }
        else if (state == 2)
        {
            if (sword == null) { state = 0; return; }
            Vector3 p = sword.position;
            Vector3 top = targetPos + Vector3.up * spawnHeight;
            sword.position = Vector3.MoveTowards(p, top, riseSpeed * Time.deltaTime);
            if (Vector3.Distance(sword.position, top) < 0.01f)
            {
                if (sword != null) Destroy(sword.gameObject);
                if (square != null) Destroy(square.gameObject);
                state = 0;
                if (singleRun)
                {
                    singleRun = false;
                    OnFinished?.Invoke();
                }
                else if (loop)
                {
                    nextFireTime = Time.time + loopDelay;
                }
            }
        }
    }

    public void StartAttack()
    {
        singleRun = true;
        Fire();
    }

    public void StopAttack()
    {
        singleRun = false;
        if (sword != null) Destroy(sword.gameObject);
        if (square != null) Destroy(square.gameObject);
        state = 0;
    }
}
