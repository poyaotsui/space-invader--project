using System.Collections.Generic;
using UnityEngine;

// everything that changes with the difficulty. Medium is the game as it plays by default;
// the multipliers are applied on top of each wave's own settings.
[System.Serializable]
public class DifficultyLevel
{
    public string name = "Medium";
    // shown on the difficulty card, one line per effect
    [TextArea(3, 8)] public string description;
    public Color color = Color.white;

    [Header("You")]
    public int hearts = 3;
    // seconds between your throws (before buffs)
    public float throwInterval = 1.5f;
    // chance that every 5th burger caught drops loot
    [Range(0, 1)] public float lootChance = .6f;

    [Header("Burgers")]
    // multiplies how fast burgers move
    public float burgerSpeed = 1;
    // multiplies how often burgers shoot
    public float burgerFire = 1;
    // multiplies the time between swoop dives (more than 1 = fewer dives)
    public float swoopInterval = 1;

    [Header("Giant Burger")]
    public float bossHealth = 1;
    // added to the bullets per volley
    public int bossVolleyBonus = 0;
    // multiplies the time between volleys (more than 1 = fewer volleys)
    public float bossFireInterval = 1;
    // multiplies the time between jump attacks (more than 1 = fewer jumps)
    public float bossJumpInterval = 1;
    // added to the extra bounces after a landing
    public int bossBounceBonus = 0;
}

public class DifficultyManager : MonoBehaviour
{
    [Header("Inscribed")]
    public List<DifficultyLevel> levels = new List<DifficultyLevel>();
    public Spaceship spaceship;
    public AlienFleet fleet;
    public LootSystem loot;
    public AlienCounter counter;

    // the last difficulty picked, so a restart highlights it (and Medium is the default)
    public static int lastChoice = 1;

    [Header("Dynamic")]
    public int index = 1;

    public DifficultyLevel Current { get { return levels[index]; } }

    // applies a difficulty to everything it changes: call this before the game starts
    public void Apply(int choice)
    {
        index = Mathf.Clamp(choice, 0, levels.Count - 1);
        lastChoice = index;
        DifficultyLevel level = Current;

        spaceship.maxHealth = level.hearts;
        spaceship.health = level.hearts;
        spaceship.healthBar.Show(spaceship.health, spaceship.maxHealth);
        spaceship.fireInterval = level.throwInterval;
        loot.dropChance = level.lootChance;
        Alien.fireScale = level.burgerFire;
        fleet.SetDifficulty(level.burgerSpeed, level.swoopInterval);
        counter.difficultyName = level.name;
        fleet.RefreshCounter();
    }
}
