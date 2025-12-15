using UnityEngine;

public class ChaserSpawner : MonoBehaviour
{
    [SerializeField] private GameObject chaserPrefab;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxActive = 6;
    [SerializeField] private float spawnMargin = 1f;
    [SerializeField] private bool spawnAtEdges = true;
    [SerializeField] private bool aggressiveOnSpawn = true;
    [SerializeField] private bool phaseThroughWallsOnSpawn = true;
    [SerializeField] private float chaserSpeed = 6f;

    private float lastSpawnTime = -999f;

    private void Update()
    {
        if (chaserPrefab == null) return;
        if (Time.time < lastSpawnTime + spawnInterval) return;
        int count = 0;
        var existing = FindObjectsByType<SimpleChaser>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        count = existing != null ? existing.Length : 0;
        if (count >= maxActive) return;
        Vector3 pos = GetRandomSpawnPosition();
        var go = Instantiate(chaserPrefab, pos, Quaternion.identity);
        go.tag = "enemy";
        var chaser = go.GetComponent<SimpleChaser>();
        if (chaser != null)
        {
            if (aggressiveOnSpawn) chaser.SetAggressive(true);
            if (phaseThroughWallsOnSpawn) chaser.SetPhaseThroughWalls(true);
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) sr = go.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    Vector2 dir = (player.transform.position - go.transform.position).normalized;
                    sr.flipX = dir.x < 0f;
                }
            }
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null) rb.isKinematic = true;
        }
        lastSpawnTime = Time.time;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        var cam = Camera.main;
        if (cam == null || !cam.orthographic) return transform.position;
        Vector3 camPos = cam.transform.position;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float left = camPos.x - halfW - spawnMargin;
        float right = camPos.x + halfW + spawnMargin;
        float bottom = camPos.y - halfH - spawnMargin;
        float top = camPos.y + halfH + spawnMargin;
        if (spawnAtEdges)
        {
            int edge = Random.Range(0, 4);
            switch (edge)
            {
                case 0: return new Vector3(left, Random.Range(bottom, top), 0f);
                case 1: return new Vector3(right, Random.Range(bottom, top), 0f);
                case 2: return new Vector3(Random.Range(left, right), top, 0f);
                default: return new Vector3(Random.Range(left, right), bottom, 0f);
            }
        }
        else
        {
            float x = Random.Range(left + spawnMargin, right - spawnMargin);
            float y = Random.Range(bottom + spawnMargin, top - spawnMargin);
            return new Vector3(x, y, 0f);
        }
    }
}
