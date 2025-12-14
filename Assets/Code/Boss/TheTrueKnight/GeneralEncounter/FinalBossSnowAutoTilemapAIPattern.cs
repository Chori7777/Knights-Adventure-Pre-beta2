using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FinalBossSnowAutoTilemapAIPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private TileBase dangerTile;
    [SerializeField] private GameObject warningPrefab;
    [SerializeField] private Transform areaLeft;
    [SerializeField] private Transform areaRight;
    [SerializeField] private Transform areaBottom;
    [SerializeField] private Transform areaTop;
    [SerializeField] private int patternsCount = 3;
    [SerializeField] private float warningDuration = 0.6f;
    [SerializeField] private float activeDuration = 1.5f;
    [SerializeField] private float gapBetweenPatterns = 0.25f;
    [SerializeField] private int stripeWidth = 2;
    [SerializeField] private int randomBlockSize = 3;
    [SerializeField] private float safeLaneChance = 0.3f;
    [SerializeField] private bool randomOrder = true;
    [SerializeField] private bool autoStartOnEnable = true;

    private Coroutine routine;
    private Grid grid;
    private Tilemap tilemap;
    private readonly List<GameObject> warnings = new List<GameObject>();

    private enum PatternKind { VerticalStripes, HorizontalStripes, Checker, Cross, Diagonal, RandomBlocks, PlayerRing }

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
        routine = StartCoroutine(RunAI());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        ClearTiles();
        ClearWarnings();
        DestroyTilemap();
    }

    private IEnumerator RunAI()
    {
        if (dangerTile == null)
        {
            OnFinished?.Invoke();
            yield break;
        }
        EnsureTilemap();
        var kinds = BuildOrder();
        for (int i = 0; i < Mathf.Max(1, patternsCount); i++)
        {
            PatternKind kind = kinds[i % kinds.Length];
            var cells = GenerateCells(kind);
            SpawnWarnings(cells);
            if (warningDuration > 0f) yield return new WaitForSeconds(warningDuration);
            PaintTiles(cells);
            yield return new WaitForSeconds(activeDuration);
            ClearTiles();
            ClearWarnings();
            if (gapBetweenPatterns > 0f) yield return new WaitForSeconds(gapBetweenPatterns);
        }
        DestroyTilemap();
        routine = null;
        OnFinished?.Invoke();
    }

    private void EnsureTilemap()
    {
        if (grid == null)
        {
            var go = new GameObject("SnowAutoTilemapGrid");
            go.transform.SetParent(transform);
            go.transform.position = Vector3.zero;
            grid = go.AddComponent<Grid>();
        }
        if (tilemap == null)
        {
            var tmGo = new GameObject("SnowAutoTilemap");
            tmGo.transform.SetParent(grid.transform);
            tmGo.transform.localPosition = Vector3.zero;
            tilemap = tmGo.AddComponent<Tilemap>();
            tmGo.AddComponent<TilemapRenderer>();
        }
    }

    private void DestroyTilemap()
    {
        if (tilemap != null)
        {
            Destroy(tilemap.gameObject);
            tilemap = null;
        }
        if (grid != null)
        {
            Destroy(grid.gameObject);
            grid = null;
        }
    }

    private Vector3Int WorldToCell(Vector3 w)
    {
        return tilemap.WorldToCell(w);
    }

    private Vector3 CellCenter(Vector3Int c)
    {
        return tilemap.GetCellCenterWorld(c);
    }

    private Rect GetArea()
    {
        float left = areaLeft != null ? areaLeft.position.x : Camera.main.transform.position.x - Camera.main.orthographicSize * Camera.main.aspect;
        float right = areaRight != null ? areaRight.position.x : Camera.main.transform.position.x + Camera.main.orthographicSize * Camera.main.aspect;
        float bottom = areaBottom != null ? areaBottom.position.y : Camera.main.transform.position.y - Camera.main.orthographicSize;
        float top = areaTop != null ? areaTop.position.y : Camera.main.transform.position.y + Camera.main.orthographicSize;
        return Rect.MinMaxRect(left, bottom, right, top);
    }

    private List<Vector3Int> GenerateCells(PatternKind kind)
    {
        var cells = new List<Vector3Int>();
        Rect a = GetArea();
        Vector3Int min = WorldToCell(new Vector3(a.xMin, a.yMin, 0f));
        Vector3Int max = WorldToCell(new Vector3(a.xMax, a.yMax, 0f));
        if (min.x > max.x) { int t = min.x; min.x = max.x; max.x = t; }
        if (min.y > max.y) { int t = min.y; min.y = max.y; max.y = t; }
        int safeLaneX = Mathf.RoundToInt(UnityEngine.Random.Range(min.x, max.x));
        int safeLaneY = Mathf.RoundToInt(UnityEngine.Random.Range(min.y, max.y));
        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                bool include = false;
                switch (kind)
                {
                    case PatternKind.VerticalStripes:
                        include = (Mathf.Abs(x - min.x) % (stripeWidth * 2)) < stripeWidth;
                        break;
                    case PatternKind.HorizontalStripes:
                        include = (Mathf.Abs(y - min.y) % (stripeWidth * 2)) < stripeWidth;
                        break;
                    case PatternKind.Checker:
                        include = ((x + y) % 2) == 0;
                        break;
                    case PatternKind.Cross:
                        include = x == safeLaneX || y == safeLaneY;
                        break;
                    case PatternKind.Diagonal:
                        include = ((x - min.x) == (y - min.y)) || ((x - min.x) == (max.y - y));
                        break;
                    case PatternKind.RandomBlocks:
                        include = UnityEngine.Random.value < 0.5f;
                        break;
                    case PatternKind.PlayerRing:
                        var cam = Camera.main;
                        Vector3 playerPos = cam != null ? cam.transform.position : Vector3.zero;
                        Vector3Int pc = WorldToCell(playerPos);
                        int dx = Mathf.Abs(x - pc.x);
                        int dy = Mathf.Abs(y - pc.y);
                        include = Mathf.Abs(dx + dy - randomBlockSize) <= 1;
                        break;
                }
                if (include)
                {
                    bool makeSafeLane = UnityEngine.Random.value < safeLaneChance;
                    if (makeSafeLane && (x == safeLaneX || y == safeLaneY)) continue;
                    cells.Add(new Vector3Int(x, y, 0));
                }
            }
        }
        return cells;
    }

    private void PaintTiles(List<Vector3Int> cells)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            tilemap.SetTile(cells[i], dangerTile);
        }
    }

    private void ClearTiles()
    {
        if (tilemap == null) return;
        tilemap.ClearAllTiles();
    }

    private void SpawnWarnings(List<Vector3Int> cells)
    {
        if (warningPrefab == null) return;
        for (int i = 0; i < cells.Count; i++)
        {
            Vector3 pos = CellCenter(cells[i]);
            var w = GameObject.Instantiate(warningPrefab, pos, Quaternion.identity);
            warnings.Add(w);
        }
    }

    private void ClearWarnings()
    {
        for (int i = 0; i < warnings.Count; i++)
        {
            var w = warnings[i];
            if (w != null) Destroy(w);
        }
        warnings.Clear();
    }

    private PatternKind[] BuildOrder()
    {
        var kinds = new List<PatternKind>
        {
            PatternKind.VerticalStripes,
            PatternKind.HorizontalStripes,
            PatternKind.Checker,
            PatternKind.Cross,
            PatternKind.Diagonal,
            PatternKind.RandomBlocks,
            PatternKind.PlayerRing
        };
        if (!randomOrder) return kinds.ToArray();
        for (int i = 0; i < kinds.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, kinds.Count);
            var t = kinds[i];
            kinds[i] = kinds[j];
            kinds[j] = t;
        }
        return kinds.ToArray();
    }
}
