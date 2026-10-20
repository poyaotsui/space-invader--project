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
    // offers a buff before every run
    public BuffPicker buffPicker;

    enum State { Waiting, ChoosingBuff, Playing, Over }
    State state;

    // set when the player restarts, so the reloaded scene skips the start screen
    static bool restarting = false;

    void Start()
    {
        endScreen.SetActive(false);
        Freeze();
        if (restarting)
        {
            restarting = false;
            startScreen.SetActive(false);
            ChooseBuff();
            return;
        }
        state = State.Waiting;
        startScreen.SetActive(true);
    }

    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            return;
        }
        if (state == State.Waiting)
        {
            startScreen.SetActive(false);
            ChooseBuff();
        }
        else if (state == State.Over)
        {
            restarting = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // the game stays frozen while the buff cards are up, and starts once one is picked
    void ChooseBuff()
    {
        state = State.ChoosingBuff;
        buffPicker.Show(BeginGame);
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
