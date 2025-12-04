using UnityEngine;

public class TrialRoomEvents : MonoBehaviour
{
    [Header("Objetos a habilitar")]
    [SerializeField] private GameObject[] objetos;
    [Header("Sistemas a habilitar")]
    [SerializeField] private Behaviour[] sistemas;

    public void ActivarSala()
    {
        SetActivos(true);
    }

    public void DesactivarSala()
    {
        SetActivos(false);
    }

    private void SetActivos(bool activo)
    {
        if (objetos != null)
        {
            for (int i = 0; i < objetos.Length; i++)
                if (objetos[i] != null) objetos[i].SetActive(activo);
        }
        if (sistemas != null)
        {
            for (int i = 0; i < sistemas.Length; i++)
                if (sistemas[i] != null) sistemas[i].enabled = activo;
        }
    }
}
