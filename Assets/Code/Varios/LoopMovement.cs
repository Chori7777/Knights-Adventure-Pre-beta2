using UnityEngine;

public class LoopMovement : MonoBehaviour
{
    public float speed = 3f;
    public float leftLimit = -12f;
    public float rightLimit = 12f;

    void Update()
    {
        transform.position += Vector3.right * speed * Time.deltaTime;

        if (transform.position.x > rightLimit)
        {
            Vector3 p = transform.position;
            p.x = leftLimit;
            transform.position = p;
        }
    }
}
