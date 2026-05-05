using UnityEngine;

public class PlayerMeleeAttack2D : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private KeyCode attackKey = KeyCode.F;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Transform weaponHoldPoint;
    [SerializeField] private float baseAttackRange = 0.6f;
    [SerializeField] private int baseDamage = 20;
    [SerializeField] private float baseKnockbackForce = 8f;
    [SerializeField] private float baseAttackCooldown = 0.4f;
    [SerializeField] private PlayerAnimation2D playerAnimation;

    [Header("Collision")]
    [SerializeField] private LayerMask playerLayer;

    private float nextAttackTime;
    private WeaponPickup2D equippedWeapon;

    private float CurrentAttackRange => equippedWeapon != null ? equippedWeapon.AttackRange : baseAttackRange;
    private int CurrentDamage => equippedWeapon != null ? equippedWeapon.Damage : baseDamage;
    private float CurrentKnockbackForce => equippedWeapon != null ? equippedWeapon.KnockbackForce : baseKnockbackForce;
    private float CurrentAttackCooldown => equippedWeapon != null ? equippedWeapon.AttackCooldown : baseAttackCooldown;

    private void Update()
    {
        if (Input.GetKeyDown(attackKey) && Time.time >= nextAttackTime)
        {
            if (playerAnimation != null)
            {
                playerAnimation.PlayAttack();
            }

            Attack();
            nextAttackTime = Time.time + CurrentAttackCooldown;
        }
    }

public void EquipWeapon(WeaponPickup2D weapon)
{
    if (weapon == null || weaponHoldPoint == null)
    {
        return;
    }

    if (equippedWeapon != null)
    {
        return;
    }

    equippedWeapon = weapon;
    equippedWeapon.AttachTo(weaponHoldPoint);

    Debug.Log($"{gameObject.name} picked up {weapon.gameObject.name}");
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
            CurrentAttackRange,
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
            health.TakeDamage(CurrentDamage, knockbackDirection, CurrentKnockbackForce);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(attackPoint.position, CurrentAttackRange);
    }

    public void ResetWeapon()
    {
        if (equippedWeapon == null)
        {
            return;
        }

        equippedWeapon.ResetPickup();
        equippedWeapon = null;
    }
}