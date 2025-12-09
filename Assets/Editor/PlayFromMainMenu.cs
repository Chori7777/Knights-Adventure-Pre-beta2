using UnityEditor;
using UnityEditor.SceneManagement;

public static class PlayFromMainMenu
{
    private const string MainMenuPath = "Assets/Scenes/AllMenus/MainMenu.unity";
    private const string FallbackStartMenuPath = "Assets/Scenes/AllMenus/StartMenu.unity";

    [InitializeOnLoadMethod]
    private static void Init()
    {
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuPath);
        if (sceneAsset != null)
        {
            EditorSceneManager.playModeStartScene = sceneAsset;
            return;
        }

        var fallback = AssetDatabase.LoadAssetAtPath<SceneAsset>(FallbackStartMenuPath);
        if (fallback != null)
        {
            EditorSceneManager.playModeStartScene = fallback;
        }
    }

    [MenuItem("Tools/Set Play From Main Menu")]
    private static void SetMainMenu()
    {
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuPath);
        if (sceneAsset != null)
        {
            EditorSceneManager.playModeStartScene = sceneAsset;
        }
    }

    [MenuItem("Tools/Clear Play Start Scene")]
    private static void ClearStartScene()
    {
        EditorSceneManager.playModeStartScene = null;
    }
}
