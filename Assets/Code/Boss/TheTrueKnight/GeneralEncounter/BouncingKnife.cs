using UnityEngine;

public class BouncingKnife : MonoBehaviour
{
    private float speed;
    private int maxBounces;
    private float lifetime;
    private Vector3 center;
    private float halfW;
    private float halfH;
    private Vector3 dir;
    private float startTime;

    public void Init(float speed, int maxBounces, float lifetime, Vector3 camCenter, float halfW, float halfH)
    {
        this.speed = speed;
        this.maxBounces = maxBounces;
        this.lifetime = lifetime;
        this.center = camCenter;
        this.halfW = halfW;
        this.halfH = halfH;
        this.dir = transform.right != Vector3.zero ? transform.right.normalized : Random.insideUnitCircle.normalized;
        startTime = Time.time;
    }

    private void Update()
    {
        transform.position += dir * speed * Time.deltaTime;
        Vector3 p = transform.position;
        bool bounced = false;
        if (p.x < center.x - halfW)
        {
            p.x = center.x - halfW;
            dir.x = -dir.x;
            bounced = true;
        }
        else if (p.x > center.x + halfW)
        {
            p.x = center.x + halfW;
            dir.x = -dir.x;
            bounced = true;
        }
        if (p.y < center.y - halfH)
        {
            p.y = center.y - halfH;
            dir.y = -dir.y;
            bounced = true;
        }
        else if (p.y > center.y + halfH)
        {
            p.y = center.y + halfH;
            dir.y = -dir.y;
            bounced = true;
        }
        if (bounced)
        {
            transform.position = p;
            maxBounces--;
            if (maxBounces <= 0) Destroy(gameObject);
        }
        if (lifetime > 0f && Time.time > startTime + lifetime) Destroy(gameObject);
    }
}
