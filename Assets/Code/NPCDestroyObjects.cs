using UnityEngine;

public class NPCDestroyObjects : MonoBehaviour
{
    [Header("Objetos a destruir después de dar la recompensa")]
    [SerializeField] private GameObject[] objectsToDestroy;

    // Este método lo llamará el NPCRewardSystem
    public void DestroyAssignedObjects()
    {
        foreach (var obj in objectsToDestroy)
        {
            if (obj != null)
                Destroy(obj);
        }
    }
}
