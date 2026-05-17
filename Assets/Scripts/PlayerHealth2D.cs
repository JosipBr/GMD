using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth2D : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    [Header("Knockback Feedback")]
    [SerializeField] private float horizontalKnockbackMultiplier = 2f;
    [SerializeField] private float verticalKnockbackForce = 2f;
    [SerializeField] private float knockbackControlLockDuration = 0.22f;

    [Header("Death Animation")]
    [SerializeField] private PlayerAnimation2D playerAnimation;
    [SerializeField] private float damageDeathRoundDelay = 0.6f;

    private int currentHealth;
    private Rigidbody2D rb;
    private bool isDead;
    private bool isInvulnerable;
    private Coroutine deathCoroutine;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;
    public bool IsInvulnerable => isInvulnerable;

    public event Action<PlayerHealth2D> OnDeathStarted;
    public event Action<PlayerHealth2D> OnDied;
    public event Action<PlayerHealth2D> OnDamaged;
    public event Action<int, int> OnHealthChanged;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (playerAnimation == null)
        {
            playerAnimation = GetComponentInChildren<PlayerAnimation2D>();
        }

        ResetHealth();
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection, float knockbackForce)
    {
        if (isDead)
        {
            return;
        }

        if (isInvulnerable)
        {
            Debug.Log($"{gameObject.name} ignored damage because they are invulnerable.");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        ApplyKnockback(knockbackDirection, knockbackForce);

        OnDamaged?.Invoke(this);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die(playDeathAnimation: true);
        }
    }

    public void ResetHealth()
    {
        if (deathCoroutine != null)
        {
            StopCoroutine(deathCoroutine);
            deathCoroutine = null;
        }

        isDead = false;
        isInvulnerable = false;
        currentHealth = maxHealth;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (playerAnimation != null)
        {
            playerAnimation.ResetToIdle();
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Kill()
    {
        Kill(playDeathAnimation: false);
    }

    public void Kill(bool playDeathAnimation)
    {
        if (isDead)
        {
            return;
        }

        currentHealth = 0;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Die(playDeathAnimation);
    }

    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;
    }

    private void ApplyKnockback(Vector2 knockbackDirection, float knockbackForce)
    {
        if (rb == null)
        {
            return;
        }

        float horizontalDirection = Mathf.Sign(knockbackDirection.x);

        if (horizontalDirection == 0f)
        {
            horizontalDirection = transform.localScale.x >= 0 ? -1f : 1f;
        }

        Vector2 force = new Vector2(
            horizontalDirection * knockbackForce * horizontalKnockbackMultiplier,
            verticalKnockbackForce
        );

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.AddForce(force, ForceMode2D.Impulse);

        PlayerMovement2D movement = GetComponent<PlayerMovement2D>();

        if (movement != null)
        {
            movement.LockMovementForKnockback(knockbackControlLockDuration);
        }
    }

    private void Die(bool playDeathAnimation)
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        Debug.Log($"{gameObject.name} died.");

        OnDeathStarted?.Invoke(this);

        AudioManager2D.Instance?.PlayDeath();

        if (playDeathAnimation && playerAnimation != null)
        {
            playerAnimation.PlayDeath();
        }

        if (playDeathAnimation && damageDeathRoundDelay > 0f)
        {
            deathCoroutine = StartCoroutine(NotifyDeathAfterDelay());
            return;
        }

        OnDied?.Invoke(this);
    }

    private IEnumerator NotifyDeathAfterDelay()
    {
        yield return new WaitForSeconds(damageDeathRoundDelay);

        deathCoroutine = null;
        OnDied?.Invoke(this);
    }
}