using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameUIManager2D : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject gameplayHUD;

    [Header("Victory")]
    [SerializeField] private TMP_Text victoryTitleText;

    [Header("Settings")]
    [SerializeField] private TMP_Text matchLengthText;
    [SerializeField] private int[] matchWinTargets = { 0, 1, 3, 5, 10 };
    [SerializeField] private int defaultMatchWinTarget = 3;

    [Header("First Selected Buttons")]
    [SerializeField] private GameObject mainMenuFirstButton;
    [SerializeField] private GameObject settingsFirstButton;
    [SerializeField] private GameObject pauseFirstButton;
    [SerializeField] private GameObject victoryFirstButton;

    [Header("Game")]
    [SerializeField] private RoundManager2D roundManager;

    private bool isGameStarted;
    private bool isPaused;
    private int matchWinTargetIndex;

    private void OnEnable()
    {
        if (roundManager != null)
        {
            roundManager.OnMatchEnded += HandleMatchEnded;
        }
    }

    private void OnDisable()
    {
        if (roundManager != null)
        {
            roundManager.OnMatchEnded -= HandleMatchEnded;
        }
    }

    private void Start()
    {
        SetupDefaultMatchLength();
        ShowMainMenu();

        AudioManager2D.Instance?.PlayPressStart();
    }

    private void Update()
    {
        if (!isGameStarted)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || WasGamepadPausePressed())
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void StartGame()
    {
        AudioManager2D.Instance?.PlayMenuSelect();

        isGameStarted = true;
        isPaused = false;

        ApplyMatchLengthToRoundManager();

        SetPanel(mainMenuPanel, false);
        SetPanel(settingsPanel, false);
        SetPanel(pausePanel, false);
        SetPanel(victoryPanel, false);
        SetPanel(gameplayHUD, true);

        Time.timeScale = 1f;

        if (roundManager != null)
        {
            roundManager.StartMatchFromMenu();
        }

        ClearSelectedButton();
    }

    public void ShowMainMenu()
    {
        isGameStarted = false;
        isPaused = false;

        Time.timeScale = 1f;

        if (roundManager != null)
        {
            roundManager.StopMatchForMenu();
        }

        SetPanel(mainMenuPanel, true);
        SetPanel(settingsPanel, false);
        SetPanel(pausePanel, false);
        SetPanel(victoryPanel, false);
        SetPanel(gameplayHUD, false);

        SelectButton(mainMenuFirstButton);
    }

    public void ShowSettings()
    {
        AudioManager2D.Instance?.PlayMenuSelect();

        SetPanel(mainMenuPanel, false);
        SetPanel(settingsPanel, true);
        SetPanel(pausePanel, false);
        SetPanel(victoryPanel, false);
        SetPanel(gameplayHUD, false);

        UpdateMatchLengthText();
        SelectButton(settingsFirstButton);
    }

    public void BackToMainMenu()
    {
        AudioManager2D.Instance?.PlayMenuBack();

        SetPanel(mainMenuPanel, true);
        SetPanel(settingsPanel, false);
        SetPanel(pausePanel, false);
        SetPanel(victoryPanel, false);

        SelectButton(mainMenuFirstButton);
    }

    public void PreviousMatchLength()
    {
        ChangeMatchLength(-1);
    }

    public void NextMatchLength()
    {
        ChangeMatchLength(1);
    }

    public void PauseGame()
    {
        if (!isGameStarted)
        {
            return;
        }

        AudioManager2D.Instance?.PlayPause();

        isPaused = true;
        Time.timeScale = 0f;

        SetPanel(pausePanel, true);
        SelectButton(pauseFirstButton);
    }

    public void ResumeGame()
    {
        AudioManager2D.Instance?.PlayPause();

        isPaused = false;
        Time.timeScale = 1f;

        SetPanel(pausePanel, false);
        ClearSelectedButton();
    }

    public void RestartMatch()
    {
        AudioManager2D.Instance?.PlayMenuSelect();

        isPaused = false;
        Time.timeScale = 1f;

        SetPanel(pausePanel, false);
        SetPanel(victoryPanel, false);
        SetPanel(gameplayHUD, true);

        ApplyMatchLengthToRoundManager();

        if (roundManager != null)
        {
            roundManager.StartMatchFromMenu();
        }

        ClearSelectedButton();
    }

    public void QuitGame()
    {
        AudioManager2D.Instance?.PlayMenuSelect();

        Debug.Log("Quit game.");

        Application.Quit();
    }

    private void HandleMatchEnded(int winningPlayerNumber)
    {
        isGameStarted = false;
        isPaused = false;

        Time.timeScale = 1f;

        if (victoryTitleText != null)
        {
            victoryTitleText.text = $"PLAYER {winningPlayerNumber} WINS!";
        }

        SetPanel(mainMenuPanel, false);
        SetPanel(settingsPanel, false);
        SetPanel(pausePanel, false);
        SetPanel(victoryPanel, true);
        SetPanel(gameplayHUD, false);

        SelectButton(victoryFirstButton);
    }

    private bool WasGamepadPausePressed()
    {
        foreach (Gamepad gamepad in Gamepad.all)
        {
            if (
                gamepad.startButton.wasPressedThisFrame ||
                gamepad.selectButton.wasPressedThisFrame ||
                gamepad.buttonNorth.wasPressedThisFrame
            )
            {
                return true;
            }
        }

        return false;
    }

    private void SetupDefaultMatchLength()
    {
        if (matchWinTargets == null || matchWinTargets.Length == 0)
        {
            matchWinTargets = new int[] { 0, 1, 3, 5, 10 };
        }

        matchWinTargetIndex = 0;

        for (int i = 0; i < matchWinTargets.Length; i++)
        {
            if (matchWinTargets[i] == defaultMatchWinTarget)
            {
                matchWinTargetIndex = i;
                break;
            }
        }

        ApplyMatchLengthToRoundManager();
        UpdateMatchLengthText();
    }

    private void ChangeMatchLength(int direction)
    {
        if (matchWinTargets == null || matchWinTargets.Length == 0)
        {
            return;
        }

        AudioManager2D.Instance?.PlayMenuSelect();

        matchWinTargetIndex += direction;

        if (matchWinTargetIndex < 0)
        {
            matchWinTargetIndex = matchWinTargets.Length - 1;
        }
        else if (matchWinTargetIndex >= matchWinTargets.Length)
        {
            matchWinTargetIndex = 0;
        }

        ApplyMatchLengthToRoundManager();
        UpdateMatchLengthText();
    }

    private void ApplyMatchLengthToRoundManager()
    {
        if (roundManager != null)
        {
            roundManager.SetMatchWinTarget(GetCurrentMatchWinTarget());
        }
    }

    private int GetCurrentMatchWinTarget()
    {
        if (matchWinTargets == null || matchWinTargets.Length == 0)
        {
            return 0;
        }

        return matchWinTargets[matchWinTargetIndex];
    }

    private void UpdateMatchLengthText()
    {
        if (matchLengthText != null)
        {
            matchLengthText.text = GetMatchLengthDisplay(GetCurrentMatchWinTarget());
        }
        else
        {
            Debug.LogWarning("Match Length Text is not assigned on GameUIManager2D.");
        }
    }

    private string GetMatchLengthDisplay(int matchWinTarget)
    {
        if (matchWinTarget <= 0)
        {
            return "ENDLESS";
        }

        return $"FIRST TO {matchWinTarget}";
    }

    private void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    private void SelectButton(GameObject button)
    {
        if (EventSystem.current == null || button == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button);
    }

    private void ClearSelectedButton()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}