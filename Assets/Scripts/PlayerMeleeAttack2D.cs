using UnityEngine;

public class PlayerMeleeAttack2D : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private KeyCode attackKey = KeyCode.F;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.6f;
    [SerializeField] private int damage = 20;
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float attackCooldown = 0.4f;
    [SerializeField] private PlayerAnimation2D playerAnimation;

    [Header("Collision")]
    [SerializeField] private LayerMask playerLayer;

    private float nextAttackTime;

    private void Update()
    {
        if (Input.GetKeyDown(attackKey) && Time.time >= nextAttackTime)
        {
            if (playerAnimation != null)
            {
                playerAnimation.PlayAttack();
            }

            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void Attack()
    {
        if (attackPoint == null)
        {
            Debug.LogWarning($"{gameObject.name} is missing an AttackPoint reference.");
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            playerLayer
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.transform.root == transform.root)
            {
                continue;
            }

            PlayerHealth2D health = hit.GetComponentInParent<PlayerHealth2D>();

            if (health == null)
            {
                continue;
            }

            Vector2 knockbackDirection = hit.transform.position - transform.position;
            health.TakeDamage(damage, knockbackDirection, knockbackForce);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}