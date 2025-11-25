using UnityEngine;
using TMPro;
using System.Collections;

public class FirstEncounterPhaseTextSpawner : MonoBehaviour
{
    [SerializeField] private string[] phrases;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private TextMeshProUGUI textPrefab;
    [SerializeField] private float interval = 2f;
    [SerializeField] private int maxTexts = 10;

    private bool running;

    public void StartSpawning()
    {
        if (textPrefab == null || spawnPoints == null || spawnPoints.Length == 0) return;
        if (phrases == null || phrases.Length == 0) return;
        running = true;
        StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        running = false;
    }

    private IEnumerator SpawnRoutine()
    {
        int count = 0;
        while (running && count < maxTexts)
        {
            Transform p = spawnPoints[Random.Range(0, spawnPoints.Length)];
            string t = phrases[Random.Range(0, phrases.Length)];
            var ui = Instantiate(textPrefab, p.position, Quaternion.identity, p);
            ui.text = t;
            count++;
            yield return new WaitForSeconds(interval);
        }
    }
}
