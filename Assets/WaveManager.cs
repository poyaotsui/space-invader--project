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

    // called when the current wave's Giant Burger is beaten
    public void WaveCleared()
    {
        if (waveIndex + 1 >= waves.Count)
        {
            screens.EndGame(true, "You beat all " + waves.Count + " waves!   Score: " + fleet.score);
            return;
        }
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
