using UnityEngine;

public class NPCDestroyObjects : MonoBehaviour
{
    [Header("Objetos a destruir despu�s de dar la recompensa")]
    [SerializeField] private GameObject[] objectsToDestroy;
    [SerializeField] private string[] objectIDs;

    // Este m�todo lo llamar� el NPCRewardSystem
    public void DestroyAssignedObjects()
    {
        for (int i = 0; i < objectsToDestroy.Length; i++)
        {
            var obj = objectsToDestroy[i];
            if (obj != null)
            {
                obj.SetActive(false);
            }

            if (ControladorDatosJuego.Instance != null && objectIDs != null && i < objectIDs.Length)
            {
                var id = objectIDs[i];
                if (!string.IsNullOrEmpty(id))
                {
                    ControladorDatosJuego.Instance.MarcarObjetoDestruido(id);
                }
            }
        }
    }

    private void Start()
    {
        if (ControladorDatosJuego.Instance == null || objectIDs == null) return;
        for (int i = 0; i < objectIDs.Length; i++)
        {
            var id = objectIDs[i];
            if (!string.IsNullOrEmpty(id) && ControladorDatosJuego.Instance.EstaObjetoDestruido(id))
            {
                if (i < objectsToDestroy.Length && objectsToDestroy[i] != null)
                {
                    objectsToDestroy[i].SetActive(false);
                }
            }
        }
    }
}
