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

    // the settings menu can open on the title screen and during play, but not over the buff cards or end screen
    public bool CanPause { get { return state == State.Waiting || state == State.Playing; } }
    // set while the settings menu is open, so Space can't start or restart the game behind it
    [HideInInspector] public bool paused = false;

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
        if (paused || Keyboard.current == null || !Keyboard.current.spaceKey.wasPressedThisFrame)
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

    // a buff pick in the middle of a run (after a boss): the game freezes behind the cards,
    // and next runs once one is picked and play resumes
    public void ChooseBuffThen(System.Action next)
    {
        state = State.ChoosingBuff;
        Freeze();
        buffPicker.Show(() =>
        {
            BeginGame();
            next();
        });
    }

    void BeginGame()
    {
        state = State.Playing;
        startScreen.SetActive(false);
        Time.timeScale = 1;
        spaceship.enabled = true;
        // the first automatic throw comes one fire interval after the game starts
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
