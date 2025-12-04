using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sistema para conectar múltiples proyectiles que se mueven sincronizados
/// Perfecto para patrones de ataque complejos
/// </summary>
public class ConnectedProjectilesSystem : MonoBehaviour
{
    [Header("═══════════════════════════════")]
    [Header("MODO DE CONEXIÓN")]
    [Header("═══════════════════════════════")]

    [SerializeField] private ConnectionMode mode = ConnectionMode.ParentChild;

    public enum ConnectionMode
    {
        ParentChild,    // Proyectiles hijos del padre (más simple)
        Synchronized,   // Proyectiles independientes sincronizados
        Formation       // Formación con offsets relativos
    }

    [Header("═══════════════════════════════")]
    [Header("CONFIGURACIÓN PRINCIPAL")]
    [Header("═══════════════════════════════")]

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int projectileCount = 2;
    [SerializeField] private Vector2 spacing = new Vector2(0, 2f); // Separación entre proyectiles

    [Header("Movimiento Compartido")]
    [SerializeField] private Vector2 globalDirection = Vector2.right;
    [SerializeField] private float globalSpeed = 5f;

    [Header("Movimiento Individual")]
    [SerializeField] private bool enableIndividualOscillation = true;
    [SerializeField] private float oscillationAmplitude = 1f;
    [SerializeField] private float oscillationFrequency = 2f;
    [SerializeField] private bool alternateOscillation = true; // Oscilan en direcciones opuestas

    [Header("═══════════════════════════════")]
    [Header("PATRONES PREDEFINIDOS")]
    [Header("═══════════════════════════════")]

    [SerializeField] private bool usePattern = false;
    [SerializeField] private PatternPreset pattern = PatternPreset.VerticalLine;

    public enum PatternPreset
    {
        VerticalLine,      // │ (espadas verticales)
        HorizontalLine,    // ─ (espadas horizontales)
        Diagonal,          // ╱ (diagonal)
        VShape,            // ∨ (forma de V)
        Circle,            // ○ (círculo)
        Grid,              // ▦ (cuadrícula)
        Wave               // ∿ (onda)
    }

    [Header("═══════════════════════════════")]
    [Header("ROTACIÓN")]
    [Header("═══════════════════════════════")]

    [SerializeField] private bool rotateFormation = false;
    [SerializeField] private float rotationSpeed = 45f;

    [Header("═══════════════════════════════")]
    [Header("DESTRUCCIÓN")]
    [Header("═══════════════════════════════")]

    [SerializeField] private float lifetime = 10f;
    [SerializeField] private bool destroyWhenOneHits = false;

    // ═══════════════════════════════════════════════════
    // VARIABLES INTERNAS
    // ═══════════════════════════════════════════════════

    private List<GameObject> projectiles = new List<GameObject>();
    private List<Vector3> relativeOffsets = new List<Vector3>();
    private float timeAlive = 0f;

    // ═══════════════════════════════════════════════════
    // INICIALIZACIÓN
    // ═══════════════════════════════════════════════════

    private void Start()
    {
        SpawnConnectedProjectiles();
    }

    public void SpawnConnectedProjectiles()
    {
        // Limpiar proyectiles anteriores
        foreach (var proj in projectiles)
        {
            if (proj != null) Destroy(proj);
        }
        projectiles.Clear();
        relativeOffsets.Clear();

        // Usar patrón predefinido si está activado
        if (usePattern)
        {
            SpawnPattern();
        }
        else
        {
            SpawnCustom();
        }

        // Configurar según modo
        switch (mode)
        {
            case ConnectionMode.ParentChild:
                SetupParentChild();
                break;
            case ConnectionMode.Synchronized:
                SetupSynchronized();
                break;
            case ConnectionMode.Formation:
                SetupFormation();
                break;
        }
    }

    // ═══════════════════════════════════════════════════
    // SPAWN CUSTOM
    // ═══════════════════════════════════════════════════

