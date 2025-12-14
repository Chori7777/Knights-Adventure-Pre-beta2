using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FinalBossSnowTilemapAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private Tilemap[] hazardMaps;
    [SerializeField] private GameObject warningPrefab;
    [SerializeField] private float warningDuration = 0.6f;
    [SerializeField] private float activeDuration = 1.6f;
    [SerializeField] private float gapBetweenMaps = 0.2f;
    [SerializeField] private bool randomOrder = false;
    [SerializeField] private bool autoStartOnEnable = true;

    private Coroutine routine;
    private readonly List<GameObject> spawnedWarnings = new List<GameObject>();

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
        routine = StartCoroutine(RunSequence());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        DeactivateAll();
        ClearWarnings();
    }

    private IEnumerator RunSequence()
    {
        if (hazardMaps == null || hazardMaps.Length == 0)
        {
            OnFinished?.Invoke();
            yield break;
        }
        int[] order = BuildOrder(hazardMaps.Length, randomOrder);
        for (int i = 0; i < order.Length; i++)
        {
            int idx = order[i];
            Tilemap tm = hazardMaps[idx];
            if (tm == null) continue;
            if (warningPrefab != null && warningDuration > 0f)
            {
                SpawnWarningsForTilemap(tm);
                yield return new WaitForSeconds(warningDuration);
            }
            tm.gameObject.SetActive(true);
            yield return new WaitForSeconds(activeDuration);
            tm.gameObject.SetActive(false);
            ClearWarnings();
            yield return new WaitForSeconds(gapBetweenMaps);
        }
        routine = null;
        OnFinished?.Invoke();
    }

    private int[] BuildOrder(int count, bool random)
    {
        int[] arr = new int[count];
        for (int i = 0; i < count; i++) arr[i] = i;
        if (!random) return arr;
        for (int i = 0; i < count; i++)
        {
            int j = UnityEngine.Random.Range(i, count);
            int t = arr[i];
            arr[i] = arr[j];
            arr[j] = t;
        }
        return arr;
    }

    private void SpawnWarningsForTilemap(Tilemap tm)
    {
        BoundsInt bounds = tm.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int c = new Vector3Int(x, y, 0);
                TileBase t = tm.GetTile(c);
                if (t == null) continue;
                Vector3 pos = tm.GetCellCenterWorld(c);
                GameObject w = Instantiate(warningPrefab, pos, Quaternion.identity);
                spawnedWarnings.Add(w);
            }
        }
    }

    private void ClearWarnings()
    {
        for (int i = 0; i < spawnedWarnings.Count; i++)
        {
            var go = spawnedWarnings[i];
            if (go != null) Destroy(go);
        }
        spawnedWarnings.Clear();
    }

    private void DeactivateAll()
    {
        if (hazardMaps == null) return;
        for (int i = 0; i < hazardMaps.Length; i++)
        {
            var tm = hazardMaps[i];
            if (tm == null) continue;
            tm.gameObject.SetActive(false);
        }
    }
}
