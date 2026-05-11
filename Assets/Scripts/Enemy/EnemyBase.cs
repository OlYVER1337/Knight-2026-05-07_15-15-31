using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Satats")]
    public float maxHealth = 100f;
    public float moveSpeed = 2f;
    public float attackDame = 10f;
    public float attackRange = 0.8f;
    public float detectionrage = 5f;

    [Header("Patrol")]
    public float patrolDistance = 3f;
    public float wailTime = 1.5f;


    [Header("Attack")]
    public float attackCoolDown = 1.5f;
    public Transform attackPoint;
    public LayerMask playerLayer;

    public enum EnemyState{Patrol, Chase,Attack, Dead}
    public EnemyState currentState = EnemyState.Patrol;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer sr;
    protected Transform player;


    protected float currentHealth;
    protected bool facingRight = true;
    protected bool isDead = false;


    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        currentHealth = maxHealth;

        GameObject  playerObj = GameObject.FindGameObjectWithTag("Player");
        if(playerObj == null)
        {
            player = gameObject.transform;
        }
    }

    protected void FlipToward (float direcetion)
    {
        if(direcetion > 0 && !facingRight) Flip();
        else if (direcetion < 0 && facingRight) Flip();
    }
       void Flip()
    {
        facingRight = !facingRight;
        sr.flipX    = !sr.flipX;
    }
 
    protected virtual void Die()
    {
        isDead = true;
        currentState = EnemyState.Dead;
        rb.linearVelocity =  Vector2.zero;

        if (anim!=null) anim.SetTrigger("Death");

        GetComponent<Collider2D>().enabled = false;

        Destroy(gameObject,5f);

    }
    protected float DistanceToPlayer()
    {
        if(player == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, player.position);
    }

   public virtual void TakeDamage(float damage)
    {
        if(isDead) return;
        currentHealth = -damage;

        if(anim != null)
        {
            anim.SetTrigger("Hurt");
        }
        if(currentHealth <= 0)
        Die();

    }



}