using UnityEngine;


public class PlayerAnimationController : MonoBehaviour
{
    private Animator anim;
    private PlayerMovement player;

    private bool isDoubleJumping;
    private float doubleJumpAnimTime = 0.6f;


    public void Initialize(PlayerMovement playerMovement)
    {
        anim = GetComponent<Animator>();
        player = playerMovement;
    }

    private void LateUpdate()
    {
        if (anim == null || player == null) return;
        UpdateAllAnimations();
    }

    private void UpdateAllAnimations()
    {
        UpdateMovementAnimation();
        UpdateJumpAnimation();
        UpdateFallingAnimation();
        UpdateGroundedAnimation();
        UpdateDashAnimation();
        UpdateWallClingAnimation();
        UpdateAttackAnimation();
        UpdateBlockAnimation();
        UpdateSpeedYAnimation();
    }


    private void UpdateMovementAnimation()
    {
        float moveAmount = Mathf.Abs(player.HorizontalInput);
        anim.SetFloat("Movement", moveAmount);
    }

    private void UpdateJumpAnimation()
    {
        if (player.IsGrounded)
        {
            anim.SetBool("Jump", false);
            isDoubleJumping = false;
        }
        else if (player.VerticalVelocity > 0.1f && !isDoubleJumping)
        {
            anim.SetBool("Jump", true);
        }
    }

    private void UpdateFallingAnimation()
    {
        bool isFalling = !player.IsGrounded && player.VerticalVelocity < -0.1f && !isDoubleJumping;
        anim.SetBool("Falling", isFalling);
    }

    private void UpdateGroundedAnimation()
    {
        if (!player.IsAttacking)
            anim.SetBool("Grounded", player.IsGrounded);
    }

    private void UpdateDashAnimation() => anim.SetBool("Dash", player.IsDashing);
    private void UpdateWallClingAnimation() => anim.SetBool("WallCling", !player.IsGrounded && player.IsTouchingWall);
    private void UpdateAttackAnimation() => anim.SetBool("isAttacking", player.IsAttacking);
    private void UpdateBlockAnimation() => anim.SetBool("isBlocking", player.IsBlocking);
    private void UpdateSpeedYAnimation() => anim.SetFloat("SpeedY", player.VerticalVelocity);

    public void TriggerDoubleJump()
    {
        if (anim == null) return;
        anim.SetTrigger("DoubleJump");
        isDoubleJumping = true;
        Invoke(nameof(ResetDoubleJump), doubleJumpAnimTime);
    }

    private void ResetDoubleJump() => isDoubleJumping = false;
    public void TriggerThrow() { if (anim == null) return; anim.SetTrigger("Throw"); }
    public void TriggerDamage() { if (anim == null) return; anim.SetBool("damage", true); }
    public void StopDamage() { if (anim == null) return; anim.SetBool("damage", false); }

 
    public void TriggerDeath()
    {
        if (anim == null) return;
        anim.ResetTrigger("DoubleJump");
        anim.ResetTrigger("Throw");
        anim.SetBool("damage", false);
        anim.SetTrigger("Death");
    }

    public void SetComboIndex(int index)
    {
        if (anim == null) return;
        anim.SetInteger("ComboIndex", index);
    }


    public float GetAnimationLength(string stateName)
    {
        if (anim == null || anim.runtimeAnimatorController == null)
            return 1f;

        foreach (var clip in anim.runtimeAnimatorController.animationClips)
        {
            if (clip.name == stateName)
                return clip.length;
        }

        return 1f;
    }


    public void OnDeathAnimationEvent()
    {
        var life = GetComponent<playerLife>();
        if (life != null)
        {
            life.OnDeathAnimationEnd();
        }
    }
}
