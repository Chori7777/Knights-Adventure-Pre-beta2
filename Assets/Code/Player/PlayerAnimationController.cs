using UnityEngine;


public class PlayerAnimationController : MonoBehaviour
{
    private Animator anim;
    private PlayerMovement player;

    private bool isDoubleJumping;
    private float doubleJumpAnimTime = 0.6f;
    private bool frozen;

    private bool hasMovement;
    private bool hasJump;
    private bool hasFalling;
    private bool hasGrounded;
    private bool hasDash;
    private bool hasWallCling;
    private bool hasIsAttacking;
    private bool hasIsBlocking;
    private bool hasSpeedY;
    private bool hasComboIndex;
    private bool hasTriggerDoubleJump;
    private bool hasTriggerThrow;
    private bool hasTriggerDeath;


    public void Initialize(PlayerMovement playerMovement)
    {
        anim = GetComponent<Animator>();
        player = playerMovement;
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            var ps = anim.parameters;
            for (int i = 0; i < ps.Length; i++)
            {
                var p = ps[i];
                var name = p.name;
                var type = p.type;
                if (type == AnimatorControllerParameterType.Float)
                {
                    if (name == "Movement") hasMovement = true;
                    else if (name == "SpeedY") hasSpeedY = true;
                }
                else if (type == AnimatorControllerParameterType.Bool)
                {
                    if (name == "Jump") hasJump = true;
                    else if (name == "Falling") hasFalling = true;
                    else if (name == "Grounded") hasGrounded = true;
                    else if (name == "Dash") hasDash = true;
                    else if (name == "WallCling") hasWallCling = true;
                    else if (name == "isAttacking") hasIsAttacking = true;
                    else if (name == "isBlocking") hasIsBlocking = true;
                }
                else if (type == AnimatorControllerParameterType.Int)
                {
                    if (name == "ComboIndex") hasComboIndex = true;
                }
                else if (type == AnimatorControllerParameterType.Trigger)
                {
                    if (name == "DoubleJump") hasTriggerDoubleJump = true;
                    else if (name == "Throw") hasTriggerThrow = true;
                    else if (name == "Death") hasTriggerDeath = true;
                }
            }
        }
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
        if (hasMovement) anim.SetFloat("Movement", moveAmount);
    }

    private void UpdateJumpAnimation()
    {
        if (player.IsGrounded)
        {
            if (hasJump) anim.SetBool("Jump", false);
            isDoubleJumping = false;
        }
        else if (player.VerticalVelocity > 0.1f && !isDoubleJumping)
        {
            if (hasJump) anim.SetBool("Jump", true);
        }
    }

    private void UpdateFallingAnimation()
    {
        bool isFalling = !player.IsGrounded && player.VerticalVelocity < -0.1f && !isDoubleJumping;
        if (hasFalling) anim.SetBool("Falling", isFalling);
    }

    private void UpdateGroundedAnimation()
    {
        if (!player.IsAttacking)
        {
            if (hasGrounded) anim.SetBool("Grounded", player.IsGrounded);
        }
    }

    private void UpdateDashAnimation() { if (hasDash) anim.SetBool("Dash", player.IsDashing); }
    private void UpdateWallClingAnimation() { if (hasWallCling) anim.SetBool("WallCling", !player.IsGrounded && player.IsTouchingWall); }
    private void UpdateAttackAnimation() { if (hasIsAttacking) anim.SetBool("isAttacking", player.IsAttacking); }
    private void UpdateBlockAnimation() { if (hasIsBlocking) anim.SetBool("isBlocking", player.IsBlocking); }
    private void UpdateSpeedYAnimation() { if (hasSpeedY) anim.SetFloat("SpeedY", player.VerticalVelocity); }

    public void TriggerDoubleJump()
    {
        if (anim == null) return;
        if (hasTriggerDoubleJump) anim.SetTrigger("DoubleJump");
        isDoubleJumping = true;
        Invoke(nameof(ResetDoubleJump), doubleJumpAnimTime);
    }

    private void ResetDoubleJump() => isDoubleJumping = false;
    public void TriggerThrow() { if (anim == null) return; if (hasTriggerThrow) anim.SetTrigger("Throw"); }
    public void TriggerDamage() { if (anim == null) return; anim.SetBool("damage", true); }
    public void StopDamage() { if (anim == null) return; anim.SetBool("damage", false); }

 
    public void TriggerDeath()
    {
        if (anim == null) return;
        if (hasTriggerDoubleJump) anim.ResetTrigger("DoubleJump");
        if (hasTriggerThrow) anim.ResetTrigger("Throw");
        anim.SetBool("damage", false);
        if (hasTriggerDeath) anim.SetTrigger("Death");
    }

    public void SetComboIndex(int index)
    {
        if (anim == null) return;
        if (hasComboIndex) anim.SetInteger("ComboIndex", index);
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

    public void SetFrozen(bool value)
    {
        frozen = value;
    }
}
