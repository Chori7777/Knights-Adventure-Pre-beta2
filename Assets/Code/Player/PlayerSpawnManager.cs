using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawnManager : MonoBehaviour
{
    public static PlayerSpawnManager Instance { get; private set; }

    [System.Serializable]
    public class SceneSpawn
    {
        public string sceneName;
        public Vector3 spawnPosition;
    }

    [Header("Configuración de Spawns")]
    [SerializeField] private SceneSpawn[] sceneSpawns;

    [Header("Spawn por Defecto")]
    [SerializeField] private Vector3 defaultSpawn = new Vector3(0, 0, 0);

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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ✅ CAMBIO CRÍTICO: Solo reposicionar si NO estamos cargando desde checkpoint
        if (ControladorDatosJuego.Instance != null)
        {
            bool isFromCheckpoint = ControladorDatosJuego.Instance.IsLoadingFromCheckpoint;
            bool isFromContinue = ControladorDatosJuego.Instance.IsLoadingFromContinue;

            if (isFromCheckpoint || isFromContinue)
            {
                Debug.Log("🔹 [PlayerSpawnManager] Cargando desde checkpoint/continue - NO reposicionar");
                return; // ✅ Dejar que ControladorDatosJuego maneje el spawn
            }
        }

        // Solo llegar aquí si es spawn NORMAL (nueva partida, transición de escena)
        Debug.Log("🔹 [PlayerSpawnManager] Spawn normal - reposicionando");
        Vector3 pos = RepositionPlayer(scene.name);

        // Guardar checkpoint de inicio de nivel para 'Restart Level'
        if (ControladorDatosJuego.Instance != null)
        {
            ControladorDatosJuego.Instance.GuardarCheckpoint(pos);
            Debug.Log($"💾 [PlayerSpawnManager] Checkpoint inicial guardado en {scene.name}: {pos}");
        }
    }

    private Vector3 RepositionPlayer(string sceneName)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("⚠️ [PlayerSpawnManager] No se encontró jugador");
            return GetSpawnPosition(sceneName);
        }

        Vector3 spawnPos = GetSpawnPosition(sceneName);
        player.transform.position = spawnPos;
        Debug.Log($"🎯 [PlayerSpawnManager] Jugador reposicionado en {sceneName}: {spawnPos}");
        return spawnPos;
    }

    private Vector3 GetSpawnPosition(string sceneName)
    {
        // 1. Buscar en el array de spawns
        foreach (var spawn in sceneSpawns)
        {
            if (spawn.sceneName == sceneName)
                return spawn.spawnPosition;
        }

        // 2. Buscar GameObject con tag "PlayerSpawn"
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawn");
        if (spawnPoint != null)
            return spawnPoint.transform.position;

        // 3. Usar spawn por defecto
        Debug.LogWarning($"⚠️ [PlayerSpawnManager] No hay spawn definido para {sceneName}, usando default");
        return defaultSpawn;
    }
}
