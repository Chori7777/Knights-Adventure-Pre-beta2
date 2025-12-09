using System.Collections;
using UnityEngine;

public class OffsetDropSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform anchor;
    [SerializeField] private float offsetY = 2f;
    [SerializeField] private float speed = 6f;
    [SerializeField] private float extraDrop = 1f;
    [SerializeField] private bool autoSpawnAbove = false;
    [SerializeField] private bool autoSpawnBelow = false;
    [SerializeField] private bool flipYWhenSpawningAbove = true;

    [Header("Rise-Hold-Drop")]
    [SerializeField] private bool enableRiseHoldDrop = false;
    [SerializeField] private float riseOffsetY = 2f;
    [SerializeField] private float riseOffsetX = 2f;
    [SerializeField] private bool riseHorizontalInsteadOfUp = false;
    [SerializeField] private bool riseToRight = true;
    [SerializeField] private float riseSpeed = 3f;
    [SerializeField] private float riseTime = 1.5f;
    [SerializeField] private float holdTime = 1f;
    [SerializeField] private float downSpeed = 3f;
    [SerializeField] private float downTime = 1.5f;
    [SerializeField] private bool destroyOnEnd = true;
    [SerializeField] private bool useRaycastForTop = false;
    [SerializeField] private LayerMask topLayer;
    [SerializeField] private float topRayDistance = 0f;
    [SerializeField] private float topStopPadding = 0.05f;

    [Header("Warning")]
    [SerializeField] private bool showWarning = false;
    [SerializeField] private GameObject warningPrefab;
    [SerializeField] private float warningDuration = 1f;
    [SerializeField] private float warningBlinkInterval = 0.15f;

    public void SpawnAbove()
    {
        if (showWarning)
            StartCoroutine(WarningThenSpawn(true));
        else
            Spawn(true);
    }

    public void SpawnBelow()
    {
        if (showWarning)
            StartCoroutine(WarningThenSpawn(false));
        else
            Spawn(false);
    }

    private void Spawn(bool above)
    {
        if (prefab == null) return;
        Vector3 basePos = anchor != null ? anchor.position : transform.position;
        float sign = above ? 1f : -1f;
        GameObject go = Instantiate(prefab, basePos + Vector3.up * offsetY * sign, Quaternion.identity);
        ApplyVerticalFlip(go, above);
        StartCoroutine(DropRoutine(go, basePos, sign));
    }

    private IEnumerator DropRoutine(GameObject go, Vector3 basePos, float sign)
    {
        if (go == null) yield break;
        while (Vector3.Distance(go.transform.position, basePos) > 0.01f)
        {
            go.transform.position = Vector3.MoveTowards(go.transform.position, basePos, speed * Time.deltaTime);
            yield return null;
        }
        Vector3 target = basePos + Vector3.down * extraDrop;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            go.transform.position = Vector3.MoveTowards(go.transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
        Destroy(go);
    }

    private void OnEnable()
    {
        if (autoSpawnAbove) SpawnAbove();
        if (autoSpawnBelow) SpawnBelow();
        if (enableRiseHoldDrop) SpawnRiseHoldDrop();
    }

    public void SpawnRiseHoldDrop()
    {
        if (prefab == null) return;
        if (showWarning)
        {
            StartCoroutine(WarningThenRise());
            return;
        }
        Vector3 basePos = anchor != null ? anchor.position : transform.position;
        GameObject go = Instantiate(prefab, basePos, Quaternion.identity);
        StartCoroutine(RiseHoldDropRoutine(go, basePos));
    }

    private IEnumerator RiseHoldDropRoutine(GameObject go, Vector3 basePos)
    {
        if (go == null) yield break;
        Vector3 topPos = basePos + (riseHorizontalInsteadOfUp ? (riseToRight ? Vector3.right * riseOffsetX : Vector3.left * riseOffsetX) : Vector3.up * riseOffsetY);
        if (useRaycastForTop)
        {
            if (riseHorizontalInsteadOfUp)
            {
                float distH = topRayDistance > 0f ? topRayDistance : riseOffsetX;
                Vector2 dirH = riseToRight ? Vector2.right : Vector2.left;
                var hitH = Physics2D.Raycast(basePos, dirH, distH, topLayer);
                if (hitH.collider != null)
                    topPos = new Vector3(hitH.point.x - (riseToRight ? topStopPadding : -topStopPadding), basePos.y, basePos.z);
            }
            else
            {
                float dist = topRayDistance > 0f ? topRayDistance : riseOffsetY;
                var hit = Physics2D.Raycast(basePos, Vector2.up, dist, topLayer);
                if (hit.collider != null)
                    topPos = new Vector3(basePos.x, hit.point.y - topStopPadding, basePos.z);
            }
        }
        float elapsed = 0f;
        while (elapsed < riseTime)
        {
            elapsed += Time.deltaTime;
            go.transform.position = Vector3.MoveTowards(go.transform.position, topPos, riseSpeed * Time.deltaTime);
            yield return null;
        }
        float holdElapsed = 0f;
        while (holdElapsed < holdTime)
        {
            holdElapsed += Time.deltaTime;
            yield return null;
        }
        float downElapsed = 0f;
        while (downElapsed < downTime)
        {
            downElapsed += Time.deltaTime;
            go.transform.position = Vector3.MoveTowards(go.transform.position, basePos, downSpeed * Time.deltaTime);
            yield return null;
        }
        if (destroyOnEnd && go != null) Destroy(go);
    }

    private void ApplyVerticalFlip(GameObject go, bool above)
    {
        if (go == null || !flipYWhenSpawningAbove) return;
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.flipY = above;
            return;
        }
        Vector3 sc = go.transform.localScale;
        float y = Mathf.Abs(sc.y);
        sc.y = above ? -y : y;
        go.transform.localScale = sc;
    }

    private IEnumerator WarningThenSpawn(bool above)
    {
        Vector3 basePos = anchor != null ? anchor.position : transform.position;
        GameObject warn = null;
        if (warningPrefab != null)
            warn = Instantiate(warningPrefab, basePos, Quaternion.identity);
        yield return StartCoroutine(BlinkWarning(warn));
        if (warn != null) Destroy(warn);
        Spawn(above);
    }

    private IEnumerator WarningThenRise()
    {
        Vector3 basePos = anchor != null ? anchor.position : transform.position;
        GameObject warn = null;
        if (warningPrefab != null)
            warn = Instantiate(warningPrefab, basePos, Quaternion.identity);
        yield return StartCoroutine(BlinkWarning(warn));
        if (warn != null) Destroy(warn);
        GameObject go = Instantiate(prefab, basePos, Quaternion.identity);
        StartCoroutine(RiseHoldDropRoutine(go, basePos));
    }

    private IEnumerator BlinkWarning(GameObject warn)
    {
        float elapsed = 0f;
        bool state = true;
        if (warn != null) warn.SetActive(true);
        while (elapsed < warningDuration)
        {
            elapsed += warningBlinkInterval;
            state = !state;
            if (warn != null) warn.SetActive(state);
            yield return new WaitForSeconds(warningBlinkInterval);
        }
        if (warn != null) warn.SetActive(true);
    }
}
