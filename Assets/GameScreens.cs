using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class GameScreens : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject startScreen;
    public GameObject endScreen;
    public TextMeshProUGUI endTitle;
    public TextMeshProUGUI endDetail;
    public Spaceship spaceship;

    enum State { Waiting, Playing, Over }
    State state;

    // set when the player restarts, so the reloaded scene skips the start screen
    static bool restarting = false;

    void Start()
    {
        endScreen.SetActive(false);
        if (restarting)
        {
            restarting = false;
            BeginGame();
            return;
        }
        state = State.Waiting;
        startScreen.SetActive(true);
        Freeze();
    }

    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            return;
        }
        if (state == State.Waiting)
        {
            BeginGame();
        }
        else if (state == State.Over)
        {
            restarting = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void BeginGame()
    {
        state = State.Playing;
        startScreen.SetActive(false);
        Time.timeScale = 1;
        spaceship.enabled = true;
        // stops the space press that started the game from also firing a dart
        spaceship.lastShot = Time.time;
    }

    public void EndGame(bool won, string detail)
    {
        if (state == State.Over)
        {
            return;
        }
        state = State.Over;
        endTitle.text = won ? "You Win!" : "Game Over";
        endDetail.text = detail;
        endScreen.SetActive(true);
        Freeze();
    }

    // stops aliens, bullets and darts (they move in FixedUpdate) and the ship's controls
    void Freeze()
    {
        Time.timeScale = 0;
        if (spaceship != null)
        {
            spaceship.enabled = false;
        }
    }
}
