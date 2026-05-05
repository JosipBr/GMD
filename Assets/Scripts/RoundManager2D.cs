using System.Collections;
using TMPro;
using UnityEngine;

public class RoundManager2D : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private PlayerHealth2D player1Health;
    [SerializeField] private PlayerHealth2D player2Health;

    [Header("Spawn Points")]
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text roundMessageText;

    [Header("Round Settings")]
    [SerializeField] private float roundResetDelay = 2f;

    private int player1Score;
    private int player2Score;
    private bool isRoundEnding;

    private void OnEnable()
    {
        player1Health.OnDied += HandlePlayerDied;
        player2Health.OnDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        player1Health.OnDied -= HandlePlayerDied;
        player2Health.OnDied -= HandlePlayerDied;
    }

    private void Start()
    {
        UpdateScoreText();
        ClearRoundMessage();
        ResetRound();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetMatch();
        }
    }

    private void HandlePlayerDied(PlayerHealth2D deadPlayer)
    {
        if (isRoundEnding)
        {
            return;
        }

        isRoundEnding = true;

        if (deadPlayer == player1Health)
        {
            player2Score++;
            ShowRoundMessage("Player 2 wins the round!");
        }
        else if (deadPlayer == player2Health)
        {
            player1Score++;
            ShowRoundMessage("Player 1 wins the round!");
        }

        UpdateScoreText();

        StartCoroutine(ResetRoundAfterDelay());
    }

    private IEnumerator ResetRoundAfterDelay()
    {
        SetPlayersEnabled(false);

        yield return new WaitForSeconds(roundResetDelay);

        ResetRound();
        ClearRoundMessage();

        SetPlayersEnabled(true);

        isRoundEnding = false;
    }

    private void ResetRound()
    {
        ResetPlayerWeapon(player1Health);
        ResetPlayerWeapon(player2Health);

        ResetPlayer(player1Health, player1SpawnPoint);
        ResetPlayer(player2Health, player2SpawnPoint);

        player1Health.ResetHealth();
        player2Health.ResetHealth();

        Debug.Log("Round reset.");
    }

    private void ResetMatch()
    {
        player1Score = 0;
        player2Score = 0;

        UpdateScoreText();
        ClearRoundMessage();
        ResetRound();

        isRoundEnding = false;

        SetPlayersEnabled(true);

        Debug.Log("Match reset.");
    }

    private void ResetPlayer(PlayerHealth2D playerHealth, Transform spawnPoint)
    {
        Transform playerTransform = playerHealth.transform;
        Rigidbody2D rb = playerHealth.GetComponent<Rigidbody2D>();

        playerTransform.position = spawnPoint.position;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void SetPlayersEnabled(bool enabled)
    {
        SetPlayerEnabled(player1Health, enabled);
        SetPlayerEnabled(player2Health, enabled);
    }

    private void SetPlayerEnabled(PlayerHealth2D playerHealth, bool enabled)
    {
        PlayerMovement2D movement = playerHealth.GetComponent<PlayerMovement2D>();
        PlayerMeleeAttack2D attack = playerHealth.GetComponent<PlayerMeleeAttack2D>();

        if (movement != null)
        {
            movement.enabled = enabled;
        }

        if (attack != null)
        {
            attack.enabled = enabled;
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"{player1Score} - {player2Score}";
        }
    }

    private void ShowRoundMessage(string message)
    {
        if (roundMessageText != null)
        {
            roundMessageText.text = message;
        }
    }

    private void ClearRoundMessage()
    {
        if (roundMessageText != null)
        {
            roundMessageText.text = "";
        }
    }

    private void ResetPlayerWeapon(PlayerHealth2D playerHealth)
    {
        PlayerMeleeAttack2D attack = playerHealth.GetComponent<PlayerMeleeAttack2D>();

        if (attack != null)
        {
            attack.ResetWeapon();
        }
    }
}