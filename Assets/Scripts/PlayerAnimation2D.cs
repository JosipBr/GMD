using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimation2D : MonoBehaviour
{
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private PlayerMovement2D playerMovement;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponentInParent<Rigidbody2D>();
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponentInParent<PlayerMovement2D>();
        }
    }

    private void Update()
    {
        if (playerRigidbody == null || playerMovement == null)
        {
            return;
        }

        animator.SetFloat("Speed", Mathf.Abs(playerRigidbody.linearVelocity.x));
        animator.SetBool("IsGrounded", playerMovement.IsGrounded);
    }

    public void PlayAttack()
    {
        animator.SetTrigger("Attack");
    }

    public void PlayDoubleJump()
    {
        animator.SetTrigger("DoubleJump");
    }

    public void PlayDash()
    {
        animator.SetTrigger("Dash");
    }

    public void SetWallSliding(bool isWallSliding)
    {
        animator.SetBool("IsWallSliding", isWallSliding);
    }

    public void PlayShoot()
    {
        animator.SetTrigger("Shoot");
    }

    public void PlayLedgeClimb()
    {
        animator.SetTrigger("LedgeClimb");
    }

    public void PlayDeath()
    {
        animator.SetTrigger("Death");
    }

    public void ResetToIdle()
    {
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("DoubleJump");
        animator.ResetTrigger("Dash");
        animator.ResetTrigger("Shoot");
        animator.ResetTrigger("Death");

        animator.SetBool("IsWallSliding", false);
        animator.SetBool("IsGrounded", true);
        animator.SetFloat("Speed", 0f);

        animator.Play("Idle", 0, 0f);
    }
}