    private void SpawnCustom()
    {
        for (int i = 0; i < projectileCount; i++)
        {
            Vector3 offset = spacing * (i - (projectileCount - 1) / 2f);
            Vector3 spawnPos = transform.position + offset;

            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            projectiles.Add(proj);
            relativeOffsets.Add(offset);
        }
    }

    // ═══════════════════════════════════════════════════
    // PATRONES PREDEFINIDOS
    // ═══════════════════════════════════════════════════

    private void SpawnPattern()
    {
        switch (pattern)
        {
            case PatternPreset.VerticalLine:
                SpawnVerticalLine();
                break;
            case PatternPreset.HorizontalLine:
                SpawnHorizontalLine();
                break;
            case PatternPreset.Diagonal:
                SpawnDiagonal();
                break;
            case PatternPreset.VShape:
                SpawnVShape();
                break;
            case PatternPreset.Circle:
                SpawnCircle();
                break;
            case PatternPreset.Grid:
                SpawnGrid();
                break;
            case PatternPreset.Wave:
                SpawnWave();
                break;
        }
    }

    private void SpawnVerticalLine()
    {
        // │ Línea vertical
        for (int i = 0; i < projectileCount; i++)
        {
            Vector3 offset = Vector3.up * spacing.y * (i - (projectileCount - 1) / 2f);
            SpawnProjectileAt(offset);
        }
    }

    private void SpawnHorizontalLine()
    {
        // ─ Línea horizontal
        for (int i = 0; i < projectileCount; i++)
        {
            Vector3 offset = Vector3.right * spacing.x * (i - (projectileCount - 1) / 2f);
            SpawnProjectileAt(offset);
        }
    }

    private void SpawnDiagonal()
    {
        // ╱ Diagonal
        for (int i = 0; i < projectileCount; i++)
        {
            float t = (i - (projectileCount - 1) / 2f);
            Vector3 offset = new Vector3(spacing.x * t, spacing.y * t, 0);
            SpawnProjectileAt(offset);
        }
    }

