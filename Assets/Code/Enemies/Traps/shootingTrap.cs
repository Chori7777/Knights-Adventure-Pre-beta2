using UnityEngine;

public class shootingTrap : MonoBehaviour
{
    public enum Direction { Right, Left, Up, Down }
    public Direction shootDirection = Direction.Right;

    [Header("Proyectil")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float cooldown = 1.5f;
    public float projectileSpeed = 10f;

    private float lastShotTime = -999f;
    private Vector2 shootVector;

    void Start()
    {
        SetDirection();
    }

    void Update()
    {
        if (Time.time > lastShotTime + cooldown)
        {
            Shoot();
            lastShotTime = Time.time;
        }
    }

    void SetDirection()
    {
        switch (shootDirection)
        {
            case Direction.Right:
                shootVector = Vector2.right;
                firePoint.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case Direction.Left:
                shootVector = Vector2.left;
                firePoint.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case Direction.Up:
                shootVector = Vector2.up;
                firePoint.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case Direction.Down:
                shootVector = Vector2.down;
                firePoint.rotation = Quaternion.Euler(0, 0, -90);
                break;
        }
    }

    void Shoot()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject p = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = p.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = shootVector * projectileSpeed;
        }

        BulletScript proj = p.GetComponent<BulletScript>();
        if (proj != null)
        {
            proj.speed = projectileSpeed;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Vector2 direction = Vector2.right;

            switch (shootDirection)
            {
                case Direction.Right: direction = Vector2.right; break;
                case Direction.Left: direction = Vector2.left; break;
                case Direction.Up: direction = Vector2.up; break;
                case Direction.Down: direction = Vector2.down; break;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint.position, firePoint.position + (Vector3)direction * 2f);
            Gizmos.DrawWireSphere(firePoint.position, 0.1f);
        }
    }
}
