using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 13f;

    [Header("Jumping")]
    [SerializeField] private int maxJumpCount = 2;

    [Header("Dash")]
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.8f;

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

    [Header("Combat")]
    [SerializeField] private PlayerHealth2D playerHealth;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool jumpPressed;
    private bool dashPressed;
    private int jumpsRemaining;

    private bool isDashing;
    private float dashTimer;
    private float nextDashTime;
    private float dashDirection = 1f;
    private float facingDirection = 1f;

    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        jumpsRemaining = maxJumpCount;

        if (playerAnimation == null)
        {
            playerAnimation = GetComponentInChildren<PlayerAnimation2D>();
        }

        if (playerHealth == null)
        {
            playerHealth = GetComponentInParent<PlayerHealth2D>();
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

        if (Input.GetKeyDown(dashKey))
        {
            dashPressed = true;
        }

        if (horizontalInput != 0)
        {
            facingDirection = Mathf.Sign(horizontalInput);

            transform.localScale = new Vector3(
                facingDirection * Mathf.Abs(transform.localScale.x),
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

        if (isDashing)
        {
            HandleDashMovement();
            jumpPressed = false;
            dashPressed = false;
            return;
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

        if (dashPressed && Time.time >= nextDashTime)
        {
            StartDash();
        }

        jumpPressed = false;
        dashPressed = false;
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        nextDashTime = Time.time + dashCooldown;

        dashDirection = horizontalInput != 0 ? Mathf.Sign(horizontalInput) : facingDirection;

        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        if (playerAnimation != null)
        {
            playerAnimation.PlayDash();
        }

        if (playerHealth != null)
        {
            playerHealth.SetInvulnerable(true);
        }
    }

    private void HandleDashMovement()
    {
        dashTimer -= Time.fixedDeltaTime;

        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        if (dashTimer <= 0f)
        {
            isDashing = false;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (playerHealth != null)
            {
                playerHealth.SetInvulnerable(false);
            }
        }
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