    private void SpawnVShape()
    {
        // ∨ Forma de V
        int half = projectileCount / 2;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = (i < half) ? -45f : 45f;
            float distance = Mathf.Abs(i - half) * spacing.magnitude;

            Vector3 offset = Quaternion.Euler(0, 0, angle) * Vector3.up * distance;
            SpawnProjectileAt(offset);
        }
    }

    private void SpawnCircle()
    {
        // ○ Círculo
        float radius = spacing.magnitude;
        float angleStep = 360f / projectileCount;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0
            );
            SpawnProjectileAt(offset);
        }
    }

    private void SpawnGrid()
    {
        // ▦ Cuadrícula
        int rows = Mathf.CeilToInt(Mathf.Sqrt(projectileCount));
        int cols = Mathf.CeilToInt((float)projectileCount / rows);

        for (int i = 0; i < projectileCount; i++)
        {
            int row = i / cols;
            int col = i % cols;

            Vector3 offset = new Vector3(
                (col - (cols - 1) / 2f) * spacing.x,
                (row - (rows - 1) / 2f) * spacing.y,
                0
            );
            SpawnProjectileAt(offset);
        }
    }

    private void SpawnWave()
    {
        // ∿ Onda sinusoidal
        for (int i = 0; i < projectileCount; i++)
        {
            float t = (float)i / (projectileCount - 1);
            Vector3 offset = new Vector3(
                (t - 0.5f) * spacing.x * projectileCount,
                Mathf.Sin(t * Mathf.PI * 2) * spacing.y,
                0
            );
            SpawnProjectileAt(offset);
        }
    }

    private void SpawnProjectileAt(Vector3 offset)
    {
        Vector3 spawnPos = transform.position + offset;
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        projectiles.Add(proj);
        relativeOffsets.Add(offset);
    }

    // ═══════════════════════════════════════════════════
    // CONFIGURACIÓN DE MODOS
    // ═══════════════════════════════════════════════════

    private void SetupParentChild()
    {
        // Hacer todos los proyectiles hijos de este GameObject
        foreach (var proj in projectiles)
        {
            if (proj != null)
            {
                proj.transform.SetParent(transform);

                // Desactivar UniversalProjectileMover si existe
                var mover = proj.GetComponent<UniversalProjectileMover>();
                if (mover != null)
                {
                    mover.enabled = false;
                }
            }
        }
    }

    private void SetupSynchronized()
    {
        // Configurar cada proyectil con la misma dirección y velocidad
        foreach (var proj in projectiles)
        {
            if (proj != null)
            {
                var mover = proj.GetComponent<UniversalProjectileMover>();
                if (mover != null)
                {
                    mover.SetCustomDirection(globalDirection);
                    mover.SetMainDirection(UniversalProjectileMover.MovementDirection.Custom);
                    mover.SetBaseSpeed(globalSpeed);
                }
            }
        }
    }

    private void SetupFormation()
    {
        // Similar a Synchronized pero mantiene offsets relativos
        SetupSynchronized();
    }

    // ═══════════════════════════════════════════════════
    // UPDATE
    // ═══════════════════════════════════════════════════

    private void Update()
    {
        timeAlive += Time.deltaTime;

        // Destruir después de lifetime
        if (timeAlive >= lifetime)
        {
            DestroyAll();
            return;
        }

        // Movimiento según modo
        switch (mode)
        {
            case ConnectionMode.ParentChild:
                UpdateParentChild();
                break;
            case ConnectionMode.Synchronized:
                UpdateSynchronized();
                break;
            case ConnectionMode.Formation:
                UpdateFormation();
                break;
        }

        // Rotación de formación
        if (rotateFormation)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }

        // Verificar destrucción
        if (destroyWhenOneHits)
        {
            CheckForHits();
        }
    }

    private void UpdateParentChild()
    {
        // Mover el padre (los hijos se mueven automáticamente)
        transform.position += (Vector3)globalDirection.normalized * globalSpeed * Time.deltaTime;

        // Oscilación individual
        if (enableIndividualOscillation)
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                if (projectiles[i] == null) continue;

                float phase = alternateOscillation ? (i % 2 == 0 ? 0 : Mathf.PI) : 0;
                float oscillation = Mathf.Sin(Time.time * oscillationFrequency + phase) * oscillationAmplitude;

                Vector3 localPos = relativeOffsets[i];

                // Aplicar oscilación perpendicular al movimiento
                Vector3 perpendicular = new Vector3(-globalDirection.y, globalDirection.x, 0).normalized;
                localPos += perpendicular * oscillation * Time.deltaTime;

                projectiles[i].transform.localPosition = localPos;
            }
        }
    }

    private void UpdateSynchronized()
    {
        // Los proyectiles se mueven solos con UniversalProjectileMover
        // Solo aplicar oscilación si está activada
        if (enableIndividualOscillation)
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                if (projectiles[i] == null) continue;

                float phase = alternateOscillation ? (i % 2 == 0 ? 0 : Mathf.PI) : 0;
                float oscillation = Mathf.Sin(Time.time * oscillationFrequency + phase) * oscillationAmplitude;

                Vector3 perpendicular = new Vector3(-globalDirection.y, globalDirection.x, 0).normalized;
                projectiles[i].transform.position += perpendicular * oscillation * Time.deltaTime;
            }
        }
    }

    private void UpdateFormation()
    {
        // Mantener formación mientras se mueven
        Vector3 centerPos = transform.position + (Vector3)globalDirection.normalized * globalSpeed * Time.deltaTime;
        transform.position = centerPos;

        for (int i = 0; i < projectiles.Count; i++)
        {
            if (projectiles[i] == null) continue;

            Vector3 targetPos = centerPos + relativeOffsets[i];

            // Oscilación individual
            if (enableIndividualOscillation)
            {
                float phase = alternateOscillation ? (i % 2 == 0 ? 0 : Mathf.PI) : 0;
                float oscillation = Mathf.Sin(Time.time * oscillationFrequency + phase) * oscillationAmplitude;
                Vector3 perpendicular = new Vector3(-globalDirection.y, globalDirection.x, 0).normalized;
                targetPos += perpendicular * oscillation;
            }

            projectiles[i].transform.position = targetPos;
        }
    }

    // ═══════════════════════════════════════════════════
    // UTILIDADES
    // ═══════════════════════════════════════════════════

    private void CheckForHits()
    {
        foreach (var proj in projectiles)
        {
            if (proj == null)
            {
                DestroyAll();
                return;
            }
        }
    }

    private void DestroyAll()
    {
        foreach (var proj in projectiles)
        {
            if (proj != null)
            {
                Destroy(proj);
            }
        }
        Destroy(gameObject);
    }

    // ═══════════════════════════════════════════════════
    // MÉTODOS PÚBLICOS
    // ═══════════════════════════════════════════════════

    /// <summary>
    /// Cambiar dirección en runtime
    /// </summary>
    public void SetGlobalDirection(Vector2 newDirection)
    {
        globalDirection = newDirection.normalized;
    }

    /// <summary>
    /// Cambiar velocidad en runtime
    /// </summary>
    public void SetGlobalSpeed(float newSpeed)
    {
        globalSpeed = newSpeed;
    }

    /// <summary>
    /// Obtener lista de proyectiles
    /// </summary>
    public List<GameObject> GetProjectiles()
    {
        return projectiles;
    }

    // ═══════════════════════════════════════════════════
    // GIZMOS
    // ═══════════════════════════════════════════════════

    private void OnDrawGizmosSelected()
    {
        // Dibujar dirección global
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, globalDirection.normalized * 3f);

        // Dibujar spacing
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + (Vector3)spacing, 0.2f);

        // Dibujar preview del patrón
        if (usePattern)
        {
            Gizmos.color = Color.green;
            DrawPatternPreview();
        }
    }

    private void DrawPatternPreview()
    {
        // Visualización simplificada del patrón
        switch (pattern)
        {
            case PatternPreset.Circle:
                float radius = spacing.magnitude;
                for (int i = 0; i < 12; i++)
                {
                    float angle = (360f / 12f) * i * Mathf.Deg2Rad;
                    Vector3 pos = transform.position + new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius,
                        0
                    );
                    Gizmos.DrawWireSphere(pos, 0.15f);
                }
                break;
        }
    }
}

