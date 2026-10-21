using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// the settings button in the top-right corner: pauses the game and opens a menu
// with Resume and How to Play. Clicking the button again (or pressing Esc) resumes.
public class PauseMenu : MonoBehaviour
{
    [Header("Inscribed")]
    public GameScreens screens;
    public Button settingsButton;
    // the dimmed full-screen layer holding both panels
    public GameObject menuScreen;
    public GameObject pausePanel;
    public GameObject howToPlayPanel;
    public Button resumeButton;
    public Button howToPlayButton;
    public Button backButton;

    [Header("Dynamic")]
    public bool open = false;

    // what the game was doing before the menu opened, so closing it puts everything back
    float savedTimeScale;
    bool savedShipEnabled;

    void Awake()
    {
        menuScreen.SetActive(false);
        settingsButton.onClick.AddListener(Toggle);
        resumeButton.onClick.AddListener(Close);
        howToPlayButton.onClick.AddListener(ShowHowToPlay);
        backButton.onClick.AddListener(ShowPause);
    }

    void Update()
    {
        // the button is only there while pausing makes sense (not on the buff cards or end screen)
        settingsButton.gameObject.SetActive(open || screens.CanPause);
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        if (open)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    void Open()
    {
        if (!screens.CanPause)
        {
            return;
        }
        open = true;
        screens.paused = true;
        savedTimeScale = Time.timeScale;
        savedShipEnabled = screens.spaceship.enabled;
        Time.timeScale = 0;
        screens.spaceship.enabled = false;
        menuScreen.SetActive(true);
        ShowPause();
    }

    void Close()
    {
        if (!open)
        {
            return;
        }
        open = false;
        screens.paused = false;
        menuScreen.SetActive(false);
        Time.timeScale = savedTimeScale;
        if (screens.spaceship != null)
        {
            screens.spaceship.enabled = savedShipEnabled;
        }
    }

    void ShowPause()
    {
        pausePanel.SetActive(true);
        howToPlayPanel.SetActive(false);
    }

    void ShowHowToPlay()
    {
        pausePanel.SetActive(false);
        howToPlayPanel.SetActive(true);
    }
}
