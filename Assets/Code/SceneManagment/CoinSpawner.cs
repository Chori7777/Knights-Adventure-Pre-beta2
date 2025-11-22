using UnityEngine;
using UnityEngine.Tilemaps;

public class CoinSpawner : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Tilemap coinMap;         // El Tilemap donde se "pintan" las monedas
    [SerializeField] private GameObject monedaPrefab; // El prefab de la moneda

    private void Start()
    {
        if (coinMap == null || monedaPrefab == null)
        {
            Debug.LogError("Faltan referencias");
            return;
        }


        foreach (Vector3Int pos in coinMap.cellBounds.allPositionsWithin)
        {
            TileBase tile = coinMap.GetTile(pos);
            if (tile != null)
            {
                Vector3 worldPos = coinMap.GetCellCenterWorld(pos);
                Instantiate(monedaPrefab, worldPos, Quaternion.identity);
            }
        }

        // Desactiva el Tilemap para que no se vea
        coinMap.gameObject.SetActive(false);

        Debug.Log("Monedas generadas");
    }
}
