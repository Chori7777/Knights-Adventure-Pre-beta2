using UnityEngine;
using UnityEngine.SceneManagement;
public class NewGameStarter : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D colision)
    {
        SceneManager.LoadScene("CorruptedGameScene");
    }
}
