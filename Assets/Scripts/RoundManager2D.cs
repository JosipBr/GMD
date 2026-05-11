using System.Collections;
using TMPro;
using UnityEngine;

public class RoundManager2D : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private PlayerHealth2D player1Health;
    [SerializeField] private PlayerHealth2D player2Health;

    [Header("Arena")]
    [SerializeField] private ArenaManager2D arenaManager;

    [Header("Spawn Points")]
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text roundMessageText;
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private GameObject roundMessagePanel;

    [Header("Round Settings")]
    [SerializeField] private float roundResetDelay = 2f;
    [SerializeField] private float readyMessageDuration = 1.2f;
    [SerializeField] private float fightMessageDuration = 0.6f;

    [Header("Weapons")]
    [SerializeField] private WeaponSpawner2D weaponSpawner;

    private int player1Score;
    private int player2Score;
    private bool isRoundEnding;
    private Arena2D currentArena;
    private Coroutine roundRoutine;

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
        if (arenaManager != null)
        {
            currentArena = arenaManager.LoadFirstArena();
        }

        UpdateScoreText();
        ClearRoundMessage();

        roundRoutine = StartCoroutine(StartRoundWithIntro(loadNextArena: false));
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

        if (weaponSpawner != null)
        {
            weaponSpawner.StopSpawning();
        }

        AudioManager2D.Instance?.PlayRoundWin();

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

        roundRoutine = StartCoroutine(ResetRoundAfterDelay());
    }

    private IEnumerator ResetRoundAfterDelay()
    {
        SetPlayersEnabled(false);

        yield return new WaitForSeconds(roundResetDelay);

        roundRoutine = StartCoroutine(StartRoundWithIntro(loadNextArena: true));
    }

    private IEnumerator StartRoundWithIntro(bool loadNextArena)
    {
        SetPlayersEnabled(false);

        if (loadNextArena && arenaManager != null)
        {
            currentArena = arenaManager.LoadNextArena();
        }

        ResetRound();

        AudioManager2D.Instance?.PlayReadyFight();

        ShowRoundMessage("READY...");
        yield return new WaitForSeconds(readyMessageDuration);

        ShowRoundMessage("FIGHT!");
        yield return new WaitForSeconds(fightMessageDuration);

        ClearRoundMessage();
        SetPlayersEnabled(true);

        isRoundEnding = false;
        roundRoutine = null;
    }

    private void ResetRound()
    {
        if (currentArena == null)
        {
            Debug.LogWarning("No current arena assigned.");
            return;
        }

        ResetPlayerWeapon(player1Health);
        ResetPlayerWeapon(player2Health);

        if (weaponSpawner != null)
        {
            weaponSpawner.SetSpawnPoints(currentArena.WeaponSpawnPoints);
            weaponSpawner.StartSpawningForRound();
        }

        ResetPlayer(player1Health, currentArena.Player1SpawnPoint);
        ResetPlayer(player2Health, currentArena.Player2SpawnPoint);

        player1Health.ResetHealth();
        player2Health.ResetHealth();

        Debug.Log("Round reset.");
    }

    private void ResetMatch()
    {
        if (roundRoutine != null)
        {
            StopCoroutine(roundRoutine);
            roundRoutine = null;
        }

        player1Score = 0;
        player2Score = 0;
        isRoundEnding = false;

        if (arenaManager != null)
        {
            currentArena = arenaManager.LoadFirstArena();
        }

        UpdateScoreText();
        ClearRoundMessage();

        roundRoutine = StartCoroutine(StartRoundWithIntro(loadNextArena: false));

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
        if (roundMessagePanel != null)
        {
            roundMessagePanel.SetActive(true);
        }

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

        if (roundMessagePanel != null)
        {
            roundMessagePanel.SetActive(false);
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