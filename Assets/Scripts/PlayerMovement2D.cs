using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 13f;

    [Header("Jumping")]
    [SerializeField] private int maxJumpCount = 2;

    [Header("Input")]
    [SerializeField] private KeyCode leftKey = KeyCode.A;
    [SerializeField] private KeyCode rightKey = KeyCode.D;
    [SerializeField] private KeyCode jumpKey = KeyCode.W;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Animation")]
    [SerializeField] private PlayerAnimation2D playerAnimation;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool jumpPressed;
    private int jumpsRemaining;

    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        jumpsRemaining = maxJumpCount;

        if (playerAnimation == null)
        {
            playerAnimation = GetComponentInChildren<PlayerAnimation2D>();
        }
    }

    private void Update()
    {
        horizontalInput = 0f;

        if (Input.GetKey(leftKey))
        {
            horizontalInput -= 1f;
        }

        if (Input.GetKey(rightKey))
        {
            horizontalInput += 1f;
        }

        if (Input.GetKeyDown(jumpKey))
        {
            jumpPressed = true;
        }

        if (horizontalInput != 0)
        {
            transform.localScale = new Vector3(
                Mathf.Sign(horizontalInput) * Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }

    private void FixedUpdate()
    {
        IsGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        if (IsGrounded && rb.linearVelocity.y <= 0.01f)
        {
            jumpsRemaining = maxJumpCount;
        }

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        if (jumpPressed && jumpsRemaining > 0)
        {
            bool isDoubleJump = !IsGrounded;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpsRemaining--;

            if (isDoubleJump && playerAnimation != null)
            {
                playerAnimation.PlayDoubleJump();
            }
        }

        jumpPressed = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}