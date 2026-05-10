using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile2D : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private LayerMask playerLayer;

    private Transform owner;
    private int damage;
    private float knockbackForce;
    private Vector2 direction;
    private float speed;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = direction.normalized * speed;
    }

    public void Initialize(
        Transform owner,
        Vector2 direction,
        float speed,
        int damage,
        float knockbackForce,
        LayerMask playerLayer
    )
    {
        this.owner = owner;
        this.direction = direction.normalized;
        this.speed = speed;
        this.damage = damage;
        this.knockbackForce = knockbackForce;
        this.playerLayer = playerLayer;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) == 0)
        {
            return;
        }

        if (owner != null && other.transform.root == owner.root)
        {
            return;
        }

        PlayerHealth2D health = other.GetComponentInParent<PlayerHealth2D>();

        if (health == null)
        {
            return;
        }

        if (health.IsInvulnerable)
        {
            Debug.Log($"{health.gameObject.name} dodged the bullet.");
            return;
        }

        Vector2 knockbackDirection = other.transform.position - transform.position;
        health.TakeDamage(damage, knockbackDirection, knockbackForce);

        Destroy(gameObject);
    }
}