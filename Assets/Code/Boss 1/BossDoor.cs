using UnityEngine;

public class BossDoor : MonoBehaviour
{
    [Header("Estado Inicial")]
    [SerializeField] private bool empiezaAbierta = true;

    [Header("Collider (Opcional)")]
    [SerializeField] private Collider2D doorCollider;

    private Animator animator;
    private bool estaAbierta;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (doorCollider == null)
        {
            doorCollider = GetComponent<Collider2D>();
        }

        estaAbierta = empiezaAbierta;

        if (doorCollider != null)
        {
            doorCollider.enabled = !estaAbierta;
        }

        if (animator != null)
        {
            if (estaAbierta)
            {
                animator.Play("Door_Open", 0, 1f);
            }
            else
            {
                animator.Play("Door_Closed", 0, 1f);
            }
        }
    }

    public void AbrirPuerta()
    {
        if (estaAbierta)
        {
            return;
        }

        if (animator != null)
        {
            animator.SetTrigger("DoorOpen");
        }
        estaAbierta = true;

        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }
    }

    public void CerrarPuerta()
    {
        if (!estaAbierta)
        {
            return;
        }

        if (animator != null)
        {
            animator.SetTrigger("DoorClosed");
        }

        estaAbierta = false;

        if (doorCollider != null)
        {
            doorCollider.enabled = true;
        }
    }

    public bool EstaAbierta()
    {
        return estaAbierta;
    }

    public void ForzarEstado(bool abrir)
    {
        if (abrir)
            AbrirPuerta();
        else
            CerrarPuerta();
    }
}