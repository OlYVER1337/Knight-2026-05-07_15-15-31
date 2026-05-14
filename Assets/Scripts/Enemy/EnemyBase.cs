using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth      = 100f;
    public float moveSpeed      = 2f;
    public float attackDamage   = 10f;
    public float attackRange    = 0.8f;
    public float detectionRange = 5f;      // ← đúng tên này

    [Header("Patrol")]
    public float patrolDistance = 3f;
    public float waitTime       = 1.5f;    // ← đúng tên này
    [Header("Patrol Detection")]
    public float wallCheckDistance  = 0.5f;   // khoảng cách check tường
    public float ledgeCheckDistance = 1f;     // khoảng cách check vực
    public LayerMask groundLayer;             // layer Ground
    [Header("Attack")]
    public float attackCooldown = 1.5f;    // ← đúng tên này
    public Transform attackPoint;
    public LayerMask playerLayer;

    public enum EnemyState { Patrol, Chase, Attack, Dead }
    public EnemyState currentState = EnemyState.Patrol;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer sr;
    protected Transform player;

    protected float currentHealth;
    protected bool facingRight = true;
    protected bool isDead      = false;

    protected virtual void Start()
    {
        rb            = GetComponent<Rigidbody2D>();
        anim          = GetComponentInChildren<Animator>();
        sr            = GetComponentInChildren<SpriteRenderer>();
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    protected void FlipToward(float direction)
    {
        if (direction > 0 && !facingRight)      Flip();
        else if (direction < 0 && facingRight)  Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
         sr.flipX = facingRight;  
    }

    protected float DistanceToPlayer()
    {
        if (player == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, player.position);
    }

    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (anim != null)
            anim.SetTrigger("Hurt");

        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        isDead            = true;
        currentState      = EnemyState.Dead;
        rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetTrigger("Death");

        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 2f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            transform.position + Vector3.left  * patrolDistance,
            transform.position + Vector3.right * patrolDistance
        );
        // Wall check
        Gizmos.color = Color.red;
        float dir = facingRight ? 1f : -1f;
        Gizmos.DrawLine(
        transform.position,
        transform.position + Vector3.right * dir * wallCheckDistance);

// Ledge check
        Gizmos.color = Color.cyan;
        Vector3 ledgePos = transform.position + Vector3.right * dir * wallCheckDistance;
        Gizmos.DrawLine(
        ledgePos,
        ledgePos + Vector3.down * ledgeCheckDistance);
    }
}