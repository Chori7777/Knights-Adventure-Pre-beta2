using UnityEngine;

public class bossCore : MonoBehaviour
{
    [Header("Componentes principales")]
    public Rigidbody2D rb;
    public Animator anim;
    public SpriteRenderer spriteRenderer;
    public Transform player;

    [Header("Módulos del Jefe")]
    public BossAnimationController animController;
    public BossLife life;
    public BossMovement movement;
    // Cada jefe puede tener sus propios scripts de ataque específicos

    [Header("Estado general")]
    public bool IsDead = false;
    public bool IsAttacking = false;
    public bool IsTakingDamage = false;
    public bool IsVulnerable = true;
    public bool CanMove = true;

    [Header("Fase actual")]
    public int CurrentPhase = 1;

    public Vector2 PlayerPosition => player != null ? (Vector2)player.position : Vector2.zero;
    [SerializeField] private bool enableAutoFlip = true;

    private void Awake()
    {
        // ✅ Cachear componentes principales
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ✅ Cachear módulos del jefe
        animController = GetComponent<BossAnimationController>();
        life = GetComponent<BossLife>();
        movement = GetComponent<BossMovement>();


        // ✅ Buscar jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] No se encontró jugador con tag 'Player'");
        }
    }

    private void Start()
    {
        // ✅ Inicializar módulos
        InitializeModules();
    }

    private void InitializeModules()
    {
        if (animController != null)
        {
            animController.Initialize(this);
            Debug.Log($"[bossCore] BossAnimationController inicializado");
        }


        if (movement != null)
        {
            movement.Initialize(this);
            Debug.Log($"[bossCore] BossMovement inicializado");
        }

        // ✅ Nota: Los scripts de ataque específicos de cada jefe
        // deben inicializarse en sus propias clases
    }

    // ========== MÉTODOS DE DISTANCIA Y DIRECCIÓN ==========

    public float DistanceToPlayer()
    {
        return player == null ? Mathf.Infinity : Vector2.Distance(transform.position, player.position);
    }

    public Vector2 DirectionToPlayer()
    {
        return player == null ? Vector2.zero : (player.position - transform.position).normalized;
    }

    public void FacePlayer()
    {
        if (!enableAutoFlip) return;
        if (player == null) return;

        transform.localScale = player.position.x < transform.position.x
            ? new Vector3(-1, 1, 1)
            : new Vector3(1, 1, 1);
    }

    // ========== CONTROL DE MOVIMIENTO ==========

    public void SetCanMove(bool state)
    {
        CanMove = state;
    }

    public void StopMovement()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    // ========== CONTROL DE ESTADOS ==========

    public void SetDead(bool state)
    {
        IsDead = state;

        if (state)
        {
            IsAttacking = false;
            IsTakingDamage = false;
            CanMove = false;
            Debug.Log($"[bossCore] Jefe marcado como muerto");
        }
    }

    public void SetAttacking(bool state)
    {
        IsAttacking = state;

        if (state)
        {
            CanMove = false; // No moverse durante ataques
        }
    }

    public void SetTakingDamage(bool state)
    {
        IsTakingDamage = state;

        if (state)
        {
            IsAttacking = false; // Cancelar ataque si recibe daño
        }
    }

    public void SetVulnerable(bool state)
    {
        IsVulnerable = state;
        Debug.Log($"[bossCore] Vulnerable: {state}");
    }

    // ========== SISTEMA DE FASES ==========

    public void ChangePhase(int newPhase)
    {
        if (IsDead) return;

        CurrentPhase = newPhase;
        Debug.Log($"[bossCore] Cambiando a fase {newPhase}");

        // Actualizar animación de fase
        if (animController != null)
        {
            animController.SetPhase(newPhase);
        }

        // Aquí puedes añadir lógica específica por fase
        OnPhaseChanged(newPhase);
    }

    protected virtual void OnPhaseChanged(int phase)
    {
        // Override este método en clases hijas para comportamiento específico por fase
        Debug.Log($"[bossCore] Fase {phase} activada");
    }

    // ========== VERIFICACIONES DE ESTADO ==========

    public bool CanPerformAction()
    {
        return !IsDead && !IsAttacking && !IsTakingDamage;
    }

    public bool CanTakeDamage()
    {
        return !IsDead && IsVulnerable;
    }

    public void SetAutoFlip(bool enabled)
    {
        enableAutoFlip = enabled;
    }

    // ========== GIZMOS ==========

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        // Línea al jugador
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, player.position);

        // Distancia al jugador (opcional)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, DistanceToPlayer());
    }
}
