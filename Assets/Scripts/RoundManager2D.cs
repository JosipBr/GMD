using System;
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
    [SerializeField] private float matchEndDelay = 3f;
    [SerializeField] private float matchStartDelay = 0.5f;
    [SerializeField] private float readyMessageDuration = 1.2f;
    [SerializeField] private float fightMessageDuration = 0.6f;

    [Header("Weapons")]
    [SerializeField] private WeaponSpawner2D weaponSpawner;

    private int player1Score;
    private int player2Score;
    private int matchWinTarget = 3;

    private bool isRoundEnding;
    private bool matchStarted;
    private Arena2D currentArena;
    private Coroutine roundRoutine;

    public event Action OnMatchEnded;

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

        player1Score = 0;
        player2Score = 0;

        UpdateScoreText();
        ClearRoundMessage();

        if (weaponSpawner != null)
        {
            weaponSpawner.StopSpawning();
            weaponSpawner.ClearSpawnedWeapons();
        }

        PreparePlayersForMenu();
        SetPlayersEnabled(false);

        matchStarted = false;
        isRoundEnding = false;
    }

    private void Update()
    {
        if (!matchStarted)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetMatch();
        }
    }

    public void SetMatchWinTarget(int newMatchWinTarget)
    {
        matchWinTarget = Mathf.Max(0, newMatchWinTarget);

        Debug.Log(matchWinTarget <= 0
            ? "Match mode set to endless."
            : $"Match mode set to first to {matchWinTarget}.");
    }

    public void StartMatchFromMenu()
    {
        StopCurrentRoundRoutine();

        matchStarted = true;
        isRoundEnding = false;

        player1Score = 0;
        player2Score = 0;

        UpdateScoreText();
        ClearRoundMessage();

        if (weaponSpawner != null)
        {
            weaponSpawner.StopSpawning();
            weaponSpawner.ClearSpawnedWeapons();
        }

        if (arenaManager != null)
        {
            currentArena = arenaManager.LoadFirstArena();
        }

        roundRoutine = StartCoroutine(StartMatchRoutine());
    }

    public void StopMatchForMenu()
    {
        StopCurrentRoundRoutine();

        matchStarted = false;
        isRoundEnding = false;

        if (weaponSpawner != null)
        {
            weaponSpawner.StopSpawning();
            weaponSpawner.ClearSpawnedWeapons();
        }

        ClearRoundMessage();
        PreparePlayersForMenu();
        SetPlayersEnabled(false);
    }

    private IEnumerator StartMatchRoutine()
    {
        SetPlayersEnabled(false);

        ResetRound();

        if (matchStartDelay > 0f)
        {
            yield return new WaitForSeconds(matchStartDelay);
        }

        yield return StartRoundIntroOnly();

        roundRoutine = null;
    }

    private void HandlePlayerDied(PlayerHealth2D deadPlayer)
    {
        if (!matchStarted || isRoundEnding)
        {
            return;
        }

        isRoundEnding = true;

        if (weaponSpawner != null)
        {
            weaponSpawner.StopSpawning();
        }

        AudioManager2D.Instance?.PlayRoundWin();

        PlayerHealth2D roundWinner = null;

        if (deadPlayer == player1Health)
        {
            player2Score++;
            roundWinner = player2Health;
        }
        else if (deadPlayer == player2Health)
        {
            player1Score++;
            roundWinner = player1Health;
        }

        UpdateScoreText();

        bool matchWon = HasPlayerWonMatch(roundWinner);

        StopCurrentRoundRoutine();

        if (matchWon)
        {
            if (roundWinner == player1Health)
            {
                ShowRoundMessage("PLAYER 1 WINS THE MATCH!");
            }
            else if (roundWinner == player2Health)
            {
                ShowRoundMessage("PLAYER 2 WINS THE MATCH!");
            }

            roundRoutine = StartCoroutine(EndMatchAfterDelay());
        }
        else
        {
            if (deadPlayer == player1Health)
            {
                ShowRoundMessage("Player 2 wins the round!");
            }
            else if (deadPlayer == player2Health)
            {
                ShowRoundMessage("Player 1 wins the round!");
            }

            roundRoutine = StartCoroutine(ResetRoundAfterDelay());
        }
    }

    private bool HasPlayerWonMatch(PlayerHealth2D playerHealth)
    {
        if (matchWinTarget <= 0 || playerHealth == null)
        {
            return false;
        }

        if (playerHealth == player1Health)
        {
            return player1Score >= matchWinTarget;
        }

        if (playerHealth == player2Health)
        {
            return player2Score >= matchWinTarget;
        }

        return false;
    }

    private IEnumerator ResetRoundAfterDelay()
    {
        SetPlayersEnabled(false);

        yield return new WaitForSeconds(roundResetDelay);

        yield return StartRoundWithIntro(loadNextArena: true);

        roundRoutine = null;
    }

    private IEnumerator EndMatchAfterDelay()
    {
        SetPlayersEnabled(false);

        if (weaponSpawner != null)
        {
            weaponSpawner.StopSpawning();
        }

        yield return new WaitForSeconds(matchEndDelay);

        matchStarted = false;
        isRoundEnding = false;
        roundRoutine = null;

        OnMatchEnded?.Invoke();
    }

    private IEnumerator StartRoundWithIntro(bool loadNextArena)
    {
        SetPlayersEnabled(false);

        if (loadNextArena && arenaManager != null)
        {
            currentArena = arenaManager.LoadNextArena();
        }

        ResetRound();

        yield return StartRoundIntroOnly();
    }

    private IEnumerator StartRoundIntroOnly()
    {
        AudioManager2D.Instance?.PlayReadyFight();

        ShowRoundMessage("READY...");
        yield return new WaitForSeconds(readyMessageDuration);

        ShowRoundMessage("FIGHT!");
        yield return new WaitForSeconds(fightMessageDuration);

        ClearRoundMessage();
        SetPlayersEnabled(true);

        if (weaponSpawner != null)
        {
            weaponSpawner.StartSpawningForRound();
        }

        isRoundEnding = false;
    }

    private void ResetRound()
    {
        if (currentArena == null)
        {
            Debug.LogWarning("No current arena assigned.");
            return;
        }

        if (weaponSpawner != null)
        {
            weaponSpawner.StopSpawning();
        }

        ResetPlayerWeapon(player1Health);
        ResetPlayerWeapon(player2Health);

        if (weaponSpawner != null)
        {
            weaponSpawner.ClearSpawnedWeapons();
            weaponSpawner.SetSpawnPoints(currentArena.WeaponSpawnPoints);
        }

        ResetPlayer(player1Health, currentArena.Player1SpawnPoint);
        ResetPlayer(player2Health, currentArena.Player2SpawnPoint);

        player1Health.ResetHealth();
        player2Health.ResetHealth();

        Debug.Log("Round reset.");
    }

    private void ResetMatch()
    {
        StartMatchFromMenu();

        Debug.Log("Match reset.");
    }

    private void PreparePlayersForMenu()
    {
        if (currentArena == null)
        {
            return;
        }

        ResetPlayerWeapon(player1Health);
        ResetPlayerWeapon(player2Health);

        if (weaponSpawner != null)
        {
            weaponSpawner.ClearSpawnedWeapons();
        }

        ResetPlayer(player1Health, currentArena.Player1SpawnPoint);
        ResetPlayer(player2Health, currentArena.Player2SpawnPoint);

        player1Health.ResetHealth();
        player2Health.ResetHealth();
    }

    private void ResetPlayer(PlayerHealth2D playerHealth, Transform spawnPoint)
    {
        if (playerHealth == null || spawnPoint == null)
        {
            return;
        }

        Transform playerTransform = playerHealth.transform;
        Rigidbody2D rb = playerHealth.GetComponent<Rigidbody2D>();

        playerTransform.position = spawnPoint.position;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void SetPlayersEnabled(bool enabled)
    {
        SetPlayerEnabled(player1Health, enabled);
        SetPlayerEnabled(player2Health, enabled);
    }

    private void SetPlayerEnabled(PlayerHealth2D playerHealth, bool enabled)
    {
        if (playerHealth == null)
        {
            return;
        }

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
        if (playerHealth == null)
        {
            return;
        }

        PlayerMeleeAttack2D attack = playerHealth.GetComponent<PlayerMeleeAttack2D>();

        if (attack != null)
        {
            attack.ResetWeapon();
        }
    }

    private void StopCurrentRoundRoutine()
    {
        if (roundRoutine != null)
        {
            StopCoroutine(roundRoutine);
            roundRoutine = null;
        }
    }
}