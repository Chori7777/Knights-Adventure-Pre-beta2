using System.Collections;
using UnityEngine;

public class SimpleObjectMoveTrigger : MonoBehaviour
{
    [SerializeField] private Transform objectReference;
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform destination;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private bool despawnOnEnd = false;

    private bool triggered;
    private Transform runtimeObject;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        triggered = true;

        runtimeObject = null;
        if (objectReference != null)
        {
            if (!objectReference.gameObject.scene.IsValid())
            {
                var src = objectReference.gameObject;
                var pos = spawnPoint != null ? spawnPoint.position : transform.position;
                var go = Instantiate(src, pos, Quaternion.identity);
                runtimeObject = go.transform;
            }
            else
            {
                runtimeObject = objectReference;
            }
        }
        else if (objectPrefab != null)
        {
            var pos = spawnPoint != null ? spawnPoint.position : transform.position;
            var go = Instantiate(objectPrefab, pos, Quaternion.identity);
            runtimeObject = go.transform;
        }

        if (runtimeObject != null && destination != null)
        {
            StartCoroutine(MoveRoutine());
        }
    }

    private IEnumerator MoveRoutine()
    {
        Transform obj = runtimeObject;
        float t = 0f;
        float dist = Vector3.Distance(obj.position, destination.position);
        float maxTime = Mathf.Max(0.1f, dist / Mathf.Max(0.01f, moveSpeed));
        while (t < maxTime)
        {
            t += Time.deltaTime;
            if (obj != null && destination != null)
            {
                Vector3 p = obj.position;
                Vector3 d = destination.position;
                obj.position = Vector3.MoveTowards(p, d, moveSpeed * Time.deltaTime);
            }
            yield return null;
        }
        if (despawnOnEnd && runtimeObject != null)
        {
            Destroy(runtimeObject.gameObject);
            runtimeObject = null;
        }
    }
}

