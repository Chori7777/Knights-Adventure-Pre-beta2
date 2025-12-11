using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class GlobalPostFX : MonoBehaviour
{
    public static GlobalPostFX Instance;
    public Volume volume;
    [System.Serializable]
    public struct SceneVolumeMapping
    {
        public string sceneName;
        public VolumeProfile profile;
    }
    [SerializeField] private SceneVolumeMapping[] sceneProfiles;
    [SerializeField] private VolumeProfile defaultProfile;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (volume == null) volume = GetComponent<Volume>();
        if (defaultProfile == null && volume != null) defaultProfile = volume.profile;
        SceneManager.sceneLoaded += OnSceneLoaded;
        var active = SceneManager.GetActiveScene();
        if (active.IsValid()) ApplyProfileForScene(active.name);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyProfileForScene(scene.name);
    }

    public void ApplyProfileForScene(string sceneName)
    {
        if (volume == null) return;
        VolumeProfile prof = defaultProfile;
        if (sceneProfiles != null)
        {
            for (int i = 0; i < sceneProfiles.Length; i++)
            {
                if (!string.IsNullOrEmpty(sceneProfiles[i].sceneName) && sceneProfiles[i].sceneName == sceneName && sceneProfiles[i].profile != null)
                {
                    prof = sceneProfiles[i].profile;
                    break;
                }
            }
        }
        volume.profile = prof;
        var cam = Camera.main;
        if (cam != null)
        {
            var data = cam.GetComponent<UniversalAdditionalCameraData>();
            if (data != null)
            {
                data.renderPostProcessing = true;
                data.volumeLayerMask |= (1 << volume.gameObject.layer);
            }
        }
    }
}
