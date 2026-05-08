
using System;
using UnityEngine;

// A behaviour that is attached to a playable
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    [Header("Movement")]
    public float jumpForce = 12f;
    public Transform groundSensor;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;
    public bool isGrounded;



    private Rigidbody2D rb;
    private float moveInput ;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        CheckGround();
        HandleJump();
    }

    void FixedUpdate()
    {
        Move();
    }
    void Move()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed , rb.linearVelocity.y);
    }
    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundSensor.position,
            groundCheckRadius,
            groundLayer
        );
    }
    void HandleJump()
    {
        if(Input.GetButtonDown("Jump")&& isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

    }
      void OnDrawGizmosSelected()
    {
        if (groundSensor == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundSensor.position, groundCheckRadius);
    }

}
