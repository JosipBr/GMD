using UnityEngine;

public enum WeaponType2D
{
    Melee,
    Gun
}

[RequireComponent(typeof(Collider2D))]
public class WeaponPickup2D : MonoBehaviour
{
    [Header("Weapon Type")]
    [SerializeField] private WeaponType2D weaponType = WeaponType2D.Melee;

    [Header("Weapon Stats")]
    [SerializeField] private int damage = 35;
    [SerializeField] private float attackRange = 1.1f;
    [SerializeField] private float knockbackForce = 12f;
    [SerializeField] private float attackCooldown = 0.55f;

    [Header("Gun Settings")]
    [SerializeField] private Projectile2D projectilePrefab;
    [SerializeField] private float projectileSpeed = 12f;

    [Header("Equipped Visual Transform")]
    [SerializeField] private Vector3 equippedLocalPosition = new Vector3(0.35f, 0.05f, 0f);
    [SerializeField] private Vector3 equippedLocalRotation = new Vector3(0f, 0f, -35f);
    [SerializeField] private Vector3 equippedLocalScale = new Vector3(0.8f, 0.08f, 1f);

    private Collider2D weaponCollider;
    private Rigidbody2D rb;

    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    public bool IsEquipped { get; private set; }

    public WeaponType2D WeaponType => weaponType;
    public int Damage => damage;
    public float AttackRange => attackRange;
    public float KnockbackForce => knockbackForce;
    public float AttackCooldown => attackCooldown;
    public Projectile2D ProjectilePrefab => projectilePrefab;
    public float ProjectileSpeed => projectileSpeed;

    private void Awake()
    {
        weaponCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsEquipped)
        {
            return;
        }

        PlayerMeleeAttack2D playerAttack = other.GetComponentInParent<PlayerMeleeAttack2D>();

        if (playerAttack == null)
        {
            return;
        }

        playerAttack.EquipWeapon(this);
    }

    public void AttachTo(Transform weaponHoldPoint)
    {
        IsEquipped = true;

        transform.SetParent(weaponHoldPoint);
        transform.localPosition = equippedLocalPosition;
        transform.localEulerAngles = equippedLocalRotation;
        transform.localScale = equippedLocalScale;

        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }

        if (rb != null)
        {
            rb.simulated = false;
        }
    }

    public void ResetPickup()
    {
        IsEquipped = false;

        transform.SetParent(originalParent);
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;

        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
        }

        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public void SetOriginalTransform(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        originalParent = null;
        originalPosition = position;
        originalRotation = rotation;
        originalScale = scale;
    }
}