// ═══════════════════════════════════════════════════════════════════
// EJEMPLOS DE USO
// ═══════════════════════════════════════════════════════════════════

// 1. DOS ESPADAS VERTICALES QUE AVANZAN EN X:
// - Connection Mode: ParentChild
// - Projectile Count: 2
// - Spacing: (0, 2)
// - Global Direction: (1, 0)
// - Global Speed: 5

// 2. CUATRO HUESOS EN CÍRCULO QUE ROTAN:
// - Use Pattern: ✓
// - Pattern Preset: Circle
// - Projectile Count: 4
// - Rotate Formation: ✓
// - Rotation Speed: 45

// 3. LÍNEA DE PROYECTILES CON OSCILACIÓN ALTERNADA:
// - Connection Mode: ParentChild
// - Pattern Preset: HorizontalLine
// - Projectile Count: 5
// - Enable Individual Oscillation: ✓
// - Alternate Oscillation: ✓

// 4. FORMACIÓN EN V:
// - Use Pattern: ✓
// - Pattern Preset: VShape
// - Projectile Count: 6
// - Global Direction: (1, 0)

// 5. GRID QUE AVANZA:
// - Use Pattern: ✓
// - Pattern Preset: Grid
// - Projectile Count: 9
// - Global Direction: (0, -1)
// - Global Speed: 3
