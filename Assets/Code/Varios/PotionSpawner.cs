using UnityEngine;
using UnityEngine.Tilemaps;

public class PotionSpawner : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Tilemap potionMap;  // El Tilemap donde se "pintan" las pocione

    [SerializeField] private GameObject potionPrefab; // El prefab de la pocion

    private void Start()
    {
        if (potionMap == null || potionPrefab == null)
        {
            Debug.LogError("Faltan referencias");
            return;
        }


        foreach (Vector3Int pos in potionMap.cellBounds.allPositionsWithin)
        {
            TileBase tile = potionMap.GetTile(pos);
            if (tile != null)
            {
                Vector3 worldPos = potionMap.GetCellCenterWorld(pos);
                Instantiate(potionPrefab, worldPos, Quaternion.identity);
            }
        }

        // Desactiva el Tilemap para que no se vea
        potionMap.gameObject.SetActive(false);

        Debug.Log("Monedas generadas");
    }
}
