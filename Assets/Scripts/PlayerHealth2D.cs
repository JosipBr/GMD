using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth2D : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;
    private Rigidbody2D rb;
    private bool isDead;
    private bool isInvulnerable;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;
    public bool IsInvulnerable => isInvulnerable;

    public event Action<PlayerHealth2D> OnDied;
    public event Action<int, int> OnHealthChanged;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
            Die();
        }
    }

    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Kill()
    {
        if (isDead)
        {
            return;
        }

        currentHealth = 0;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Die();
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        Debug.Log($"{gameObject.name} died.");

        OnDied?.Invoke(this);
    }

    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;
    }
}