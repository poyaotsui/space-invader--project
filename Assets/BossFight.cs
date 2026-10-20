using System.Collections;
using UnityEngine;

public class BossFight : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject bossPrefab;
    public AlienFleet fleet;
    public GameScreens screens;
    // the flashing WARNING overlay
    public GameObject warning;
    public int warningFlashes = 4;
    public float flashOnTime = .35f;
    public float flashOffTime = .2f;
    // the boss health bar; the fill shrinks from the right as the boss is hit
    public GameObject bossHealthBar;
    public RectTransform bossHealthFill;
    public int pointsPerHit = 5;
    public int defeatBonus = 250;

    [Header("Dynamic")]
    public bool started = false;

    void Start()
    {
        warning.SetActive(false);
        bossHealthBar.SetActive(false);
    }

    public void Begin()
    {
        if (started)
        {
            return;
        }
        started = true;
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        for (int i = 0; i < warningFlashes; i++)
        {
            warning.SetActive(true);
            yield return new WaitForSeconds(flashOnTime);
            warning.SetActive(false);
            yield return new WaitForSeconds(flashOffTime);
        }
        // flies in from above the top of the screen, with this wave's health and firing
        GameObject boss = Instantiate(bossPrefab, new Vector3(0, 16, 0), Quaternion.identity);
        BossBurger bossBurger = boss.GetComponent<BossBurger>();
        WaveSettings wave = fleet.waves.CurrentWave;
        bossBurger.fight = this;
        bossBurger.maxHealth = wave.bossHealth;
        bossBurger.bulletsPerVolley = wave.bossBulletsPerVolley;
        bossBurger.fireInterval = wave.bossFireInterval;
        bossBurger.lowHealthFireInterval = wave.bossLowHealthFireInterval;
        bossHealthBar.SetActive(true);
        SetFill(1);
    }

    public void BossHit(int health, int maxHealth)
    {
        fleet.AddScore(pointsPerHit);
        SetFill((float)health / maxHealth);
    }

    public void BossDefeated()
    {
        fleet.AddScore(defeatBonus);
        bossHealthBar.SetActive(false);
        // ready for the next wave's Giant Burger
        started = false;
        fleet.waves.WaveCleared();
    }

    void SetFill(float fraction)
    {
        bossHealthFill.anchorMax = new Vector2(fraction, 1);
    }
}
