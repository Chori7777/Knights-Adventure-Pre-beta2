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
    public static PlayerHealthUI SecondaryInstance;
    private playerLife player;

    [Header("Texto")]
    public TextMeshProUGUI potionText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI axeText;
    [SerializeField] private GameObject axeHud;
    [SerializeField] private bool useTimeInsteadOfScoreForSecondPlayer = false;
    [SerializeField] private string timeModeSceneName = "";
    [SerializeField] private float startingTimeSeconds = 120f;
    private float currentTimeSeconds = 0f;
    private bool timeModeActive = false;
    private int lastPotionCount = -1;

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
    [Header("Sprites Mago (3 vidas)")]
    public Sprite mage3HealthSprite;
    public Sprite mage2HealthSprite;
    public Sprite mage1HealthSprite;

    [Header("Configuración")]
    public float knightMoveDistancePerHealth = 50f;
    public float headOffsetX = 0f;
    public float headOffsetY = 0f;
    public float segmentSpacing = 50f;
    public float headMoveSpeed = 5f;

    [Header("Partículas")]
    public GameObject damageParticles;
    public Transform particleSpawnPoint;

    [Header("Escudo")]
    public float shieldBarWidth = 60f;
    public float shieldBarHeight = 6f;
    [SerializeField] private Image shieldBarImageRef;
    [SerializeField] private string shieldImageObjectName = "Shield Image";
    private Image shieldBarImage;
    private RectTransform shieldBarRect;

    private List<Image> allSwordMiddleParts = new List<Image>();
    private Vector2 targetHeadPosition;
    private Tweener headTween;
    private float initialKnightX;

    public bool IsInitialized { get; private set; }
    [Header("Visibilidad")]
    [SerializeField] private bool hideWhenNoPlayer = true;
    [SerializeField] private CanvasGroup hudGroup;
    [Header("Mage HUD")]
    [SerializeField] private Transform mageContainer;
    [SerializeField] private GameObject mageOrbPrefab;
    [SerializeField] private Sprite mageOrbFullSprite;
    [SerializeField] private Sprite mageOrbEmptySprite;
    [SerializeField] private float mageOrbSpacing = 28f;
    private List<Image> mageOrbImages = new List<Image>();
    [Header("Mage Overshield Bar")]
    [SerializeField] private float mageOvershieldWidth = 80f;
    [SerializeField] private float mageOvershieldHeight = 6f;
    [SerializeField] private Vector2 mageOvershieldOffset = new Vector2(10f, -68f);
    private Image mageOvershieldImage;
    private RectTransform mageOvershieldRect;

    [Header("HUD Secundario")]
    [SerializeField] private bool isSecondaryHUD = false;
    [SerializeField] private bool hideKnightLife = false;

    void Awake()
    {
        if (!isSecondaryHUD)
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            if (SecondaryInstance == null)
            {
                SecondaryInstance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        // Cachear referencias FIJAS (las que están en el prefab)
        CacheFixedReferences();
        SetHUDVisible(false);

        if (knightImage != null)
        {
            var rect = knightImage.GetComponent<RectTransform>();
            initialKnightX = rect != null ? rect.anchoredPosition.x : 0f;
        }
    }

    private void OnDestroy()
    {
        // ✅ Limpiar eventos
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ✅ NUEVO: Se llama cada vez que cambia la escena
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[PlayerHealthUI] Escena cargada: {scene.name}");

        // Reconectar al jugador de la nueva escena
        SetHUDVisible(false);
        StartCoroutine(ReconnectToPlayer());
    }

    // ✅ CLAVE: Busca y reconecta al nuevo jugador
    private IEnumerator ReconnectToPlayer()
    {
        // Esperar un frame para que el jugador se inicialice
        yield return new WaitForEndOfFrame();

        GameObject playerObj = null;
        if (!isSecondaryHUD)
        {
            playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                var pl = FindFirstObjectByType<playerLife>(FindObjectsInactive.Exclude);
                if (pl != null) playerObj = pl.gameObject;
            }
        }
        else
        {
            var players = GameObject.FindObjectsOfType<playerLife>(false);
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null && players[i].IsSecondCharacterMage)
                {
                    playerObj = players[i].gameObject;
                    break;
                }
            }
        }

        if (playerObj == null)
        {
            Debug.LogWarning("[PlayerHealthUI] No se encontró jugador en la escena");
            SetHUDVisible(false);
            yield break;
        }

        playerLife newPlayer = playerObj.GetComponent<playerLife>();

        if (newPlayer == null)
        {
            Debug.LogError("[PlayerHealthUI] El jugador no tiene componente playerLife");
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
        Debug.Log("[PlayerHealthUI] Reconectado al nuevo jugador");

        ConfigureTimeModeForScene();

        // ✅ Actualizar display completo
        ForceRefresh();
        SetHUDVisible(true);
    }

    private void CacheFixedReferences()
    {
        if (swordMiddle0 == null || swordMiddle1 == null || swordMiddle2 == null)
        {
            Debug.LogError("[PlayerHealthUI] FALTAN segmentos base en el Inspector");
            return;
        }

        allSwordMiddleParts.Clear();
        allSwordMiddleParts.Add(swordMiddle0);
        allSwordMiddleParts.Add(swordMiddle1);
        allSwordMiddleParts.Add(swordMiddle2);

        Debug.Log($"[PlayerHealthUI] {allSwordMiddleParts.Count} segmentos base cacheados");
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

        hideKnightLife = player.IsSecondCharacterMage;

        if (player.IsSecondCharacterMage)
        {
            HideSwordHUD();
            EnsureMageOrbsExists();
            SetAxeHUDVisible(false);
        }
        else
        {
            if (hideKnightLife)
            {
                HideSwordHUD();
            }
            else
            {
                ShowSwordHUD();
                AdjustSwordSegments(player.MaxHealth);
            }
            SetAxeHUDVisible(true);
        }
        UpdateDisplay();

        // Actualizar monedas/hachas si existe el controlador
        var controlador = ControladorDatosJuego.Instance;
        if (controlador != null)
        {
            ActualizarMonedas(controlador.ObtenerMonedas());
            ActualizarHachas(controlador.datosjuego.cantidadHachas);
        }

        IsInitialized = true;
        EnsureShieldBarExists();
        Debug.Log("[PlayerHealthUI] Inicialización completa");
    }

    public void ForceRefresh()
    {
        if (player == null)
        {
            Debug.LogWarning("[PlayerHealthUI] No hay player para refrescar");
            return;
        }

        hideKnightLife = player.IsSecondCharacterMage;

        if (player.IsSecondCharacterMage)
        {
            HideSwordHUD();
            EnsureMageOrbsExists();
            SetAxeHUDVisible(false);
        }
        else
        {
            if (hideKnightLife)
            {
                HideSwordHUD();
            }
            else
            {
                ShowSwordHUD();
                AdjustSwordSegments(player.MaxHealth);
            }
            SetAxeHUDVisible(true);
        }
        EnsureShieldBarExists();
        UpdateDisplay();

        Debug.Log("[PlayerHealthUI] Refresh forzado completado");
    }

    private void Update()
    {
        if (timeModeActive)
        {
            currentTimeSeconds = Mathf.Max(0f, currentTimeSeconds - Time.deltaTime);
            UpdateTimeText();
        }
    }

    private void SetHUDVisible(bool visible)
    {
        if (!hideWhenNoPlayer) return;
        var cg = hudGroup != null ? hudGroup : GetComponentInParent<CanvasGroup>();
        if (cg == null)
        {
            cg = GetComponentInChildren<CanvasGroup>(true);
        }
        if (cg != null)
        {
            cg.alpha = visible ? 1f : 0f;
            cg.interactable = visible;
            cg.blocksRaycasts = visible;
        }
        else
        {
            gameObject.SetActive(visible);
        }
    }

    public void SetHUDVisibility(bool visible)
    {
        SetHUDVisible(visible);
    }

    private void AdjustSwordSegments(int maxHealth)
    {
        int requiredSegments = Mathf.Max(0, maxHealth - 2);

        if (allSwordMiddleParts.Count == 0)
        {
            Debug.LogError("[PlayerHealthUI] allSwordMiddleParts vacío");
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
                    Debug.LogError($"[PlayerHealthUI] Falló añadir segmento {i + 1}/{segmentsToAdd}");
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
            Debug.LogWarning("[PlayerHealthUI] No hay player para actualizar");
            return;
        }

        UpdatePotionText();
        UpdateTimeText();
        UpdateKnightSprite();
        if (player.IsSecondCharacterMage)
        {
            UpdateMageOrbs();
            UpdateHeadPositionMage();
        }
        else
        {
            if (!hideKnightLife)
            {
                UpdateSword();
                UpdateHeadPosition();
            }
        }
        UpdateKnightPosition();
        UpdateShieldBar();
        if (player.IsSecondCharacterMage) UpdateMageOvershieldBar();
    }

    private void EnsureShieldBarExists()
    {
        if (shieldBarImage != null) return;
        if (shieldBarImageRef != null)
        {
            shieldBarImage = shieldBarImageRef;
            shieldBarRect = shieldBarImage.rectTransform;
            return;
        }
        Transform child = !string.IsNullOrEmpty(shieldImageObjectName) ? transform.Find(shieldImageObjectName) : null;
        if (child != null)
        {
            var img = child.GetComponent<Image>();
            if (img != null)
            {
                shieldBarImage = img;
                shieldBarRect = img.rectTransform;
                return;
            }
        }
        GameObject go = new GameObject("ShieldBar");
        go.transform.SetParent(transform, false);
        shieldBarImage = go.AddComponent<Image>();
        shieldBarImage.color = Color.cyan;
        shieldBarRect = shieldBarImage.rectTransform;
        shieldBarRect.anchorMin = new Vector2(0f, 1f);
        shieldBarRect.anchorMax = new Vector2(0f, 1f);
        shieldBarRect.pivot = new Vector2(0f, 1f);
        shieldBarRect.anchoredPosition = new Vector2(10f, -10f);
        shieldBarRect.sizeDelta = new Vector2(shieldBarWidth, shieldBarHeight);
    }

    private void UpdateShieldBar()
    {
        EnsureShieldBarExists();
        if (player == null)
        {
            shieldBarImage.enabled = false;
            return;
        }
        if (player.IsSecondCharacterMage)
        {
            shieldBarImage.enabled = false;
            return;
        }
        PlayerShield ps = player.GetComponent<PlayerShield>();
        if (ps == null)
        {
            shieldBarImage.enabled = false;
            return;
        }
        float ratio = ps.Stamina01;
        shieldBarImage.enabled = true;
        shieldBarRect.sizeDelta = new Vector2(Mathf.Max(0.0001f, shieldBarWidth * ratio), shieldBarHeight);
    }

    private void EnsureMageOrbsExists()
    {
        if (mageContainer == null)
        {
            var go = new GameObject("MageContainer");
            go.transform.SetParent(transform, false);
            mageContainer = go.transform;
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(10f, -40f);
        }
        int required = 3;
        if (player != null)
        {
            required = Mathf.Clamp(player.MaxHealth + player.TempShieldMax, 1, 20);
        }
        if (mageOrbImages.Count != required)
        {
            for (int i = 0; i < mageOrbImages.Count; i++)
            {
                if (mageOrbImages[i] != null)
                    Destroy(mageOrbImages[i].gameObject);
            }
            mageOrbImages.Clear();
            for (int i = 0; i < required; i++)
            {
                Image img = null;
                if (mageOrbPrefab != null)
                {
                    var o = Instantiate(mageOrbPrefab, mageContainer);
                    img = o.GetComponent<Image>();
                    if (img == null) img = o.AddComponent<Image>();
                }
                else
                {
                    var o = new GameObject("MageOrb_" + i);
                    o.transform.SetParent(mageContainer, false);
                    img = o.AddComponent<Image>();
                }
                if (img != null)
                {
                    img.sprite = mageOrbEmptySprite != null ? mageOrbEmptySprite : middleEmptySprite;
                    var rt = img.rectTransform;
                    rt.anchorMin = new Vector2(0f, 1f);
                    rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(24f, 24f);
                    float x = 10f + (i * mageOrbSpacing);
                    rt.anchoredPosition = new Vector2(x, -40f);
                    mageOrbImages.Add(img);
                }
            }
        }
        if (mageContainer != null) mageContainer.gameObject.SetActive(true);
    }

    private void UpdateMageOrbs()
    {
        EnsureMageOrbsExists();
        int fullCount = Mathf.Clamp(player.Health + player.TempShield, 0, mageOrbImages.Count);
        for (int i = 0; i < mageOrbImages.Count; i++)
        {
            var img = mageOrbImages[i];
            if (img == null) continue;
            bool full = i < fullCount;
            img.sprite = full
                ? (mageOrbFullSprite != null ? mageOrbFullSprite : middleFullSprite)
                : (mageOrbEmptySprite != null ? mageOrbEmptySprite : middleEmptySprite);
        }
    }

    private void EnsureMageOvershieldBarExists()
    {
        if (mageOvershieldImage == null)
        {
            GameObject go = new GameObject("MageOvershieldBar");
            go.transform.SetParent(transform, false);
            mageOvershieldImage = go.AddComponent<Image>();
            mageOvershieldImage.color = Color.red;
            mageOvershieldRect = mageOvershieldImage.rectTransform;
            mageOvershieldRect.anchorMin = new Vector2(0f, 1f);
            mageOvershieldRect.anchorMax = new Vector2(0f, 1f);
            mageOvershieldRect.pivot = new Vector2(0f, 1f);
            mageOvershieldRect.anchoredPosition = mageOvershieldOffset;
            mageOvershieldRect.sizeDelta = new Vector2(mageOvershieldWidth, mageOvershieldHeight);
            go.transform.SetAsLastSibling();
        }
    }

    private void UpdateMageOvershieldBar()
    {
        EnsureMageOvershieldBarExists();
        if (player == null || mageOvershieldImage == null)
        {
            if (mageOvershieldImage != null) mageOvershieldImage.enabled = false;
            return;
        }
        int tsMax = Mathf.Max(1, player.TempShieldMax);
        int ts = Mathf.Clamp(player.TempShield, 0, tsMax);
        if (ts <= 0)
        {
            mageOvershieldImage.enabled = false;
            return;
        }
        float ratio = Mathf.Clamp01((float)ts / tsMax);
        mageOvershieldImage.enabled = true;
        mageOvershieldRect.sizeDelta = new Vector2(Mathf.Max(0.0001f, mageOvershieldWidth * ratio), mageOvershieldHeight);
        Debug.Log($"[PlayerHealthUI] Mage overshield: {ts}/{tsMax} ratio={ratio}");
    }

    private void HideSwordHUD()
    {
        if (swordHandle != null) swordHandle.gameObject.SetActive(false);
        if (swordTip != null) swordTip.gameObject.SetActive(false);
        if (knightImage != null) knightImage.gameObject.SetActive(false);
        if (knightHeadImage != null) knightHeadImage.gameObject.SetActive(false);
        for (int i = 0; i < allSwordMiddleParts.Count; i++)
        {
            if (allSwordMiddleParts[i] != null)
                allSwordMiddleParts[i].gameObject.SetActive(false);
        }
    }

    private void ShowSwordHUD()
    {
        if (swordHandle != null) swordHandle.gameObject.SetActive(true);
        if (swordTip != null) swordTip.gameObject.SetActive(true);
        if (knightImage != null) knightImage.gameObject.SetActive(true);
        if (knightHeadImage != null) knightHeadImage.gameObject.SetActive(true);
        for (int i = 0; i < allSwordMiddleParts.Count; i++)
        {
            if (allSwordMiddleParts[i] != null)
                allSwordMiddleParts[i].gameObject.SetActive(true);
        }
        if (mageContainer != null) mageContainer.gameObject.SetActive(false);
    }

    private void UpdateHeadPositionMage()
    {
        if (player == null || knightHeadImage == null) return;
        if (mageOrbImages.Count == 0) return;
        RectTransform headRect = knightHeadImage.GetComponent<RectTransform>();
        int index = Mathf.Clamp(player.Health + player.TempShield - 1, 0, mageOrbImages.Count - 1);
        Vector2 target = mageOrbImages[index].rectTransform.anchoredPosition + new Vector2(headOffsetX, headOffsetY);
        targetHeadPosition = target;
        if (headTween != null && headTween.IsActive())
            headTween.Kill();
        headTween = headRect.DOAnchorPos(targetHeadPosition, 0.35f).SetEase(Ease.OutQuad);
    }

    void UpdatePotionText()
    {
        if (potionText != null && player != null)
        {
            potionText.text = player.Potions + "/" + player.MaxPotions;
            if (lastPotionCount >= 0 && player.Potions > lastPotionCount)
            {
                PulsePotionText();
            }
            lastPotionCount = player.Potions;
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

        bool useMage = player != null && player.IsSecondCharacterMage;
        if (useMage)
        {
            if (h >= 3 && mage3HealthSprite != null) currentSprite = mage3HealthSprite;
            else if (h == 2 && mage2HealthSprite != null) currentSprite = mage2HealthSprite;
            else if (h == 1 && mage1HealthSprite != null) currentSprite = mage1HealthSprite;
        }
        else
        {
            if (h >= 5 && knight5HealthSprite != null) currentSprite = knight5HealthSprite;
            else if (h == 4 && knight4HealthSprite != null) currentSprite = knight4HealthSprite;
            else if (h == 3 && knight3HealthSprite != null) currentSprite = knight3HealthSprite;
            else if (h == 2 && knight2HealthSprite != null) currentSprite = knight2HealthSprite;
        }

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

    private void SetAxeHUDVisible(bool visible)
    {
        if (axeHud != null) axeHud.SetActive(visible);
        if (axeText != null) axeText.gameObject.SetActive(visible);
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
        Vector2 newPos = knightRect.anchoredPosition;
        newPos.x = initialKnightX;
        knightRect.anchoredPosition = newPos;
    }

    private void ConfigureTimeModeForScene()
    {
        timeModeActive = false;
        if (!useTimeInsteadOfScoreForSecondPlayer) return;
        if (player == null || !player.IsSecondCharacterMage) return;
        if (!string.IsNullOrEmpty(timeModeSceneName))
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.name != timeModeSceneName) return;
        }
        currentTimeSeconds = startingTimeSeconds;
        timeModeActive = true;
        UpdateTimeText();
    }

    private void UpdateTimeText()
    {
        if (!timeModeActive || coinText == null) return;
        int t = Mathf.RoundToInt(currentTimeSeconds);
        int minutes = t / 60;
        int seconds = t % 60;
        coinText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    public void AddTime(float seconds)
    {
        currentTimeSeconds = Mathf.Max(0f, currentTimeSeconds + seconds);
        UpdateTimeText();
    }

    private void PulsePotionText()
    {
        if (potionText == null) return;
        var cg = potionText.GetComponent<CanvasGroup>();
        if (cg == null) cg = potionText.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.DOFade(0.2f, 0.2f).SetLoops(2, LoopType.Yoyo);
        potionText.rectTransform.DOPunchScale(new Vector3(0.1f, 0.1f, 0f), 0.3f, 1, 0.5f);
    }
}
