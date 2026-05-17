using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth2D))]
public class PlayerDamageFeedback2D : MonoBehaviour
{
    [Header("Flash")]
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private int flashCount = 2;

    private PlayerHealth2D playerHealth;
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        originalColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamaged += HandleDamaged;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamaged -= HandleDamaged;
        }

        RestoreOriginalColors();
    }

    private void HandleDamaged(PlayerHealth2D damagedPlayer)
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < flashCount; i++)
        {
            SetColor(damageFlashColor);
            yield return new WaitForSeconds(flashDuration);

            RestoreOriginalColors();
            yield return new WaitForSeconds(flashDuration);
        }

        RestoreOriginalColors();
        flashCoroutine = null;
    }

    private void SetColor(Color color)
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }
    }

    private void RestoreOriginalColors()
    {
        if (spriteRenderers == null || originalColors == null)
        {
            return;
        }

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = originalColors[i];
            }
        }
    }
}