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
        // the wave's settings, adjusted by the difficulty picked at the start
        WaveSettings wave = fleet.waves.CurrentWave;
        DifficultyLevel level = fleet.difficulty.Current;
        bossBurger.fight = this;
        bossBurger.maxHealth = Mathf.Max(1, Mathf.RoundToInt(wave.bossHealth * level.bossHealth));
        bossBurger.bulletsPerVolley = Mathf.Max(1, wave.bossBulletsPerVolley + level.bossVolleyBonus);
        bossBurger.fireInterval = wave.bossFireInterval * level.bossFireInterval;
        bossBurger.lowHealthFireInterval = wave.bossLowHealthFireInterval * level.bossFireInterval;
        // 0 means this wave's boss never jumps, whatever the difficulty
        bossBurger.slamInterval = wave.bossSlamInterval * level.bossJumpInterval;
        bossBurger.slamBounces = Mathf.Max(0, wave.bossSlamBounces + level.bossBounceBonus);
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
