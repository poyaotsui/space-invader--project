using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// everything that can differ from one wave to the next
[System.Serializable]
public class WaveSettings
{
    public int columns = 10;
    public int rows = 3;
    // sideways speed with the full fleet, and with only one burger left
    public float startSpeed = .1f;
    public float lastBurgerSpeed = .4f;
    // seconds between swoop dives with the full fleet and with one burger left
    public float swoopIntervalStart = 4f;
    public float swoopIntervalEnd = 1.2f;
    // true: every burger wanders on its own instead of moving as one formation
    public bool independentMovement = false;
    // whether a Giant Burger comes after the burgers; without one, the wave ends with the last burger
    public bool hasBoss = true;
    public int bossHealth = 20;
    // Giant Burger bullets per volley (two more below half health), and seconds between
    // volleys at full health and when nearly beaten
    public int bossBulletsPerVolley = 5;
    public float bossFireInterval = 1.4f;
    public float bossLowHealthFireInterval = .7f;
}

public class WaveManager : MonoBehaviour
{
    [Header("Inscribed")]
    // each wave is a fleet of burgers followed by a Giant Burger; clearing the last one wins
    public List<WaveSettings> waves = new List<WaveSettings>();
    public AlienFleet fleet;
    public GameScreens screens;
    // the "WAVE 2" banner shown between waves
    public GameObject banner;
    public TextMeshProUGUI bannerText;
    public float bannerTime = 2.5f;

    [Header("Dynamic")]
    public int waveIndex = 0;

    public int WaveNumber { get { return waveIndex + 1; } }
    public WaveSettings CurrentWave { get { return waves[waveIndex]; } }

    void Start()
    {
        banner.SetActive(false);
        fleet.SpawnWave(CurrentWave);
    }

    // called when the current wave is over: its Giant Burger is beaten, or its last burger caught
    public void WaveCleared()
    {
        if (waveIndex + 1 >= waves.Count)
        {
            string won;
            if (waves.Count > 1)
            {
                won = "You beat all " + waves.Count + " waves!";
            }
            else
            {
                won = CurrentWave.hasBoss ? "You beat the Giant Burger!" : "You caught every burger!";
            }
            screens.EndGame(true, won + "   Score: " + fleet.score);
            return;
        }
        // after each Giant Burger you pick a new buff before the next wave
        if (CurrentWave.hasBoss)
        {
            screens.ChooseBuffThen(AdvanceWave);
        }
        else
        {
            AdvanceWave();
        }
    }

    void AdvanceWave()
    {
        waveIndex++;
        StartCoroutine(NextWave());
    }

    IEnumerator NextWave()
    {
        fleet.RefreshCounter();
        bannerText.text = "WAVE " + WaveNumber;
        banner.SetActive(true);
        yield return new WaitForSeconds(bannerTime);
        banner.SetActive(false);
        fleet.SpawnWave(CurrentWave);
    }
}
