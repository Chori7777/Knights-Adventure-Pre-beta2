using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TheTrueKnightAttackManager : MonoBehaviour
{
    [SerializeField] private bossCore core;
    [SerializeField] private Animator bossAnimator;

    [SerializeField] private GameObject tilemapA;
    [SerializeField] private Transform projectileSpawn;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int spawnCount = 5;
    [SerializeField] private float spawnSpreadDegrees = 30f;
    [SerializeField] private float projectileSpeed = 12f;

    [Header("Extra Prefabs")]
    [SerializeField] private GameObject knifePrefab;
    [SerializeField] private GameObject slashPrefab;
    [SerializeField] private GameObject rockPrefab;

    [Header("Bouncing Knives")]
    [SerializeField] private int bouncingKnifeCount = 6;
    [SerializeField] private float bouncingKnifeSpeed = 10f;
    [SerializeField] private int bouncingKnifeMaxBounces = 4;
    [SerializeField] private float bouncingKnifeLifetime = 6f;

    [Header("Front Slices")]
    [SerializeField] private int frontSliceCount = 5;
    [SerializeField] private float frontSliceInterval = 0.08f;
    [SerializeField] private float frontSliceSpeed = 12f;

    [Header("Ground Slam Rocks")]
    [SerializeField] private int groundRockCount = 10;
    [SerializeField] private float groundRockFallSpeed = 8f;
    [SerializeField] private float groundRockSpreadX = 8f;

    [System.Serializable]
    public class AttackEntry
    {
        public string name;
        public GameObject root;
        public Grid grid;
        public bool activateChildren = true;
    }

    [SerializeField] private List<GameObject> bossObjects = new List<GameObject>();
    [SerializeField] private AttackEntry[] activationEntries;

    public void Play(string attackName)
    {
        if (string.IsNullOrEmpty(attackName)) return;

        if (activationEntries != null)
        {
            for (int i = 0; i < activationEntries.Length; i++)
            {
                var e = activationEntries[i];
                if (e != null && e.name == attackName)
                {
                    ActivateEntry(e);
                    return;
                }
            }
        }

        switch (attackName)
        {
            case "attack_0":
                Attack0();
                break;
            case "tilemap_A":
                AttackTilemapA();
                break;
            case "spawn_proj":
                AttackSpawnProjectiles();
                break;
            case "activate_objs":
                AttackActivateObjects();
                break;
            case "burst_center":
                AttackBurstCenter();
                break;
            case "bounce_knives":
                AttackBouncingKnives();
                break;
            case "front_slices":
                StartCoroutine(AttackFrontSlices());
                break;
            case "ground_slam_rocks":
                AttackGroundSlamRocks();
                break;
        }
    }

    private void Attack0()
    {
        if (bossAnimator == null && core != null) bossAnimator = core.anim;
        if (bossAnimator != null) bossAnimator.SetTrigger("Attack");

        if (core == null || projectilePrefab == null || projectileSpawn == null) return;

        Vector2 dir = Vector2.right;
        if (core.player != null)
        {
            dir = ((Vector2)core.player.position - (Vector2)projectileSpawn.position).normalized;
        }

        GameObject p = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.identity);
        var rb = p.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = dir * projectileSpeed;
        }
        else
        {
            p.transform.right = new Vector3(dir.x, dir.y, 0f);
        }
    }

    private void AttackTilemapA()
    {
        if (tilemapA == null) return;
        tilemapA.SetActive(true);
    }

    private void AttackSpawnProjectiles()
    {
        if (core == null || projectilePrefab == null || projectileSpawn == null) return;

        Vector2 baseDir = Vector2.right;
        if (core.player != null)
        {
            baseDir = ((Vector2)core.player.position - (Vector2)projectileSpawn.position).normalized;
        }

        float half = spawnSpreadDegrees * 0.5f;
        for (int i = 0; i < spawnCount; i++)
        {
            float t = spawnCount <= 1 ? 0f : (float)i / (spawnCount - 1);
            float angle = Mathf.Lerp(-half, half, t);
            Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDir;

            GameObject p = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.identity);
            var rb = p.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = dir.normalized * projectileSpeed;
            }
            else
            {
                p.transform.right = new Vector3(dir.x, dir.y, 0f);
            }
        }
    }

    private void AttackBurstCenter()
    {
        Transform spawn = projectileSpawn != null ? projectileSpawn : transform;
        GameObject prefab = knifePrefab != null ? knifePrefab : projectilePrefab;
        if (prefab == null || spawn == null) return;
        Vector2 baseDir = Vector2.right;
        if (core != null && core.player != null)
        {
            baseDir = ((Vector2)core.player.position - (Vector2)spawn.position).normalized;
        }
        float half = spawnSpreadDegrees * 0.5f;
        int count = Mathf.Max(1, 5);
        for (int i = 0; i < count; i++)
        {
            float t = count <= 1 ? 0f : (float)i / (count - 1);
            float angle = Mathf.Lerp(-half, half, t);
            Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDir;
            GameObject p = Instantiate(prefab, spawn.position, Quaternion.identity);
            var rb = p.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = dir.normalized * projectileSpeed;
            else p.transform.right = new Vector3(dir.x, dir.y, 0f);
        }
    }

    private void AttackBouncingKnives()
    {
        Transform spawn = projectileSpawn != null ? projectileSpawn : transform;
        GameObject prefab = knifePrefab != null ? knifePrefab : projectilePrefab;
        if (prefab == null || spawn == null) return;
        Camera cam = Camera.main;
        Vector3 c = cam != null ? cam.transform.position : spawn.position;
        float halfH = cam != null ? cam.orthographicSize : 5f;
        float halfW = cam != null ? halfH * cam.aspect : 8f;
        for (int i = 0; i < bouncingKnifeCount; i++)
        {
            Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
            Vector3 pos = spawn.position;
            GameObject p = Instantiate(prefab, pos, Quaternion.identity);
            var rb = p.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = dir * bouncingKnifeSpeed;
            }
            var bounce = p.AddComponent<BouncingKnife>();
            bounce.Init(bouncingKnifeSpeed, bouncingKnifeMaxBounces, bouncingKnifeLifetime, c, halfW, halfH);
        }
    }

    private IEnumerator AttackFrontSlices()
    {
        Transform spawn = projectileSpawn != null ? projectileSpawn : transform;
        GameObject prefab = slashPrefab != null ? slashPrefab : projectilePrefab;
        if (prefab == null || spawn == null) yield break;
        Vector2 dir = Vector2.right;
        if (core != null && core.player != null)
        {
            dir = ((Vector2)core.player.position - (Vector2)spawn.position).normalized;
        }
        for (int i = 0; i < frontSliceCount; i++)
        {
            GameObject p = Instantiate(prefab, spawn.position, Quaternion.identity);
            var rb = p.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = dir * frontSliceSpeed;
            else p.transform.right = new Vector3(dir.x, dir.y, 0f);
            yield return new WaitForSeconds(frontSliceInterval);
        }
    }

    private void AttackGroundSlamRocks()
    {
        GameObject prefab = rockPrefab != null ? rockPrefab : projectilePrefab;
        if (prefab == null) return;
        Camera cam = Camera.main;
        float halfH = cam != null ? cam.orthographicSize : 5f;
        Vector3 center = cam != null ? cam.transform.position : transform.position;
        float topY = center.y + halfH + 0.5f;
        for (int i = 0; i < groundRockCount; i++)
        {
            float x = center.x + UnityEngine.Random.Range(-groundRockSpreadX, groundRockSpreadX);
            Vector3 pos = new Vector3(x, topY, 0f);
            GameObject rock = Instantiate(prefab, pos, Quaternion.identity);
            var rb = rock.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.down * groundRockFallSpeed;
        }
    }
    private void AttackActivateObjects()
    {
        if (bossObjects == null) return;
        for (int i = 0; i < bossObjects.Count; i++)
        {
            var obj = bossObjects[i];
            if (obj == null) continue;
            obj.SetActive(true);
        }
    }

    private void ActivateEntry(AttackEntry entry)
    {
        if (entry.grid != null)
        {
            entry.grid.gameObject.SetActive(true);
        }
        if (entry.root != null)
        {
            entry.root.SetActive(true);
            if (entry.activateChildren)
            {
                for (int i = 0; i < entry.root.transform.childCount; i++)
                {
                    var child = entry.root.transform.GetChild(i);
                    child.gameObject.SetActive(true);
                }
            }
        }
    }
}
