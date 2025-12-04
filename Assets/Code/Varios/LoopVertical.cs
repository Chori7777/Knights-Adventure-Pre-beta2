using UnityEngine;

public class LoopMovementVertical : MonoBehaviour
{
    public float speed = 2f;
    public float bottomLimit = -6f;
    public float topLimit = 6f;

    void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;

        if (transform.position.y > topLimit)
        {
            Vector3 p = transform.position;
            p.y = bottomLimit;
            transform.position = p;
        }
    }
}
