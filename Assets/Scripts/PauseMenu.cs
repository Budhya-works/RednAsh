using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pausePanel;          // The panel to show/hide
    public Button resumeButton;
    public Button restartButton;
    public Button musicToggleButton;
    public Button mainMenuButton;

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu"; // Name of your main menu scene

    private bool isPaused = false;
    private AudioSource backgroundMusic;

    void Start()
    {
        // Ensure panel starts hidden
        pausePanel.SetActive(false);

        // Hook up button listeners
        resumeButton.onClick.AddListener(ResumeGame);
        restartButton.onClick.AddListener(RestartLevel);
        musicToggleButton.onClick.AddListener(ToggleMusic);
        mainMenuButton.onClick.AddListener(LoadMainMenu);

        // Find background music (if you used the BackgroundMusic script)
        GameObject musicObj = GameObject.FindWithTag("Music");
        if (musicObj != null)
        {
            backgroundMusic = musicObj.GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        // Toggle pause when ESC is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    // === Pause System ===
    public void PauseGame()
    {
        pausePanel.SetActive(true);    // Show menu
        Time.timeScale = 0f;           // Freeze game
        isPaused = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);   // Hide menu
        Time.timeScale = 1f;           // Resume game
        isPaused = false;
    }

    // === Restart Current Level ===
    public void RestartLevel()
    {
        Time.timeScale = 1f; // Reset time before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // === Toggle Music ===
    public void ToggleMusic()
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.mute = !backgroundMusic.mute;
        }
    }

    // === Go Back to Main Menu ===
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Ensure normal time when loading menu
        SceneManager.LoadScene(mainMenuSceneName);
    }
}

