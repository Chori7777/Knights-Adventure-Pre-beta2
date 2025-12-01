using UnityEngine;
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
