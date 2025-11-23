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
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ✅ Solo reposicionar si NO estamos cargando desde un checkpoint
        if (ControladorDatosJuego.Instance != null &&
            !ControladorDatosJuego.Instance.IsLoadingFromCheckpoint)
        {
            RepositionPlayer(scene.name);
        }
        else
        {
            Debug.Log("📂 Cargando desde checkpoint, manteniendo posición guardada");
        }
    }
    private void RepositionPlayer(string sceneName)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Buscar spawn específico para esta escena
        Vector3 spawnPos = GetSpawnPosition(sceneName);

        player.transform.position = spawnPos;
        Debug.Log($"🎯 Jugador reposicionado en {sceneName}: {spawnPos}");
    }

    private Vector3 GetSpawnPosition(string sceneName)
    {
        // Buscar en el array de spawns
        foreach (var spawn in sceneSpawns)
        {
            if (spawn.sceneName == sceneName)
            {
                return spawn.spawnPosition;
            }
        }

        // Buscar GameObject con tag "PlayerSpawn" en la escena actual
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawn");
        if (spawnPoint != null)
        {
            return spawnPoint.transform.position;
        }

        // Usar spawn por defecto
        Debug.LogWarning($"⚠️ No hay spawn definido para {sceneName}, usando default");
        return defaultSpawn;
    }
}