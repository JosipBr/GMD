using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth2D : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

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

    public event Action<PlayerHealth2D> OnDied;
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

        rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);

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

    private void Die(bool playDeathAnimation)
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        Debug.Log($"{gameObject.name} died.");

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

    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;
    }
}