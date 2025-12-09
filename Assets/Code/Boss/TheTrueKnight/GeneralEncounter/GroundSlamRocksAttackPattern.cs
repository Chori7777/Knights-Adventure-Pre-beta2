using System;
using System.Collections;
using UnityEngine;

public class GroundSlamRocksAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private int rockCount = 10;
    [SerializeField] private float fallSpeed = 8f;
    [SerializeField] private float spreadX = 8f;
    [SerializeField] private float spawnYOffset = 0.5f;
    [SerializeField] private bool autoStartOnEnable = true;

    private Coroutine routine;

    private void OnEnable()
    {
        if (autoStartOnEnable) StartAttack();
    }

    private void OnDisable()
    {
        StopAttack();
    }

    public void StartAttack()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(RockRoutine());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator RockRoutine()
    {
        if (rockPrefab == null)
        {
            OnFinished?.Invoke();
            yield break;
        }
        Camera cam = Camera.main;
        Vector3 c = cam != null ? cam.transform.position : transform.position;
        float halfH = cam != null ? cam.orthographicSize : 5f;
        float topY = c.y + halfH + spawnYOffset;
        for (int i = 0; i < rockCount; i++)
        {
            float x = c.x + UnityEngine.Random.Range(-spreadX, spreadX);
            Vector3 pos = new Vector3(x, topY, 0f);
            GameObject r = Instantiate(rockPrefab, pos, Quaternion.identity);
            var rb = r.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.down * fallSpeed;
            yield return null;
        }
        routine = null;
        OnFinished?.Invoke();
    }
}
