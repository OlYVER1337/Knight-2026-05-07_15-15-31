using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    public Transform attackPoint;
    public float attackRadius = 0.5f;
    public LayerMask enemyLayer;

    [Header("Combo")]
    public float comboResetTime = 1f;

    private Animator anim;
    private PlayerMovement movement;

    private int comboStep = 0;
    private float lastAttackTime;

    private static readonly int Anim1   = Animator.StringToHash("Attack1");
    private static readonly int Anim2   = Animator.StringToHash("Attack2");
    private static readonly int Anim3   = Animator.StringToHash("Attack3");

    void Start()
    {
        anim     = GetComponentInChildren<Animator>();
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        CheckComboReset();
        HandleAttackInput();
    }

    void CheckComboReset()
    {
        if (Time.time - lastAttackTime > comboResetTime)
            comboStep = 0;
    }

    bool IsAttacking()
    {
        AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
        return state.IsName("Attack1")
            || state.IsName("Attack2")
            || state.IsName("Attack3");
    }

    void HandleAttackInput()
    {

        if (Input.GetMouseButtonDown(0) && !IsAttacking())
        {
            if (movement.isGrounded)
                ComboAttack();
            else
                AirAttack();
        }
    }

    void ComboAttack()
    {
        lastAttackTime = Time.time;
        comboStep++;
        if (comboStep > 3) comboStep = 1;

        switch (comboStep)
        {
            case 1: anim.SetTrigger(Anim1); break;
            case 2: anim.SetTrigger(Anim2); break;
            case 3: anim.SetTrigger(Anim3); break;
        }

        DealDamage();
    }

    void AirAttack()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger(Anim1);
        DealDamage();
    }

    void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            enemyLayer
        );

        foreach (Collider2D hit in hits)
        {
            Debug.Log($"Trúng: {hit.name}");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}