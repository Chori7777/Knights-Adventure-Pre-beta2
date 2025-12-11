using UnityEngine;
using UnityEngine.Rendering;

public class GlobalPostFX : MonoBehaviour
{
    public static GlobalPostFX Instance;
    public Volume volume;

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
    }
}
