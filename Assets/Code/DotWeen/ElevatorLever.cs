using UnityEngine;
using UnityEngine.UI;

public class ElevatorLever : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private ElevatorSystem elevator;
    [SerializeField] private bool callToPointA = false;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionRadius = 1.5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Visuales")]
    [SerializeField] private Sprite leverOffSprite;
    [SerializeField] private Sprite leverOnSprite;
    [SerializeField] private GameObject floatingTextPrefab;

    [Header("Opciones Extra")]
    [SerializeField] private bool canReactivate = false;
    [SerializeField] private AudioClip activateSound;

    private bool isActivated = false;
    private bool playerInRange = false;
    private GameObject floatingTextInstance;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && leverOffSprite != null)
            spriteRenderer.sprite = leverOffSprite;

        if (floatingTextPrefab != null)
        {
            floatingTextInstance = Instantiate(floatingTextPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
            floatingTextInstance.transform.SetParent(transform);
            floatingTextInstance.SetActive(false);
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && activateSound != null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        DetectPlayer();
        if (playerInRange && Input.GetKeyDown(interactKey))
            TryActivateLever();
    }

    private void DetectPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactionRadius, playerLayer);
        playerInRange = (hit != null);

        if (floatingTextInstance != null)
        {
            bool shouldShow = playerInRange && (!isActivated || canReactivate);
            floatingTextInstance.SetActive(shouldShow);
        }
    }

    private void TryActivateLever()
    {
        if (isActivated && !canReactivate)
            return;

        if (elevator == null || elevator.IsMoving)
            return;

        elevator.CallElevator(callToPointA);
        isActivated = true;
        UpdateLeverVisual();

        if (audioSource != null && activateSound != null)
            audioSource.PlayOneShot(activateSound);

        if (!canReactivate && floatingTextInstance != null)
            floatingTextInstance.SetActive(false);
    }

    private void UpdateLeverVisual()
    {
        if (spriteRenderer == null) return;

        if (isActivated && leverOnSprite != null)
            spriteRenderer.sprite = leverOnSprite;
        else if (!isActivated && leverOffSprite != null)
            spriteRenderer.sprite = leverOffSprite;
    }

    public void ResetLever()
    {
        isActivated = false;
        UpdateLeverVisual();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);

        if (elevator != null)
        {
            Gizmos.color = callToPointA ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, elevator.transform.position);
        }
    }

    public bool IsActivated => isActivated;
    public bool PlayerInRange => playerInRange;
}
