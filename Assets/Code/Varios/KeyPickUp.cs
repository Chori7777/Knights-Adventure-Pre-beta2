using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using UnityEngine.Events;

public class KeyPickup : MonoBehaviour
{
    [Header("Objetos que se destruyen al recoger la llave")]
    [SerializeField] private Tilemap[] tilemapsToDestroy;
    [SerializeField] private GameObject[] objectsToDestroy;

    [Header("Efectos opcionales")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float pickupVolume = 0.7f;
    [SerializeField] private GameObject pickupEffect;
    [Header("Trigger")]
    [SerializeField] private UnityEvent onPickedUp;
    [Header("Shake del objeto")]
    [SerializeField] private float objectShakeDuration = 0.15f;
    [SerializeField] private float objectShakeIntensity = 0.1f;

    private AudioSource audioSource;

    private void Start()
    {
        // Crea un AudioSource si hay sonido configurado
        if (pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        StartCoroutine(PickupSequence(collision));
    }

    private IEnumerator PickupSequence(Collider2D collision)
    {
        if (pickupSound != null)
        {
            float randomPitch = Random.Range(0.9f, 1.1f);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(pickupSound, pickupVolume, randomPitch);
            }
            else if (audioSource != null)
            {
                audioSource.pitch = randomPitch;
                audioSource.PlayOneShot(pickupSound, pickupVolume);
            }
        }

        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        yield return StartCoroutine(ShakeObject());

        var pm = collision.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.TriggerCameraShake();
        }

        foreach (Tilemap tm in tilemapsToDestroy)
        {
            if (tm != null)
                Destroy(tm.gameObject);
        }
        foreach (GameObject obj in objectsToDestroy)
        {
            if (obj != null)
                Destroy(obj);
        }
        onPickedUp?.Invoke();
        Destroy(gameObject);
    }

    private IEnumerator ShakeObject()
    {
        Vector3 original = transform.localPosition;
        float t = 0f;
        while (t < objectShakeDuration)
        {
            float x = Random.Range(-1f, 1f) * objectShakeIntensity;
            float y = Random.Range(-1f, 1f) * objectShakeIntensity;
            transform.localPosition = original + new Vector3(x, y, 0f);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = original;
    }
}
