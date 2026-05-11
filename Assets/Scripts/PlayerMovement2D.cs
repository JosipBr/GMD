using UnityEngine;
using System.Collections;

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

    [Header("Wall Slide")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.18f, 1.1f);
    [SerializeField] private float wallSlideSpeed = 2.5f;

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

    [Header("Ledge Climb")]
    [SerializeField] private Transform ledgeCheck;
    [SerializeField] private Vector2 ledgeCheckSize = new Vector2(0.2f, 0.2f);
    [SerializeField] private float ledgeRayStartHeight = 0.8f;
    [SerializeField] private float ledgeRayForwardOffset = 0.35f;
    [SerializeField] private float ledgeRayDistance = 2f;
    [SerializeField] private float ledgeStandHorizontalOffset = 0.35f;
    [SerializeField] private float ledgeClimbDuration = 0.35f;
    [SerializeField] private float minLedgeStepHeight = 0.15f;
    [SerializeField] private float maxLedgeStepHeight = 1.0f;

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

    private bool isClimbingLedge;
    private float originalGravityScale;

    private bool hasCheckedGrounded;
    private bool wasGroundedLastFixedUpdate;

    public bool IsGrounded { get; private set; }
    public bool IsWallSliding { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalGravityScale = rb.gravityScale;
        jumpsRemaining = maxJumpCount;

        if (playerAnimation == null)
        {
            playerAnimation = GetComponentInChildren<PlayerAnimation2D>();
        }

        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth2D>();
        }
    }

    private void OnDisable()
    {
        isDashing = false;
        IsWallSliding = false;
        hasCheckedGrounded = false;
        wasGroundedLastFixedUpdate = false;

        if (playerHealth != null)
        {
            playerHealth.SetInvulnerable(false);
        }

        if (playerAnimation != null)
        {
            playerAnimation.SetWallSliding(false);
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

        HandleLandingSound();

        if (isClimbingLedge)
        {
            rb.linearVelocity = Vector2.zero;
            jumpPressed = false;
            dashPressed = false;
            return;
        }

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

        UpdateWallSlide();

        if (TryStartLedgeClimb())
        {
            jumpPressed = false;
            dashPressed = false;
            return;
        }

        if (IsWallSliding)
        {
            rb.linearVelocity = new Vector2(0f, -wallSlideSpeed);
        }
        else
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }

        if (jumpPressed && jumpsRemaining > 0)
        {
            bool isDoubleJump = !IsGrounded;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpsRemaining--;

            AudioManager2D.Instance?.PlayJump();

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

    private void HandleLandingSound()
    {
        bool playerIsAlive = playerHealth == null || !playerHealth.IsDead;

        bool justLanded =
            hasCheckedGrounded &&
            !wasGroundedLastFixedUpdate &&
            IsGrounded &&
            rb.linearVelocity.y <= 0.1f &&
            !isClimbingLedge &&
            playerIsAlive;

        if (justLanded)
        {
            AudioManager2D.Instance?.PlayLanding();
        }

        wasGroundedLastFixedUpdate = IsGrounded;
        hasCheckedGrounded = true;
    }

    private void UpdateWallSlide()
    {
        bool isTouchingWall = false;

        if (wallCheck != null)
        {
            isTouchingWall = Physics2D.OverlapBox(
                wallCheck.position,
                wallCheckSize,
                0f,
                groundLayer
            );
        }

        bool isPushingTowardWall = horizontalInput != 0f;

        IsWallSliding =
            isTouchingWall &&
            !IsGrounded &&
            isPushingTowardWall &&
            !isDashing;

        if (playerAnimation != null)
        {
            playerAnimation.SetWallSliding(IsWallSliding);
        }
    }

    private void StartDash()
    {
        isDashing = true;
        IsWallSliding = false;

        AudioManager2D.Instance?.PlayDash();

        if (playerAnimation != null)
        {
            playerAnimation.SetWallSliding(false);
        }

        dashTimer = dashDuration;
        nextDashTime = Time.time + dashCooldown;

        dashDirection = horizontalInput != 0 ? Mathf.Sign(horizontalInput) : facingDirection;

        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        if (playerHealth != null)
        {
            playerHealth.SetInvulnerable(true);
        }

        if (playerAnimation != null)
        {
            playerAnimation.PlayDash();
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
        if (groundCheck != null)
        {
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (wallCheck != null)
        {
            Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
        }

        if (ledgeCheck != null)
        {
            Gizmos.DrawWireCube(ledgeCheck.position, ledgeCheckSize);

            Vector2 rayOrigin =
                (Vector2)ledgeCheck.position +
                Vector2.up * ledgeRayStartHeight +
                Vector2.right * facingDirection * ledgeRayForwardOffset;

            Gizmos.DrawLine(
                rayOrigin,
                rayOrigin + Vector2.down * ledgeRayDistance
            );
        }
    }

    private bool TryStartLedgeClimb()
    {
        if (isClimbingLedge || isDashing || IsGrounded)
        {
            return false;
        }

        if (wallCheck == null || ledgeCheck == null)
        {
            return false;
        }

        bool isTouchingWall = Physics2D.OverlapBox(
            wallCheck.position,
            wallCheckSize,
            0f,
            groundLayer
        );

        bool isLedgeBlocked = Physics2D.OverlapBox(
            ledgeCheck.position,
            ledgeCheckSize,
            0f,
            groundLayer
        );

        bool isPushingTowardWall = horizontalInput != 0f;

        if (!isTouchingWall || isLedgeBlocked || !isPushingTowardWall)
        {
            return false;
        }

        if (!TryGetLedgeStandPosition(out Vector3 standPosition))
        {
            return false;
        }

        StartCoroutine(ClimbLedge(standPosition));
        return true;
    }

    private bool TryGetLedgeStandPosition(out Vector3 standPosition)
    {
        Vector2 rayOrigin =
            (Vector2)ledgeCheck.position +
            Vector2.up * ledgeRayStartHeight +
            Vector2.right * facingDirection * ledgeRayForwardOffset;

        RaycastHit2D hit = Physics2D.Raycast(
            rayOrigin,
            Vector2.down,
            ledgeRayDistance,
            groundLayer
        );

        if (hit.collider == null)
        {
            standPosition = transform.position;
            return false;
        }

        float playerFeetY = groundCheck.position.y;
        float stepHeight = hit.point.y - playerFeetY;

        if (stepHeight < minLedgeStepHeight || stepHeight > maxLedgeStepHeight)
        {
            standPosition = transform.position;
            return false;
        }

        float groundOffset = transform.position.y - groundCheck.position.y;

        standPosition = new Vector3(
            hit.point.x + facingDirection * ledgeStandHorizontalOffset,
            hit.point.y + groundOffset,
            transform.position.z
        );

        return true;
    }

    private IEnumerator ClimbLedge(Vector3 standPosition)
    {
        isClimbingLedge = true;
        IsWallSliding = false;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        if (playerAnimation != null)
        {
            playerAnimation.SetWallSliding(false);
            playerAnimation.PlayLedgeClimb();
        }

        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < ledgeClimbDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / ledgeClimbDuration;
            transform.position = Vector3.Lerp(startPosition, standPosition, t);

            rb.linearVelocity = Vector2.zero;

            yield return null;
        }

        transform.position = standPosition;
        rb.gravityScale = originalGravityScale;
        rb.linearVelocity = Vector2.zero;

        jumpsRemaining = maxJumpCount;
        isClimbingLedge = false;
    }
}