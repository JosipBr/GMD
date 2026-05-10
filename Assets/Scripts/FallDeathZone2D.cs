using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FallDeathZone2D : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth2D playerHealth = other.GetComponentInParent<PlayerHealth2D>();

        if (playerHealth == null)
        {
            return;
        }

        playerHealth.Kill(playDeathAnimation: false);
    }
}