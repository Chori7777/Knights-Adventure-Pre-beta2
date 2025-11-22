using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class NextLevel : MonoBehaviour
{
    public KeyCode interactionKey = KeyCode.E;
    public Animator doorAnimator;
    public string triggerName = "Open";

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            StartCoroutine(LevelTransition());
        }
    }

    IEnumerator LevelTransition()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.SetActive(false);
        }

        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(triggerName);
            yield return new WaitForSeconds(2f);
        }

        SceneManager.LoadScene("Victory");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
