using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Los datos del juego
    public DatosJuego datos = new DatosJuego();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}