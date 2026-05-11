using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 12f;
    public Transform groundSensor;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Better Jump")]
    public float fallMultiplier = 5f;
    public float lowJumpMultiplier = 2f;

    [Header("Wall Slide")]
    public Transform wallSensorRight;
    public Transform wallSensorLeft;
    public float wallCheckDistance = 0.2f;
    public float wallSlideSpeed = 1.5f;

    [Header("Wall Jump")]
    public Vector2 wallJumpForce = new Vector2(8f, 14f);

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;

    public bool isGrounded;
    private Rigidbody2D rb;
    private float moveInput;
    private SpriteRenderer sr;
    private Animator anim;                 // ← thêm
    private bool facingRight = true;
    private bool isWallSliding;
    private bool isTouchingWall;
    private bool isWallJumping;
    private bool isDashing;
    private float dashTimeLeft;
    private float dashCooldownTimer;

    // ── Animation Hashes ─────────────────────────────────
    private static readonly int AnimGrounded  = Animator.StringToHash("Grounded");
    private static readonly int AnimWallSlide = Animator.StringToHash("WallSlide");
    private static readonly int AnimAirSpeedY = Animator.StringToHash("AirSpeedY");
    private static readonly int AnimJump      = Animator.StringToHash("Jump");
    private static readonly int AnimRoll      = Animator.StringToHash("Roll");
    private static readonly int AnimState     = Animator.StringToHash("AnimState");

    void Start()
    {
        rb   = GetComponent<Rigidbody2D>();
        sr   = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();  // ← thêm
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (isDashing) return;

        CheckGround();
        CheckWall();
        HandleInput();
        HandleJump();
        HandleFlip();
        HandleWallSlide();
        UpdateAnimations();                // ← thêm
    }

    void FixedUpdate()
    {
        HandleDash();

        if (isDashing) return;

        Move();
        BetterJump();
    }

    // ── Animation ────────────────────────────────────────
    void UpdateAnimations()
    {
        // Idle / Run
        anim.SetInteger(AnimState, moveInput != 0 && isGrounded ? 1 : 0);

        // Grounded
        anim.SetBool(AnimGrounded, isGrounded);

        // Wall Slide
        anim.SetBool(AnimWallSlide, isWallSliding);

        // AirSpeedY để phân biệt Jump / Fall
        anim.SetFloat(AnimAirSpeedY, rb.linearVelocity.y);
    }

    void Move()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundSensor.position,
            groundCheckRadius,
            groundLayer
        );
    }

    void CheckWall()
    {
        bool touchRight = Physics2D.Raycast(
            wallSensorRight.position,
            Vector2.right,
            wallCheckDistance,
            groundLayer
        );
        bool touchLeft = Physics2D.Raycast(
            wallSensorLeft.position,
            Vector2.left,
            wallCheckDistance,
            groundLayer
        );

        isTouchingWall = (facingRight && touchRight)
                      || (!facingRight && touchLeft);
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)
            && dashCooldownTimer <= 0
            && !isDashing)
        {
            StartDash();
        }
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                anim.SetTrigger(AnimJump);     // ← thêm
            }
            else if (isWallSliding)
            {
                WallJump();
            }
        }
    }

    void WallJump()
    {
        float direction = facingRight ? -1f : 1f;
        rb.linearVelocity = new Vector2(
            wallJumpForce.x * direction,
            wallJumpForce.y
        );
        anim.SetTrigger(AnimJump);             // ← thêm
        Flip();
        isWallJumping = true;
        Invoke(nameof(StopWallJumping), 0.2f);
    }

    void StopWallJumping()
    {
        isWallJumping = false;
    }

    void BetterJump()
    {
        if (isWallSliding) return;

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up
                * Physics2D.gravity.y
                * (fallMultiplier - 1)
                * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector2.up
                * Physics2D.gravity.y
                * (lowJumpMultiplier - 1)
                * Time.fixedDeltaTime;
        }
    }

    void HandleFlip()
    {
        if (isWallJumping) return;
        if (moveInput > 0 && !facingRight) Flip();
        else if (moveInput < 0 && facingRight) Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        sr.flipX    = !sr.flipX;
    }

    void HandleWallSlide()
    {
        isWallSliding = isTouchingWall
                     && !isGrounded
                     && rb.linearVelocity.y < 0
                     && moveInput != 0;

        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed)
            );
        }
    }

    void StartDash()
    {
        isDashing         = true;
        dashTimeLeft      = dashDuration;
        dashCooldownTimer = dashCooldown;
        rb.gravityScale   = 0;
        anim.SetTrigger(AnimRoll);             // ← thêm
    }

    void HandleDash()
    {
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        if (!isDashing) return;

        dashTimeLeft -= Time.deltaTime;

        if (dashTimeLeft > 0)
        {
            float direction = facingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * dashSpeed, 0);
        }
        else
        {
            StopDash();
        }
    }

    void StopDash()
    {
        isDashing         = false;
        rb.gravityScale   = 1;
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    void OnDrawGizmosSelected()
    {
        if (groundSensor != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundSensor.position, groundCheckRadius);
        }
        if (wallSensorRight != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallSensorRight.position,
                wallSensorRight.position + Vector3.right * wallCheckDistance);
        }
        if (wallSensorLeft != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallSensorLeft.position,
                wallSensorLeft.position + Vector3.left * wallCheckDistance);
        }
    }
    public void AE_SlideDust()
{
    // Để trống — chỉ cần có function này để Unity không báo lỗi
    // Sau này có thể thêm particle effect bụi ở đây
}
}