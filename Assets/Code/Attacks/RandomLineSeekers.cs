using System.Collections;
using UnityEngine;

public class RandomLineSeekers : MonoBehaviour
{
    [SerializeField] private GameObject agentPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private int spawnCount = 5;
    [SerializeField] private float spawnInterval = 0.2f;
    [SerializeField] private float agentSpeed = 5f;
    [SerializeField] private float agentLifetime = 5f;
    [SerializeField] private bool autoStartOnEnable = false;
    private Coroutine spawnRoutine;

    public void StartSpawning()
    {
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        Camera cam = Camera.main;
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 v = new Vector3(Random.value, Random.value, 0f);
            Vector3 pos = v;
            if (cam != null) pos = cam.ViewportToWorldPoint(new Vector3(v.x, v.y, cam.nearClipPlane));
            pos.z = 0f;
            GameObject go = Instantiate(agentPrefab, pos, Quaternion.identity);
            var agent = go.AddComponent<SeekLineAgent>();
            agent.player = player;
            agent.speed = agentSpeed;
            agent.lifetime = agentLifetime;
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void OnEnable()
    {
        if (autoStartOnEnable) StartSpawning();
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }
}

public class SeekLineAgent : MonoBehaviour
{
    public Transform player;
    public float speed = 5f;
    public float lifetime = 5f;
    private LineRenderer line;
    private float t;

    private void Awake()
    {
        line = gameObject.GetComponent<LineRenderer>();
        if (line == null) line = gameObject.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.widthMultiplier = 0.05f;
    }

    private void Update()
    {
        t += Time.deltaTime;
        if (t >= lifetime) { Destroy(gameObject); return; }
        if (player != null)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, player.position);
        }
    }
}
