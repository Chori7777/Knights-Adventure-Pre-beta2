using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class FirstEncounterBossAttackManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private GameObject wandPrefab;

    [Header("Puntos de Wand (random)")]
    [SerializeField] private Transform[] wandPoints;

    [Header("Configuración general")]
    [SerializeField] private float indicatorTime = 1f;
    [SerializeField] private Color indicatorColor = Color.red;
    [SerializeField] private float attackIntervalPhase1 = 3f;
    [SerializeField] private float attackIntervalPhase2 = 2f;

    [Header("Parámetros de Overhead")]
    [SerializeField] private int overheadCount = 5;
    [SerializeField] private float overheadSpread = 6f;
    [SerializeField] private float overheadTopMargin = 0f;
    [SerializeField] private float overheadPlayerYOffset = 3f;

    [Header("Parámetros de Orbes Laterales")]
    [SerializeField] private float sideOrbDuration = 2.2f;
    [SerializeField] private float sideOrbEdgeOffset = 0.5f;
    [SerializeField] private bool sideOrbsTrackPlayerY = true;
    [SerializeField] private float sideOrbsY = 0f;
    [SerializeField] private bool sideOrbsFollowDuringMove = false;

    private bool running;
    private Coroutine loopRoutine;
    private System.Collections.Generic.List<GameObject> spawnedObjects = new System.Collections.Generic.List<GameObject>();
    [SerializeField] private float fallbackLifetime = 4f;

    public void StartTrialAttacks(bool phase2)
    {
        StopAttacks();
        running = true;
        loopRoutine = StartCoroutine(AttackLoop(phase2));
    }

    public void StopAttacks()
    {
        running = false;
        if (loopRoutine != null)
        {
            StopCoroutine(loopRoutine);
            loopRoutine = null;
        }
        ClearSpawnedObjects();
    }

    private IEnumerator AttackLoop(bool phase2)
    {
        float interval = phase2 ? attackIntervalPhase2 : attackIntervalPhase1;
        int lastPick = -1;
        while (running)
        {
            int options = phase2 ? 4 : 3;
            int pick = Random.Range(0, options);
            if (pick == lastPick)
            {
                pick = (pick + 1) % options;
            }
            if (pick == 0)
                yield return LaunchOverheadVolley();
            else if (pick == 1)
                yield return LaunchGroundSpikes();
            else if (pick == 2)
                yield return LaunchWandBurst();
            else
                yield return LaunchSideSweepOrbs();
            lastPick = pick;
            if (!running) yield break;
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator LaunchOverheadVolley()
    {
        if (projectilePrefab == null) yield break;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;
        Vector3 targetIndicator = player.transform.position;
        var cam = Camera.main;
        if (cam == null) yield break;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        float leftX = cam.transform.position.x - halfWidth;
        float rightX = cam.transform.position.x + halfWidth;
        float topY = cam.transform.position.y + halfHeight;

        int count = Mathf.Max(1, overheadCount);
        float spread = overheadSpread;
        var spawnPositions = new List<Vector3>(count);
        for (int i = 0; i < count; i++)
        {
            float offX = (i - count / 2f) * (spread / count);
            float x = Mathf.Clamp(targetIndicator.x + offX, leftX, rightX);
            float ySpawn = player.transform.position.y + overheadPlayerYOffset;
            ySpawn = Mathf.Min(ySpawn, topY - overheadTopMargin);
            Vector3 spawnPos = new Vector3(x, ySpawn, 0f);
            DrawIndicator(spawnPos, targetIndicator);
            spawnPositions.Add(spawnPos);
        }
        yield return new WaitForSeconds(indicatorTime);

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = spawnPositions[i];
            Vector3 targetFire = targetIndicator;
            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            RegisterSpawn(proj);
            Sequence seq = DOTween.Sequence();
            seq.Append(proj.transform.DOMove(spawnPos + Vector3.up * 1.5f, 0.25f).SetEase(Ease.OutQuad));
            seq.Append(proj.transform.DOMove(targetFire, 0.8f).SetEase(Ease.InQuad));
            seq.OnComplete(() => Destroy(proj));
        }
    }

    private IEnumerator LaunchGroundSpikes()
    {
        if (spikePrefab == null) yield break;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;
        Vector3 p = player.transform.position;
        DrawIndicator(p + Vector3.down * 0.5f, p + Vector3.up * 2f);
        yield return new WaitForSeconds(indicatorTime);
        var spike = Instantiate(spikePrefab, new Vector3(p.x, p.y - 0.1f, 0f), Quaternion.identity);
        RegisterSpawn(spike);
        Destroy(spike, fallbackLifetime);
    }

    private IEnumerator LaunchWandBurst()
    {
        if (wandPrefab == null || projectilePrefab == null) yield break;
        Vector3 pos = GetRandomWandPoint();
        GameObject wand = Instantiate(wandPrefab, pos, Quaternion.identity);
        RegisterSpawn(wand);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Destroy(wand);
            yield break;
        }
        Vector3 target = player.transform.position;
        DrawIndicator(pos, target);
        DrawIndicator(pos, target + new Vector3(3f, 0f, 0f));
        DrawIndicator(pos, target + new Vector3(-3f, 0f, 0f));
        yield return new WaitForSeconds(indicatorTime);

        FireProjectile(pos, target);
        FireProjectile(pos, target + new Vector3(3f, 0f, 0f));
        FireProjectile(pos, target + new Vector3(-3f, 0f, 0f));

        yield return new WaitForSeconds(1f);
        Destroy(wand);
    }

    private void FireProjectile(Vector3 from, Vector3 to)
    {
        GameObject proj = Instantiate(projectilePrefab, from, Quaternion.identity);
        RegisterSpawn(proj);
        float dist = Vector3.Distance(from, to);
        float dur = Mathf.Clamp(dist / 12f, 0.4f, 1.2f);
        proj.transform.DOMove(to, dur).SetEase(Ease.InQuad).OnComplete(() => Destroy(proj));
    }

    private IEnumerator LaunchSideSweepOrbs()
    {
        if (projectilePrefab == null) yield break;
        var cam = Camera.main;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (cam == null || player == null) yield break;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        float leftX = cam.transform.position.x - halfWidth - sideOrbEdgeOffset;
        float rightX = cam.transform.position.x + halfWidth + sideOrbEdgeOffset;

        float y = sideOrbsTrackPlayerY ? player.transform.position.y : sideOrbsY;

        DrawIndicator(new Vector3(leftX, y, 0f), new Vector3(rightX, y, 0f));
        DrawIndicator(new Vector3(rightX, y, 0f), new Vector3(leftX, y, 0f));
        yield return new WaitForSeconds(indicatorTime);

        GameObject orbLeft = Instantiate(projectilePrefab, new Vector3(leftX, y, 0f), Quaternion.identity);
        GameObject orbRight = Instantiate(projectilePrefab, new Vector3(rightX, y, 0f), Quaternion.identity);
        RegisterSpawn(orbLeft);
        RegisterSpawn(orbRight);

        float dur = sideOrbDuration;
        var twLeft = orbLeft.transform.DOMoveX(rightX, dur).SetEase(Ease.Linear);
        twLeft.OnUpdate(() =>
        {
            if (sideOrbsFollowDuringMove && sideOrbsTrackPlayerY && player != null)
            {
                var pos = orbLeft.transform.position;
                pos.y = player.transform.position.y;
                orbLeft.transform.position = pos;
            }
        }).OnComplete(() => Destroy(orbLeft));

        var twRight = orbRight.transform.DOMoveX(leftX, dur).SetEase(Ease.Linear);
        twRight.OnUpdate(() =>
        {
            if (sideOrbsFollowDuringMove && sideOrbsTrackPlayerY && player != null)
            {
                var pos = orbRight.transform.position;
                pos.y = player.transform.position.y;
                orbRight.transform.position = pos;
            }
        }).OnComplete(() => Destroy(orbRight));
    }

    private void DrawIndicator(Vector3 from, Vector3 to)
    {
        GameObject go = new GameObject("AttackIndicator");
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, from);
        lr.SetPosition(1, to);
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = indicatorColor;
        lr.endColor = indicatorColor;
        Destroy(go, indicatorTime + 0.1f);
    }

    private Vector3 GetRandomWandPoint()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            GameObject playerFallback = GameObject.FindGameObjectWithTag("Player");
            if (playerFallback != null) return playerFallback.transform.position;
            return Vector3.zero;
        }
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        float leftX = cam.transform.position.x - halfWidth;
        float rightX = cam.transform.position.x + halfWidth;
        float bottomY = cam.transform.position.y - halfHeight;
        float topY = cam.transform.position.y + halfHeight;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float x = Mathf.Clamp(player.transform.position.x + Random.Range(-3f, 3f), leftX, rightX);
            float y = Mathf.Clamp(player.transform.position.y + Random.Range(2f, 4f), bottomY, topY);
            return new Vector3(x, y, 0f);
        }
        float rx = Random.Range(leftX, rightX);
        float ry = Random.Range(bottomY, topY);
        return new Vector3(rx, ry, 0f);
    }

    private void RegisterSpawn(GameObject go)
    {
        if (go == null) return;
        spawnedObjects.Add(go);
    }

    public void ClearSpawnedObjects()
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            var go = spawnedObjects[i];
            if (go != null)
            {
                Destroy(go);
            }
        }
        spawnedObjects.Clear();
    }
}
