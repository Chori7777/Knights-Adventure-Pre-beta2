using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealthUI : MonoBehaviour
{
    public static PlayerHealthUI Instance;
    private playerLife player;

    [Header("Texto")]
    public TextMeshProUGUI potionText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI axeText;

    [Header("Espada - Referencias FIJAS")]
    public Image swordHandle;
    public Image swordMiddle0;
    public Image swordMiddle1;
    public Image swordMiddle2;
    public Image swordTip;

    [Header("Espada - Prefabs")]
    public GameObject swordMiddlePrefab;
    public Transform swordContainer;

    [Header("Sprites Espada")]
    public Sprite handleFullSprite;
    public Sprite middleFullSprite;
    public Sprite tipFullSprite;
    public Sprite handleEmptySprite;
    public Sprite middleEmptySprite;
    public Sprite tipEmptySprite;

    [Header("Caballero")]
    public Image knightImage;
    public Image knightHeadImage;

    [Header("Sprites Caballero")]
    public Sprite knight5HealthSprite;
    public Sprite knight4HealthSprite;
    public Sprite knight3HealthSprite;
    public Sprite knight2HealthSprite;
    public Sprite knight1HealthSprite;

    [Header("Configuración")]
    public float knightMoveDistancePerHealth = 50f;
    public float headOffsetX = 0f;
    public float headOffsetY = 0f;
    public float segmentSpacing = 50f;
    public float headMoveSpeed = 5f;

    [Header("Partículas")]
    public GameObject damageParticles;
    public Transform particleSpawnPoint;

    private List<Image> allSwordMiddleParts = new List<Image>();
    private Vector2 targetHeadPosition;
    private Tweener headTween;

    public bool IsInitialized { get; private set; }

    void Awake()
    {
        // Singleton persistente
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // ✅ Suscribirse a cambios de escena
            SceneManager.sceneLoaded += OnSceneLoaded;

            Debug.Log("✅ [PlayerHealthUI] HUD creado y persistente");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Cachear referencias FIJAS (las que están en el prefab)
        CacheFixedReferences();
    }

    private void OnDestroy()
    {
        // ✅ Limpiar eventos
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ✅ NUEVO: Se llama cada vez que cambia la escena
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"🔄 [PlayerHealthUI] Escena cargada: {scene.name}");

        // Reconectar al jugador de la nueva escena
        StartCoroutine(ReconnectToPlayer());
    }

    // ✅ CLAVE: Busca y reconecta al nuevo jugador
    private IEnumerator ReconnectToPlayer()
    {
        // Esperar un frame para que el jugador se inicialice
        yield return new WaitForEndOfFrame();

        // Buscar el jugador en la nueva escena
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj == null)
        {
            Debug.LogWarning("⚠️ [PlayerHealthUI] No se encontró jugador en la escena");
            yield break;
        }

        playerLife newPlayer = playerObj.GetComponent<playerLife>();

        if (newPlayer == null)
        {
            Debug.LogError("❌ [PlayerHealthUI] El jugador no tiene componente playerLife");
            yield break;
        }

        // ✅ Esperar a que el jugador esté inicializado
        float timeout = 2f;
        float elapsed = 0f;

        while (!newPlayer.IsInitialized && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ✅ Reconectar
        player = newPlayer;
        Debug.Log("🔗 [PlayerHealthUI] Reconectado al nuevo jugador");

        // ✅ Actualizar display completo
        ForceRefresh();
    }

    private void CacheFixedReferences()
    {
        if (swordMiddle0 == null || swordMiddle1 == null || swordMiddle2 == null)
        {
            Debug.LogError("❌ [PlayerHealthUI] FALTAN segmentos base en el Inspector");
            return;
        }

        allSwordMiddleParts.Clear();
        allSwordMiddleParts.Add(swordMiddle0);
        allSwordMiddleParts.Add(swordMiddle1);
        allSwordMiddleParts.Add(swordMiddle2);

        Debug.Log($"✅ [PlayerHealthUI] {allSwordMiddleParts.Count} segmentos base cacheados");
    }

    public void Initialize(playerLife p)
    {
        if (p == null)
        {
            Debug.LogWarning("[PlayerHealthUI] Initialize recibió player null");
            return;
        }

        player = p;
        Debug.Log($"[PlayerHealthUI] Inicializando con vida {player.Health}/{player.MaxHealth}");

        AdjustSwordSegments(player.MaxHealth);
        UpdateDisplay();

        // Actualizar monedas/hachas si existe el controlador
        var controlador = ControladorDatosJuego.Instance;
        if (controlador != null)
        {
            ActualizarMonedas(controlador.ObtenerMonedas());
            ActualizarHachas(controlador.datosjuego.cantidadHachas);
        }

        IsInitialized = true;
        Debug.Log("✅ [PlayerHealthUI] Inicialización completa");
    }

    public void ForceRefresh()
    {
        if (player == null)
        {
            Debug.LogWarning("⚠️ [PlayerHealthUI] No hay player para refrescar");
            return;
        }

        AdjustSwordSegments(player.MaxHealth);
        UpdateDisplay();

        Debug.Log("🔄 [PlayerHealthUI] Refresh forzado completado");
    }

    private void AdjustSwordSegments(int maxHealth)
    {
        int requiredSegments = Mathf.Max(0, maxHealth - 2);

        if (allSwordMiddleParts.Count == 0)
        {
            Debug.LogError("❌ allSwordMiddleParts vacío");
            return;
        }

        // Añadir segmentos si es necesario
        int segmentsToAdd = requiredSegments - allSwordMiddleParts.Count;
        if (segmentsToAdd > 0)
        {
            for (int i = 0; i < segmentsToAdd; i++)
            {
                if (!AddSwordSegmentDynamic())
                {
                    Debug.LogError($"❌ Falló añadir segmento {i + 1}/{segmentsToAdd}");
                    break;
                }
            }
        }

        // Activar/desactivar según vida máxima
        for (int i = 0; i < allSwordMiddleParts.Count; i++)
        {
            if (allSwordMiddleParts[i] != null)
            {
                bool shouldBeActive = i < requiredSegments;
                allSwordMiddleParts[i].gameObject.SetActive(shouldBeActive);
            }
        }

        RepositionSwordSegments();
    }

    private bool AddSwordSegmentDynamic()
    {
        if (swordMiddlePrefab != null && swordContainer != null)
        {
            GameObject newSegment = Instantiate(swordMiddlePrefab, swordContainer);
            Image segmentImage = newSegment.GetComponent<Image>();

            if (segmentImage != null)
            {
                allSwordMiddleParts.Add(segmentImage);
                segmentImage.sprite = middleFullSprite;
                return true;
            }
        }
        else if (allSwordMiddleParts.Count > 0 && allSwordMiddleParts[0] != null)
        {
            GameObject cloned = Instantiate(allSwordMiddleParts[0].gameObject, allSwordMiddleParts[0].transform.parent);
            Image clonedImage = cloned.GetComponent<Image>();

            if (clonedImage != null)
            {
                clonedImage.sprite = middleFullSprite;
                allSwordMiddleParts.Add(clonedImage);
                return true;
            }
        }

        return false;
    }

    private void RepositionSwordSegments()
    {
        if (swordHandle == null || swordTip == null) return;

        RectTransform handleRect = swordHandle.GetComponent<RectTransform>();
        RectTransform tipRect = swordTip.GetComponent<RectTransform>();

        // Posicionar segmentos medios
        for (int i = 0; i < allSwordMiddleParts.Count; i++)
        {
            if (allSwordMiddleParts[i] != null && allSwordMiddleParts[i].gameObject.activeSelf)
            {
                RectTransform segmentRect = allSwordMiddleParts[i].GetComponent<RectTransform>();
                float yPos = handleRect.anchoredPosition.y + ((i + 1) * segmentSpacing);
                segmentRect.anchoredPosition = new Vector2(handleRect.anchoredPosition.x, yPos);
            }
        }

        // Posicionar punta
        float tipYPos = handleRect.anchoredPosition.y + ((allSwordMiddleParts.Count + 1) * segmentSpacing);
        tipRect.anchoredPosition = new Vector2(handleRect.anchoredPosition.x, tipYPos);
    }

    public void UpdateDisplay()
    {
        if (player == null)
        {
            Debug.LogWarning("⚠️ [PlayerHealthUI] No hay player para actualizar");
            return;
        }

        UpdatePotionText();
        UpdateKnightSprite();
        UpdateSword();
        UpdateHeadPosition();
        UpdateKnightPosition();
    }

    void UpdatePotionText()
    {
        if (potionText != null && player != null)
        {
            potionText.text = player.Potions + "/" + player.MaxPotions;
        }
    }

    public void ActualizarMonedas(int cantidad)
    {
        if (coinText != null)
        {
            coinText.text = cantidad.ToString();
        }
    }

    public void ActualizarHachas(int cantidad)
    {
        if (axeText != null)
        {
            int maxHachas = ControladorDatosJuego.Instance?.datosjuego.maxHachas ?? 3;
            axeText.text = cantidad + "/" + maxHachas;
        }
    }

    void UpdateKnightSprite()
    {
        if (player == null || knightImage == null) return;

        Sprite currentSprite = knight1HealthSprite;
        int h = Mathf.Clamp(player.Health, 0, player.MaxHealth);

        if (h >= 5 && knight5HealthSprite != null) currentSprite = knight5HealthSprite;
        else if (h == 4 && knight4HealthSprite != null) currentSprite = knight4HealthSprite;
        else if (h == 3 && knight3HealthSprite != null) currentSprite = knight3HealthSprite;
        else if (h == 2 && knight2HealthSprite != null) currentSprite = knight2HealthSprite;

        if (currentSprite != null)
        {
            knightImage.sprite = currentSprite;
            knightHeadImage.sprite = currentSprite;
        }
    }

    void UpdateSword()
    {
        if (player == null) return;

        int h = player.Health;
        int max = player.MaxHealth;

        // Punta (vida máxima)
        if (swordTip != null)
        {
            swordTip.sprite = (h == max) ? tipFullSprite : tipEmptySprite;
        }

        // Segmentos medios (invertidos)
        for (int i = 0; i < allSwordMiddleParts.Count; i++)
        {
            if (allSwordMiddleParts[i] != null && allSwordMiddleParts[i].gameObject.activeSelf)
            {
                int displayIndex = allSwordMiddleParts.Count - 1 - i;
                int healthValue = max - displayIndex - 1;
                bool isFull = h >= healthValue;

                allSwordMiddleParts[i].sprite = isFull ? middleFullSprite : middleEmptySprite;
            }
        }

        // Mango (vida >= 1)
        if (swordHandle != null)
        {
            swordHandle.sprite = (h >= 1) ? handleFullSprite : handleEmptySprite;
        }
    }

    void UpdateHeadPosition()
    {
        if (player == null || knightHeadImage == null) return;

        RectTransform headRect = knightHeadImage.GetComponent<RectTransform>();
        int h = player.Health;
        int max = player.MaxHealth;

        Vector2 target = Vector2.zero;

        if (h <= 0)
        {
            target = swordHandle.GetComponent<RectTransform>().anchoredPosition;
        }
        else if (h >= max)
        {
            target = swordTip.GetComponent<RectTransform>().anchoredPosition;
        }
        else
        {
            int segmentIndex = max - h - 1;
            int arrayIndex = Mathf.Clamp(allSwordMiddleParts.Count - 1 - segmentIndex, 0, allSwordMiddleParts.Count - 1);
            target = allSwordMiddleParts[arrayIndex].GetComponent<RectTransform>().anchoredPosition;
        }

        target += new Vector2(headOffsetX, headOffsetY);
        targetHeadPosition = target;

        if (headTween != null && headTween.IsActive())
            headTween.Kill();

        headTween = headRect.DOAnchorPos(targetHeadPosition, 0.35f).SetEase(Ease.OutQuad);
    }

    void UpdateKnightPosition()
    {
        if (player == null || knightImage == null) return;

        RectTransform knightRect = knightImage.GetComponent<RectTransform>();
        float healthLost = player.MaxHealth - player.Health;
        float moveAmount = healthLost * knightMoveDistancePerHealth;

        Vector2 newPos = knightRect.anchoredPosition;
        newPos.x = -moveAmount;
        knightRect.anchoredPosition = newPos;
    }
}