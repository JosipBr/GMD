using UnityEngine;

public class PlayerMeleeAttack2D : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private KeyCode attackKey = KeyCode.F;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Transform weaponHoldPoint;
    [SerializeField] private Transform weaponAttackPoint;
    [SerializeField] private float baseAttackRange = 0.6f;
    [SerializeField] private int baseDamage = 20;
    [SerializeField] private float baseKnockbackForce = 8f;
    [SerializeField] private float baseAttackCooldown = 0.4f;
    [SerializeField] private PlayerAnimation2D playerAnimation;

    [Header("Collision")]
    [SerializeField] private LayerMask playerLayer;

    private float nextAttackTime;
    private WeaponPickup2D equippedWeapon;
    private WeaponUseAnimation2D equippedWeaponUseAnimation;

    private bool HasGun => equippedWeapon != null && equippedWeapon.WeaponType == WeaponType2D.Gun;

    private float CurrentAttackRange => equippedWeapon != null ? equippedWeapon.AttackRange : baseAttackRange;
    private int CurrentDamage => equippedWeapon != null ? equippedWeapon.Damage : baseDamage;
    private float CurrentKnockbackForce => equippedWeapon != null ? equippedWeapon.KnockbackForce : baseKnockbackForce;
    private float CurrentAttackCooldown => equippedWeapon != null ? equippedWeapon.AttackCooldown : baseAttackCooldown;

    private void Update()
    {
        if (Input.GetKeyDown(attackKey) && Time.time >= nextAttackTime)
        {
            PlayEquippedWeaponUseAnimation();

            if (HasGun)
            {
                if (playerAnimation != null)
                {
                    playerAnimation.PlayShoot();
                }

                Shoot();
            }
            else
            {
                if (playerAnimation != null)
                {
                    playerAnimation.PlayAttack();
                }

                MeleeAttack();
            }

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
        equippedWeapon.AttachTo(transform, weaponHoldPoint);

        equippedWeaponUseAnimation = equippedWeapon.GetComponentInChildren<WeaponUseAnimation2D>();

        Debug.Log($"{gameObject.name} picked up {weapon.gameObject.name}");
    }

    public void ResetWeapon()
    {
        if (equippedWeapon == null)
        {
            return;
        }

        equippedWeapon.ResetPickup();
        equippedWeapon = null;
        equippedWeaponUseAnimation = null;
    }

    private void PlayEquippedWeaponUseAnimation()
    {
        if (equippedWeaponUseAnimation == null)
        {
            return;
        }

        if (HasGun)
        {
            equippedWeaponUseAnimation.PlayUseAnimation();
        }
        else
        {
            equippedWeaponUseAnimation.PlayUseAnimation(weaponAttackPoint);
        }
    }

    private void MeleeAttack()
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

    private void Shoot()
    {
        if (equippedWeapon.ProjectilePrefab == null)
        {
            Debug.LogWarning($"{equippedWeapon.gameObject.name} is missing a projectile prefab.");
            return;
        }

        Vector2 shootDirection = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;

        Vector3 spawnPosition = equippedWeapon.MuzzlePoint != null
            ? equippedWeapon.MuzzlePoint.position
            : weaponHoldPoint.position;

        Projectile2D projectile = Instantiate(
            equippedWeapon.ProjectilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        projectile.Initialize(
            transform,
            shootDirection,
            equippedWeapon.ProjectileSpeed,
            equippedWeapon.Damage,
            equippedWeapon.KnockbackForce,
            playerLayer
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(attackPoint.position, CurrentAttackRange);
    }
}