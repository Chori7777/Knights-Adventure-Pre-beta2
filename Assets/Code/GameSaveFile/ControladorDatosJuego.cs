using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorDatosJuego : MonoBehaviour
{
    public static ControladorDatosJuego Instance;
    public DatosJuego datosjuego = new DatosJuego();
    private string rutaArchivo;
    private string checkpointEscena;
    private Vector3 checkpointPos;
    private Vector3 checkpointCamara;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        rutaArchivo = Application.persistentDataPath + "/save.json";
    }


    public void GuardarDatos(bool guardarPosicion = true)
    {
        if (!FindPlayer())
        {
            Debug.LogWarning("No se encontró el jugador para guardar");
            return;
        }

        if (guardarPosicion)
            CapturarDatosJugador();
        else
            CapturarDatosBasicos();

        EscribirArchivo();
        Debug.Log(" Datos guardados correctamente");
        if (SaveNotification.Instance != null)
        {
            SaveNotification.Instance.ShowSaveSuccess();
        }

    }

    private void CapturarDatosJugador()
    {
        GameObject player = FindPlayer();
        if (player != null)
        {
            datosjuego.posicion = player.transform.position;
        }

        CapturarDatosBasicos();
    }

    private void CapturarDatosBasicos()
    {
        datosjuego.escenaActual = SceneManager.GetActiveScene().name;

        Camera cam = Camera.main;
        if (cam != null)
            datosjuego.posicionCamara = cam.transform.position;
    }

    public void GuardarCheckpoint(Vector3 playerPosition)
    {
        checkpointPos = playerPosition;
        checkpointEscena = SceneManager.GetActiveScene().name;

        Debug.Log(" Escena actual guardado"); 

        Camera cam = Camera.main;
        if (cam != null)
            checkpointCamara = cam.transform.position;

        datosjuego.posicion = checkpointPos;
        datosjuego.posicionCamara = checkpointCamara;
        datosjuego.escenaActual = checkpointEscena;

        GuardarDatos(true);
        Debug.Log(" Checkpoint guardado en");
    }



    public void CargarDatos()
    {
        if (File.Exists(rutaArchivo))
        {
            string json = File.ReadAllText(rutaArchivo);
            datosjuego = JsonUtility.FromJson<DatosJuego>(json);
            Debug.Log(" Datos cargados correctamente");
        }
        else
        {
            Debug.LogWarning(" No hay archivo de guardado existente");
        }
        if (SaveNotification.Instance != null)
        {
            SaveNotification.Instance.ShowLoadSuccess();
        }
    }
    public void ContinuarPartida()
    {
        CargarDatos();

        if (!string.IsNullOrEmpty(datosjuego.escenaActual))
        {
            Debug.Log($" Cargando escena: '{datosjuego.escenaActual}'");
            Debug.Log($" Posición guardada: {datosjuego.posicion}");
            Debug.Log($" Cámara guardada: {datosjuego.posicionCamara}");

            SceneManager.sceneLoaded += OnContinueSceneLoaded;


            SceneManager.LoadScene(datosjuego.escenaActual);
        }
        else
        {
            Debug.LogWarning(" No hay escena guardada para continuar");
        }
    }


    private void OnContinueSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        SceneManager.sceneLoaded -= OnContinueSceneLoaded;

        Debug.Log(" Escena cargada correctamente");


        StartCoroutine(RecolocarJugadorDespuesDeContinuar());
    }


    private IEnumerator RecolocarJugadorDespuesDeContinuar()
    {

        yield return new WaitForSeconds(0.3f);

 
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = datosjuego.posicion;
            Debug.Log("Jugador recolocado");
        }
        else
        {
            Debug.LogError(" No se encontró jugador con tag Player");
        }

        // Recolocamos la cámara
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = datosjuego.posicionCamara;
        }


        if (player != null)
        {
            // Restaurar vida
            playerLife vida = player.GetComponent<playerLife>();
            if (vida != null)
            {
                Debug.Log("Vida restaurada");
            }
        }

        if (PlayerHealthUI.Instance != null)
        {
            PlayerHealthUI.Instance.ActualizarMonedas(datosjuego.cantidadMonedas);
            Debug.Log(" Monedas restauradas en UI");
        }
    }
    private void OnSceneLoadedContinue(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoadedContinue;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = datosjuego.posicion;
            Debug.Log("Jugador colocado en checkpoint");
        }

        if (Camera.main != null)
            Camera.main.transform.position = datosjuego.posicionCamara;
    }
 

    private GameObject FindPlayer()
    {
        return GameObject.FindGameObjectWithTag("Player");
    }

    private void EscribirArchivo()
    {
        string json = JsonUtility.ToJson(datosjuego, true);
        File.WriteAllText(rutaArchivo, json);
    }

    public void EliminarGuardado()
    {
        if (File.Exists(rutaArchivo))
        {
            File.Delete(rutaArchivo);
            Debug.Log("Guardado eliminado correctamente");
        }
    }

    public int ObtenerMonedas()
    {
        return datosjuego.cantidadMonedas;
    }

    public void AgregarMonedas(int cantidad)
    {
        datosjuego.cantidadMonedas += cantidad;
        GuardarDatos(false);

        if (PlayerHealthUI.Instance != null)
            PlayerHealthUI.Instance.ActualizarMonedas(datosjuego.cantidadMonedas);
    }



    public void RespawnearJugadorEnCheckpoint()
    {
        if (datosjuego == null)
        {
            Debug.LogWarning("No hay datos guardados. Recargando escena actual...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        if (datosjuego.posicion == Vector3.zero)
        {
            Debug.Log("No hay posición de checkpoint guardada. Recargando escena...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        if (!string.IsNullOrEmpty(datosjuego.escenaActual))
        {
            SceneManager.LoadScene(datosjuego.escenaActual);
            Instance.StartCoroutine(RecolocarJugador());
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private IEnumerator RecolocarJugador()
    {
        yield return new WaitForSeconds(0.2f);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = datosjuego.posicion;
            Debug.Log("[ControladorDatosJuego] Jugador recolocado en checkpoint.");
        }

        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = datosjuego.posicionCamara;
            Debug.Log("[ControladorDatosJuego] Cámara recolocada en checkpoint.");
        }
    }

    public void ResetearDatos()
    {
        datosjuego = new DatosJuego(); 


        checkpointEscena = "";
        checkpointPos = Vector3.zero;
        checkpointCamara = Vector3.zero;


        string archivo = Application.persistentDataPath + "/save.json";
        if (File.Exists(archivo))
        {
            File.Delete(archivo);
            Debug.Log(" Guardado anterior eliminado al iniciar nueva partida.");
        }

        Debug.Log(" Datos del juego reseteados correctamente.");
    }

}
