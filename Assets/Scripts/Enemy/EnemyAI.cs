using System.Data;
using System.Diagnostics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Timeline;


public class EnemyAI : EnemyBase
{
   private Vector2 patrolOrigin;
   private float patrolTimer;
   private float attackTimer;
   private bool isWaiting;

    protected override void Start()
    {
        base.Start();
        patrolOrigin = transform.position;


    }
    void Update()
    {
        if(isDead) return;
        UpdateState();

        switch (currentState)
        {
            case EnemyState.Patrol:HandlePatrol(); break;
            case EnemyState.Chase:HandleChase(); break;
            case EnemyState.Attack:HandleAttack(); break;
        }
        if(attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
    }
    void UpdateState()
    {
    float dist = DistanceToPlayer();

    if(dist <= attackRange)
        {
            currentState = EnemyState.Attack;
        }else if (dist <= detectionrage)
        {
            currentState = EnemyState.Chase;
        }
        else
        {
            currentState = EnemyState.Patrol;
        }
    }
    void HandlePatrol()
    {
        if(anim!= null)
        {
            anim.SetBool("Walk",true);
        }
        if (isWaiting)
        {
            rb.linearVelocity =Vector2.zero;
            patrolTimer -= Time.deltaTime;

            if (patrolTimer <= 0)
            {
                isWaiting = false;
                return;
            }
            float dir = facingRight ? 1f :-1f;
            float target = patrolOrigin.x + (facingRight ?patrolDistance : -patrolDistance);


            if((facingRight && transform.position.x >= target) || (!facingRight && transform.position.x <= target))
            {
                isWaiting =true;
                patrolTimer = wailTime;
                FlipToward(-dir);
                rb.linearVelocity = Vector2.zero;
                return;
            }
            rb.linearVelocity = new Vector2(dir*moveSpeed, rb.linearVelocity.y);
        }
        
    }

    void HandleChase()
    {
        if (anim != null)
        {
            anim.SetBool("Walk",true);
        }
        if(player == null)
        {
            return;
        }
        float dir = player.position.x -transform.position.x;

        FlipToward(dir);

        rb.linearVelocity = new Vector2(
            Mathf.Sign(dir) * moveSpeed * 1.5f,
            rb.linearVelocity.y
        );
    }

    void HandleAttack()
    {
        rb.linearVelocity = Vector2.zero;

        if(player != null)
        FlipToward(player.position.x - transform.position.x);

        if(anim !=null)
        anim.SetBool("Walk",false);

        if(attackTimer <= 0)
        {
            PerformAttack();
            attackTimer = attackCoolDown;
        }
    }
    void PerformAttack()
    {
        if(anim!= null)
        {
            anim.SetTrigger("Attach");

        }
        if (player != null && Vector2.Distance(transform.position,player.position) <= attackRange)
        {
            // PlayerTakeDamage() ti viet
        }
    }
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        if (!isDead)
        currentState = EnemyState.Chase;
    }

}