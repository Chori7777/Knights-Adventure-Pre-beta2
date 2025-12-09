using UnityEngine;

public class BeamController : MonoBehaviour
{
    [SerializeField] private LineRenderer line;
    [SerializeField] private BoxCollider2D hitCollider;
    [SerializeField] private float maxLength = 20f;
    [SerializeField] private float width = 0.2f;
    [SerializeField] private LayerMask hitMask;

    private float endTime;
    private bool active;

    private void Awake()
    {
        if (line == null) line = GetComponent<LineRenderer>();
        if (hitCollider == null) hitCollider = GetComponent<BoxCollider2D>();
        if (line != null)
        {
            line.positionCount = 2;
            line.useWorldSpace = false;
            line.startWidth = width;
            line.endWidth = width;
        }
        if (hitCollider != null)
        {
            hitCollider.isTrigger = true;
        }
    }

    public void SetMaxLength(float len)
    {
        maxLength = Mathf.Max(0.1f, len);
    }

    public void Activate(float duration)
    {
        endTime = Time.time + duration;
        active = true;
    }

    private void Update()
    {
        if (!active)
        {
            return;
        }
        if (Time.time >= endTime)
        {
            active = false;
            Destroy(gameObject);
            return;
        }

        Vector2 dir = transform.right;
        float length = maxLength;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, maxLength, hitMask);
        if (hit.collider != null)
        {
            length = hit.distance;
        }

        if (line != null)
        {
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, Vector3.right * length);
            line.startWidth = width;
            line.endWidth = width;
        }
        if (hitCollider != null)
        {
            hitCollider.size = new Vector2(length, width);
            hitCollider.offset = new Vector2(length * 0.5f, 0f);
        }
    }
}
