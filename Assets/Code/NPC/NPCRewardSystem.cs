using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sistema de recompensas para NPCs con diferentes tipos de premios
/// </summary>
public class NPCRewardSystem : MonoBehaviour
{
    [Header("Configuración del NPC")]
    [SerializeField] private string npcID = "NPC_001";
    [SerializeField] private bool canGiveRewardMultipleTimes = false;

    [Header("Interacción")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionRange = 2f;

    [Header("Diálogos")]
    [TextArea(3, 6)]
    [SerializeField] private string dialogueBeforeReward = "¡Hola aventurero! Tengo algo para ti.";
    [TextArea(3, 6)]
    [SerializeField] private string dialogueAfterReward = "¡Espero que te sea útil!";
    [TextArea(3, 6)]
    [SerializeField] private string dialogueAlreadyGiven = "Ya te di mi recompensa, ve con cuidado.";

    [Header("Recompensas - Dinero")]
    [SerializeField] private bool giveCoins = false;
    [SerializeField] private int coinAmount = 50;

    [Header("Recompensas - Vida")]
    [SerializeField] private bool giveHealth = false;
    [SerializeField] private int healthAmount = 1;
    [SerializeField] private bool giveMaxHealthUpgrade = false;

    [Header("Recompensas - Pociones")]
    [SerializeField] private bool givePotions = false;
    [SerializeField] private int potionAmount = 1;
    [SerializeField] private bool giveMaxPotionUpgrade = false;

    [Header("Recompensas - Hachas")]
    [SerializeField] private bool giveAxes = false;
    [SerializeField] private int axeAmount = 1;

    [Header("Recompensas - Habilidades")]
    [SerializeField] private bool unlockDoubleJump = false;
    [SerializeField] private bool unlockDash = false;
    [SerializeField] private bool unlockWallCling = false;
    [SerializeField] private bool unlockShield = false;

    [Header("Recompensas - Daño")]
    [SerializeField] private bool increaseAttackDamage = false;
    [SerializeField] private int attackDamageUpgradeAmount = 1;

    [Header("Efectos Visuales")]
    [SerializeField] private GameObject rewardParticles;
    [SerializeField] private AudioClip rewardSound;
    [SerializeField] private Animator npcAnimator;
    [SerializeField] private string rewardAnimationTrigger = "GiveReward";

    [Header("Indicador Visual")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private SpriteRenderer exclamationMark;

    private bool playerInRange = false;
    private bool rewardGiven = false;
    private Transform player;

    private void Start()
    {
        // Verificar si ya se dio la recompensa anteriormente
        if (ControladorDatosJuego.Instance != null)
        {
            rewardGiven = PlayerPrefs.GetInt($"Reward_{npcID}", 0) == 1;
        }

        // Configurar indicador visual
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        UpdateExclamationMark();
    }

    private void Update()
    {
        // Detectar jugador cercano
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            bool wasInRange = playerInRange;
            playerInRange = distance <= interactionRange;

            // Mostrar/ocultar prompt
            if (playerInRange != wasInRange && interactionPrompt != null)
            {
                interactionPrompt.SetActive(playerInRange && (!rewardGiven || canGiveRewardMultipleTimes));
            }

            // Interacción
            if (playerInRange && Input.GetKeyDown(interactKey))
            {
                Interact();
            }
        }
    }

    private void Interact()
    {
        // Ya dio la recompensa y no puede darla de nuevo
        if (rewardGiven && !canGiveRewardMultipleTimes)
        {
            ShowDialogue(dialogueAlreadyGiven);
            return;
        }

        // Mostrar diálogo inicial
        ShowDialogue(dialogueBeforeReward);

        // Dar recompensas
        GiveRewards();

        // Mostrar diálogo final
        ShowDialogue(dialogueAfterReward);

        // Marcar como entregada
        if (!canGiveRewardMultipleTimes)
        {
            rewardGiven = true;
            PlayerPrefs.SetInt($"Reward_{npcID}", 1);
            PlayerPrefs.Save();
            UpdateExclamationMark();
        }

        // Efectos visuales y sonoros
        PlayRewardEffects();
        // Destruir objetos asignados
        NPCDestroyObjects destroyer = GetComponent<NPCDestroyObjects>();
        if (destroyer != null)
        {
            destroyer.DestroyAssignedObjects();
        }

    }

    private void GiveRewards()
    {
        if (ControladorDatosJuego.Instance == null || player == null)
        {
            Debug.LogWarning("[NPCRewardSystem] No se puede dar recompensa: faltan referencias");
            return;
        }

        var datos = ControladorDatosJuego.Instance.datosjuego;
        playerLife playerLifeComponent = player.GetComponent<playerLife>();

        // Recompensa: Monedas
        if (giveCoins)
        {
            ControladorDatosJuego.Instance.AgregarMonedas(coinAmount);
            Debug.Log($"[NPC] Entregadas {coinAmount} monedas");
        }

        // Recompensa: Vida
        if (giveHealth && playerLifeComponent != null)
        {
            playerLifeComponent.Heal(healthAmount);
            Debug.Log($"[NPC] Curados {healthAmount} puntos de vida");
        }

        // Recompensa: Vida máxima
        if (giveMaxHealthUpgrade && playerLifeComponent != null)
        {
            int newMax = playerLifeComponent.MaxHealth + 1;
            playerLifeComponent.SetMaxHealth(newMax);
            datos.vidaMaxima = newMax;
            Debug.Log($"[NPC] Vida máxima aumentada a {newMax}");
        }

        // Recompensa: Pociones
        if (givePotions && playerLifeComponent != null)
        {
            playerLifeComponent.AddPotion(potionAmount);
            Debug.Log($"[NPC] Entregadas {potionAmount} pociones");
        }

        // Recompensa: Capacidad de pociones
        if (giveMaxPotionUpgrade && playerLifeComponent != null)
        {
            int newMax = playerLifeComponent.MaxPotions + 1;
            playerLifeComponent.SetMaxPotions(newMax);
            datos.maxPotions = newMax;
            Debug.Log($"[NPC] Capacidad de pociones aumentada a {newMax}");
        }

        // Recompensa: Hachas
        if (giveAxes)
        {
            datos.cantidadHachas = Mathf.Min(datos.cantidadHachas + axeAmount, datos.maxHachas);
            if (PlayerHealthUI.Instance != null)
            {
                PlayerHealthUI.Instance.ActualizarHachas(datos.cantidadHachas);
            }
            Debug.Log($"[NPC] Entregadas {axeAmount} hachas");
        }

        // Recompensa: Habilidades
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            if (unlockDoubleJump)
            {
                playerMovement.canDoubleJump = true;
                datos.hasDoubleJump = true;
                Debug.Log("[NPC] Desbloqueada: Doble salto");
            }

            if (unlockDash)
            {
                playerMovement.canDash = true;
                datos.hasDash = true;
                Debug.Log("[NPC] Desbloqueada: Dash");
            }

            if (unlockWallCling)
            {
                playerMovement.canWallCling = true;
                datos.hasWallCling = true;
                Debug.Log("[NPC] Desbloqueada: Escalada de paredes");
            }

            if (unlockShield)
            {
                playerMovement.canBlock = true;
                datos.hasShield = true;
                Debug.Log("[NPC] Desbloqueada: Escudo");
            }
        }

        // Recompensa: Daño de ataque
        if (increaseAttackDamage)
        {
            datos.attackDamageUpgrades = Mathf.Max(0, datos.attackDamageUpgrades + attackDamageUpgradeAmount);
            Debug.Log($"[NPC] Daño de ataque aumentado en {attackDamageUpgradeAmount}. Total upgrades: {datos.attackDamageUpgrades}");
        }

        // Guardar cambios
        ControladorDatosJuego.Instance.GuardarDatos(false);
    }

