using UnityEngine;

[DisallowMultipleComponent]
public class BossSpawnedAutoCleanup : MonoBehaviour
{
    [SerializeField] private float lifetime = 0f;
    private void OnEnable()
    {
        if (lifetime > 0f)
        {
            Destroy(gameObject, lifetime);
        }
    }
}
