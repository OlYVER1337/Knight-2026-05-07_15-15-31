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

    public bool isGrounded;
    private Rigidbody2D rb;
    private float moveInput;
    private SpriteRenderer sr;
    private bool facingRight = true;
    private bool isWallSliding;
    private bool isTouchingWall;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        CheckGround();
        CheckWall();        // ← trước HandleWallSlide
        HandleJump();
        HandleFlip();
        HandleWallSlide();
    }

    void FixedUpdate()
    {
        Move();
        BetterJump();
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

        // ✅ không có dấu ! ở touchLeft
        isTouchingWall = (facingRight && touchRight)
                      || (!facingRight && touchLeft);
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
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
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump")) // ✅ GetButton
        {
            rb.linearVelocity += Vector2.up
                * Physics2D.gravity.y
                * (lowJumpMultiplier - 1)
                * Time.fixedDeltaTime;
        }
    }

    void HandleFlip()
    {
        if (moveInput > 0 && !facingRight) Flip();
        else if (moveInput < 0 && facingRight) Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        sr.flipX = !sr.flipX;
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
}