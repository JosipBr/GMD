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
}