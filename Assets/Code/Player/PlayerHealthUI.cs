using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PlayerHealthUI : MonoBehaviour
{
    public static PlayerHealthUI Instance;

    private playerLife player;

    [Header("Texto")]
    public TextMeshProUGUI potionText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI axeText;

    [Header("Espada - Prefabs (OPCIONAL para expansión dinámica)")]
    public GameObject swordHandlePrefab;
    public GameObject swordMiddlePrefab;
    public GameObject swordTipPrefab;

    [Header("Espada - Referencias FIJAS (Las 5 partes base)")]
    public Image swordHandle;           // Mango (abajo)
    public Image swordMiddle0;          // Segmento inferior
    public Image swordMiddle1;          // Segmento medio
    public Image swordMiddle2;          // Segmento superior
    public Image swordTip;              // Punta (arriba)

    [Header("Espada - Contenedor (para segmentos extra)")]
    public Transform swordContainer;

    [Header("Sprites Espada Llena")]
    public Sprite handleFullSprite;
    public Sprite middleFullSprite;
    public Sprite tipFullSprite;

    [Header("Sprites Espada Vacía")]
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

    [Header("Animación de Cabeza")]
    public bool smoothHeadMovement = true;
    public float headMoveSpeed = 5f;

    [Header("Partículas de Daño")]
    public GameObject damageParticles;
    public Transform particleSpawnPoint;


    private List<Image> allSwordMiddleParts = new List<Image>();

    private Vector2 targetHeadPosition;
    private Tweener headTween;   


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Orden: Deabajo a arriba (se vacían de arriba hacia abajo)
        if (swordMiddle0 != null) allSwordMiddleParts.Add(swordMiddle0); // Inferior
        if (swordMiddle1 != null) allSwordMiddleParts.Add(swordMiddle1); // Medio
        if (swordMiddle2 != null) allSwordMiddleParts.Add(swordMiddle2); // Superior

        Debug.Log($"[PlayerHealthUI] Inicializado con {allSwordMiddleParts.Count} segmentos base");
    }

    public void Initialize(playerLife p)
    {
        if (p == null)
        {
            Debug.LogWarning("[PlayerHealthUI] Initialize recibió player null.");
            return;
        }

        player = p;

        try
        {
            Debug.Log($"[PlayerHealthUI] Inicializando con vida {player.Health}/{player.MaxHealth}");

            // Ajustar segmentos según vida máxima
            AdjustSwordSegments(player.MaxHealth);

            // FORZAR actualización completa
            UpdateDisplay();

            var controlador = ControladorDatosJuego.Instance;
            if (controlador != null)
            {
                ActualizarMonedas(controlador.ObtenerMonedas());
                ActualizarHachas(controlador.datosjuego.cantidadHachas);
            }
            else
            {
                if (coinText != null) coinText.text = "0";
                if (axeText != null) axeText.text = "0";
            }

            Debug.Log("HUD  Iniciado  correctamente");
        }
        catch (System.Exception ex)
        {

        }
    }
    public void ForceRefresh()
    {
        if (player == null)
        {
            Debug.LogWarning("El jugador no esta asignado");
            return;
        }


        AdjustSwordSegments(player.MaxHealth);
        UpdateDisplay();
    }

    private void AdjustSwordSegments(int maxHealth)
    {
        int requiredSegments = Mathf.Max(0, maxHealth - 2);

        // Verificar que la lista base esté inicializada
        if (allSwordMiddleParts.Count == 0)
        {
            Debug.LogError("allSwordMiddleParts está vacio..");
            Debug.LogError("Verificarse que swordMiddle0, swordMiddle1, swordMiddle2 estén asignados en Awake(), si no claramente no funcionara");
            return;
        }

        // Si necesitamos MÁS segmentos (aumentar vida)
        int segmentsToAdd = requiredSegments - allSwordMiddleParts.Count;
        if (segmentsToAdd > 0)
        {

            for (int i = 0; i < segmentsToAdd; i++)
            {
                bool success = AddSwordSegmentDynamic();
                if (!success)
                {
                    Debug.LogError($"falló al añadir segmento {i + 1}/{segmentsToAdd}");
                    break;
                }
            }
        }

        // Activar/desactivar segmentos según sea necesario
        for (int i = 0; i < allSwordMiddleParts.Count; i++)
        {
            if (allSwordMiddleParts[i] != null)
            {
                bool shouldBeActive = i < requiredSegments;
                allSwordMiddleParts[i].gameObject.SetActive(shouldBeActive);

                if (shouldBeActive)
                {
                    Debug.Log($"Segmento {i}: ACTIVO");
                }
            }
            else
            {
                Debug.LogWarning($"Segmento {i} es NULL");
            }
        }

        Debug.Log($"Ajuste completado. Total activos: {requiredSegments}");

        RepositionSwordSegments();
    }

    private bool AddSwordSegmentDynamic()
    {
        Debug.Log("Intentando añadir segmento...");


        if (swordMiddlePrefab != null && swordContainer != null)
        {
            Debug.Log("Usando prefab");

            GameObject newSegment = Instantiate(swordMiddlePrefab, swordContainer);
            Image segmentImage = newSegment.GetComponent<Image>();

            if (segmentImage != null)
            {
                allSwordMiddleParts.Add(segmentImage);
                segmentImage.sprite = middleFullSprite;

                Debug.Log($"Segmento creado desde PREFAB. Total: {allSwordMiddleParts.Count}");
                return true;
            }
            else
            {
                Debug.LogError("El prefab no tiene componente Image");
                Destroy(newSegment);
                return false;
            }
        }
        // OPCIÓN 2: Clonar un segmento existente
        else if (allSwordMiddleParts.Count > 0 && allSwordMiddleParts[0] != null)
        {
            Debug.Log("Clonando segmento existente");

            GameObject clonedSegment = Instantiate(allSwordMiddleParts[0].gameObject, allSwordMiddleParts[0].transform.parent);
            Image clonedImage = clonedSegment.GetComponent<Image>();

            if (clonedImage != null)
            {
                clonedImage.sprite = middleFullSprite;
                clonedSegment.name = $"SwordHUD_Cloned_{allSwordMiddleParts.Count}";
                allSwordMiddleParts.Add(clonedImage);

                Debug.Log($"Segmento CLONADO. Total: {allSwordMiddleParts.Count}");
                return true;
            }
            else
            {
                Debug.LogError("El clon no tiene componente Image");
                Destroy(clonedSegment);
                return false;
            }
        }
        // OPCIÓN 3: Crear desde cero (último recurso)
        else
        {
            Debug.LogError("Asigna 'swordMiddlePrefab' en el Inspector o verifica que los segmentos base existan");
            return false;
        }
    }

    private void RepositionSwordSegments()
    {
        if (swordHandle == null || swordTip == null) return;

        RectTransform handleRect = swordHandle.GetComponent<RectTransform>();
        RectTransform tipRect = swordTip.GetComponent<RectTransform>();

        if (handleRect == null || tipRect == null) return;

        // Posicionar cada segmento medio
        for (int i = 0; i < allSwordMiddleParts.Count; i++)
        {
            if (allSwordMiddleParts[i] != null && allSwordMiddleParts[i].gameObject.activeSelf)
            {
                RectTransform segmentRect = allSwordMiddleParts[i].GetComponent<RectTransform>();
                if (segmentRect != null)
                {
                    // Posición desde el mango hacia arriba
                    float yPos = handleRect.anchoredPosition.y + ((i + 1) * segmentSpacing);
                    segmentRect.anchoredPosition = new Vector2(handleRect.anchoredPosition.x, yPos);
                }
            }
        }

        // Posicionar punta al final
        float tipYPos = handleRect.anchoredPosition.y + ((allSwordMiddleParts.Count + 1) * segmentSpacing);
        tipRect.anchoredPosition = new Vector2(handleRect.anchoredPosition.x, tipYPos);
    }

    public void UpdateDisplay()
    {
        if (player == null) return;

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
        if (player == null || knightImage == null || knightHeadImage == null) return;

        Sprite currentSprite = knight1HealthSprite;
        int h = Mathf.Clamp(player.Health, 0, player.MaxHealth);

        if (h >= 5 && knight5HealthSprite != null) currentSprite = knight5HealthSprite;
        else if (h == 4 && knight4HealthSprite != null) currentSprite = knight4HealthSprite;
        else if (h == 3 && knight3HealthSprite != null) currentSprite = knight3HealthSprite;
        else if (h == 2 && knight2HealthSprite != null) currentSprite = knight2HealthSprite;
        else if (knight1HealthSprite != null) currentSprite = knight1HealthSprite;

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

        // DEBUG
        Debug.Log($"[UpdateSword] Vida: {h}/{max}");

     
        if (swordTip != null)
        {
            bool tipFull = (h == max); 
            swordTip.sprite = tipFull ? tipFullSprite : tipEmptySprite;
            Debug.Log($"Punta: {(tipFull ? "LLENA" : "VACÍA")} (vida {h} == max {max})");
        }

 
        for (int i = 0; i < allSwordMiddleParts.Count; i++)
        {
            if (allSwordMiddleParts[i] != null && allSwordMiddleParts[i].gameObject.activeSelf)
            {
                // Invertir el índice para que el superior (último en el array) se vacíe primero
                int displayIndex = allSwordMiddleParts.Count - 1 - i;

                // Cada segmento representa 1 punto de vida
                // Segmento superior (displayIndex = N-1) = vida (max - 1)
                // Segmento medio (displayIndex = 1) = vida (max - N + 1)
                // Segmento inferior (displayIndex = 0) = vida 2
                int healthValue = max - displayIndex - 1;
                bool isFull = h >= healthValue; 

                allSwordMiddleParts[i].sprite = isFull ? middleFullSprite : middleEmptySprite;

                Debug.Log($"Segmento {i} (display {displayIndex}): {(isFull ? "LLENO" : "VACÍO")} (vida {h} >= {healthValue})");
            }
        }

   
        if (swordHandle != null)
        {
            bool handleFull = (h >= 1); 
            swordHandle.sprite = handleFull ? handleFullSprite : handleEmptySprite;
            Debug.Log($"Mango: {(handleFull ? "LLENO" : "VACÍO")} (vida {h} >= 1)");
        }
    }

    void UpdateHeadPosition()
    {
        if (player == null || knightHeadImage == null) return;

        RectTransform headRect = knightHeadImage.GetComponent<RectTransform>();
        if (headRect == null) return;

        int h = player.Health;
        int max = player.MaxHealth;

        Vector2 target = Vector2.zero;


        if (h <= 0)
        {
            RectTransform handleRect = swordHandle.GetComponent<RectTransform>();
            target = handleRect.anchoredPosition;
        }

        else if (h >= max)
        {
            RectTransform tipRect = swordTip.GetComponent<RectTransform>();
            target = tipRect.anchoredPosition;
        }
        else
        {
            // VIDA INTERMEDIA → segmento correspondiente
            int segmentIndex = max - h - 1;
            int arrayIndex = allSwordMiddleParts.Count - 1 - segmentIndex;

            arrayIndex = Mathf.Clamp(arrayIndex, 0, allSwordMiddleParts.Count - 1);

            Image seg = allSwordMiddleParts[arrayIndex];
            if (seg != null)
            {
                RectTransform segRect = seg.GetComponent<RectTransform>();
                target = segRect.anchoredPosition;
            }
        }

        // APLICAR OFFSET
        target += new Vector2(headOffsetX, headOffsetY);

        // Guardar destino
        targetHeadPosition = target;

        // CANCELAR tween anterior si existe
        if (headTween != null && headTween.IsActive())
            headTween.Kill();

        // ANIMACIÓN SUAVE CON DOTWEEN
        headTween = headRect.DOAnchorPos(targetHeadPosition, 0.35f)
                            .SetEase(Ease.OutQuad);
    }

    void UpdateKnightPosition()
    {
        if (player == null || knightImage == null) return;

        RectTransform knightRect = knightImage.GetComponent<RectTransform>();
        if (knightRect == null) return;

        float healthLost = player.MaxHealth - player.Health;
        float moveAmount = healthLost * knightMoveDistancePerHealth;

        Vector2 newPos = knightRect.anchoredPosition;
        newPos.x = -moveAmount;
        knightRect.anchoredPosition = newPos;
    }
}