    private void ShowDialogue(string text)
    {
        if (TextManager.Instance != null)
        {
            TextManager.Instance.ShowDialogue(text);
        }
        else
        {
            Debug.Log($"[NPC {npcID}]: {text}");
        }
    }

    private void PlayRewardEffects()
    {
        // Animación del NPC
        if (npcAnimator != null && !string.IsNullOrEmpty(rewardAnimationTrigger))
        {
            npcAnimator.SetTrigger(rewardAnimationTrigger);
        }

        // Partículas
        if (rewardParticles != null)
        {
            GameObject particles = Instantiate(rewardParticles, transform.position, Quaternion.identity);
            Destroy(particles, 2f);
        }

        // Sonido
        if (rewardSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(rewardSound, 0.7f);
        }
    }

    private void UpdateExclamationMark()
    {
        if (exclamationMark != null)
        {
            exclamationMark.enabled = !rewardGiven || canGiveRewardMultipleTimes;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar rango de interacción
        Gizmos.color = rewardGiven ? Color.gray : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Línea hacia el jugador si está en rango
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= interactionRange)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, player.position);
            }
        }
    }

    // Métodos públicos para uso externo
    public void ForceGiveReward()
    {
        GiveRewards();
        rewardGiven = true;
    }

    public void ResetReward()
    {
        rewardGiven = false;
        PlayerPrefs.DeleteKey($"Reward_{npcID}");
        UpdateExclamationMark();
    }

    public bool HasGivenReward()
    {
        return rewardGiven;
    }
}
