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

    // 🔹 FLAGS DE CONTROL CRÍTICOS
    public bool IsLoadingFromCheckpoint { get; private set; }
    public bool IsLoadingFromContinue { get; private set; }

    // 🔹 Para evitar reposicionamiento múltiple
    private bool hasRepositionedThisScene = false;
    private bool hasSavedInitialCheckpointThisScene = false;

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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 🔹 EVENTO DE ESCENA CARGADA
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[Save] Escena cargada: {scene.name}");
        Debug.Log($"[Save] IsLoadingFromCheckpoint: {IsLoadingFromCheckpoint}");
        Debug.Log($"[Save] IsLoadingFromContinue: {IsLoadingFromContinue}");

        // Reset flag
        hasRepositionedThisScene = false;
        hasSavedInitialCheckpointThisScene = false;

        // Si estamos cargando desde checkpoint O continue, reposicionar
        if (IsLoadingFromCheckpoint || IsLoadingFromContinue)
        {
            StartCoroutine(RepositionPlayerAfterLoad());
        }
        else
        {
            StartCoroutine(SaveInitialCheckpointAfterLoad(scene.name));
        }

        StartCoroutine(ApplyAbilityLocksAfterLoad());
    }

    // 🔹 REPOSICIONAMIENTO UNIFICADO
    private IEnumerator RepositionPlayerAfterLoad()
    {
        if (hasRepositionedThisScene)
        {
            Debug.LogWarning("[Save] Ya se reposicionó en esta escena, ignorando");
            yield break;
        }

        hasRepositionedThisScene = true;

        // Esperar a que el jugador exista
        yield return new WaitForSeconds(0.1f);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[Save] No se encontró jugador");
            ResetFlags();
            yield break;
        }

        // ✅ REPOSICIONAR JUGADOR
        player.transform.position = datosjuego.posicion;
        Debug.Log($"[Save] Jugador reposicionado en: {datosjuego.posicion}");

        // ✅ REPOSICIONAR CÁMARA
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = datosjuego.posicionCamara;
            Debug.Log($"[Save] Cámara reposicionada en: {datosjuego.posicionCamara}");
        }

        // ✅ RESTAURAR VIDA
        playerLife vida = player.GetComponent<playerLife>();
        if (vida != null)
        {
            vida.SetMaxHealth(datosjuego.vidaMaxima);
            vida.SetHealth(datosjuego.vidaActual);
            vida.SetPotions(datosjuego.cantidadpociones);
            vida.SetMaxPotions(datosjuego.maxPotions);
            Debug.Log($"[Save] Vida restaurada: {datosjuego.vidaActual}/{datosjuego.vidaMaxima}");
        }

        // ✅ ACTUALIZAR HUD
        yield return new WaitForSeconds(0.1f); // Esperar a que HUD esté listo

        if (PlayerHealthUI.Instance != null)
        {
            PlayerHealthUI.Instance.ActualizarMonedas(datosjuego.cantidadMonedas);
            PlayerHealthUI.Instance.ActualizarHachas(datosjuego.cantidadHachas);
            Debug.Log("[Save] HUD actualizado");
        }

        // ✅ RESETEAR FLAGS
        ResetFlags();
    }
    private IEnumerator SaveInitialCheckpointAfterLoad(string sceneName)
    {
        if (hasSavedInitialCheckpointThisScene) yield break;
        yield return new WaitForSeconds(0.1f);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;
        Vector3 spawnPos = player.transform.position;
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawn");
        if (spawnPoint != null)
        {
            spawnPos = spawnPoint.transform.position;
        }
        GuardarCheckpoint(spawnPos);
        hasSavedInitialCheckpointThisScene = true;
    }

    private void ResetFlags()
    {
        IsLoadingFromCheckpoint = false;
        IsLoadingFromContinue = false;
        Debug.Log("[Save] Flags reseteados");
    }
    private IEnumerator ApplyAbilityLocksAfterLoad()
    {
        yield return new WaitForSeconds(0.1f);
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;
        var pm = player.GetComponent<PlayerMovement>();
        if (pm == null) yield break;
        pm.canBlock = datosjuego.hasShield;
        pm.canWallCling = datosjuego.hasWallCling;
        pm.canDoubleJump = datosjuego.hasDoubleJump;
        pm.canDash = datosjuego.hasDash;
        pm.canThrowProjectile = datosjuego.hasRangedAttack;
    }

    // ═══════════════════════════════════════════════════
    //  GUARDAR/CARGAR
    // ═══════════════════════════════════════════════════

    public void GuardarDatos(bool guardarPosicion = true)
    {
        GameObject player = FindPlayer();
        if (player == null)
        {
            Debug.LogWarning("[Save] No se encontró jugador para guardar");
            return;
        }

        if (guardarPosicion)
        {
            datosjuego.posicion = player.transform.position;
            Camera cam = Camera.main;
            if (cam != null)
                datosjuego.posicionCamara = cam.transform.position;
        }

        datosjuego.escenaActual = SceneManager.GetActiveScene().name;
        EscribirArchivo();

        Debug.Log("[Save] Datos guardados");

        if (SaveNotification.Instance != null)
            SaveNotification.Instance.ShowSaveSuccess();
    }

    public void GuardarCheckpoint(Vector3 playerPosition)
    {
        datosjuego.posicion = playerPosition;
        datosjuego.escenaActual = SceneManager.GetActiveScene().name;

        Camera cam = Camera.main;
        if (cam != null)
            datosjuego.posicionCamara = cam.transform.position;

        GuardarDatos(false); // Ya capturamos la posición manualmente
        Debug.Log($"[Save] Checkpoint guardado: {playerPosition}");
    }

    public void CargarDatos()
    {
        if (File.Exists(rutaArchivo))
        {
            string json = File.ReadAllText(rutaArchivo);
            datosjuego = JsonUtility.FromJson<DatosJuego>(json);
            Debug.Log("[Save] Datos cargados");
        }
        else
        {
            Debug.LogWarning("[Save] No hay archivo de guardado");
        }

        if (SaveNotification.Instance != null)
            SaveNotification.Instance.ShowLoadSuccess();
    }

    // 🔹 CONTINUAR PARTIDA (desde menú)
    public void ContinuarPartida()
    {
        CargarDatos();

        if (string.IsNullOrEmpty(datosjuego.escenaActual))
        {
            Debug.LogWarning("[Save] No hay escena guardada");
            return;
        }

        Debug.Log($"[Save] Continuando partida en: {datosjuego.escenaActual}");
        Debug.Log($"[Save] Posición guardada: {datosjuego.posicion}");

        // ✅ ACTIVAR FLAG
        IsLoadingFromContinue = true;

        SceneManager.LoadScene(datosjuego.escenaActual);
    }

    // 🔹 RESPAWN EN CHECKPOINT (muerte)
    public void RespawnearJugadorEnCheckpoint()
    {
        if (datosjuego == null || datosjuego.posicion == Vector3.zero)
        {
            Debug.LogWarning("[Save] No hay checkpoint, recargando escena");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        if (string.IsNullOrEmpty(datosjuego.escenaActual))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        Debug.Log($"[Save] Respawneando en checkpoint: {datosjuego.escenaActual}");

        // ✅ ACTIVAR FLAG
        IsLoadingFromCheckpoint = true;

        SceneManager.LoadScene(datosjuego.escenaActual);
    }

    // ═══════════════════════════════════════════════════
    //  MONEDAS
    // ═══════════════════════════════════════════════════

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

    // ═══════════════════════════════════════════════════
    //  UTILIDADES
    // ═══════════════════════════════════════════════════

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
            Debug.Log("[Save] Guardado eliminado");
        }
    }

    public void ResetearDatos()
    {
        datosjuego = new DatosJuego();
        EliminarGuardado();
        Debug.Log("[Save] Datos reseteados");
    }

    public bool EstaNPCRecompensaEntregada(string npcID)
    {
        if (string.IsNullOrEmpty(npcID)) return false;
        return datosjuego.npcsRecompensaEntregada.Contains(npcID);
    }

    public void MarcarNPCRecompensaEntregada(string npcID)
    {
        if (string.IsNullOrEmpty(npcID)) return;
        if (!datosjuego.npcsRecompensaEntregada.Contains(npcID))
        {
            datosjuego.npcsRecompensaEntregada.Add(npcID);
            GuardarDatos(false);
        }
    }

    public bool EstaObjetoDestruido(string objetoID)
    {
        if (string.IsNullOrEmpty(objetoID)) return false;
        return datosjuego.objetosDestruidos.Contains(objetoID);
    }

    public void MarcarObjetoDestruido(string objetoID)
    {
        if (string.IsNullOrEmpty(objetoID)) return;
        if (!datosjuego.objetosDestruidos.Contains(objetoID))
        {
            datosjuego.objetosDestruidos.Add(objetoID);
            GuardarDatos(false);
        }
    }

    // VARIANTE DE INICIO (Original vs NewGamePlus)
    public void SetStartModeVariant(int variant)
    {
        datosjuego.startModeVariant = Mathf.Clamp(variant, 0, 1);
        GuardarDatos(false);
        ChangeScene.MainMenuVariation = datosjuego.startModeVariant;
        Debug.Log("[Save] startModeVariant actualizado a " + datosjuego.startModeVariant);
    }
}
