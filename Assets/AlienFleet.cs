using UnityEngine;
using System.Collections.Generic;

public class AlienFleet : MonoBehaviour
{
    [Header("Inscribed")]

    public GameObject alienPrefab;

    public AlienCounter alienCounter;

    public Spaceship spaceship;

    public GameScreens screens;

    // starts once every burger is gone, if the wave has a boss
    public BossFight bossFight;

    // decides what each wave looks like and what happens after each Giant Burger
    public WaveManager waves;

    // may drop a pickup when a burger is caught
    public LootSystem loot;

    [Header("Dynamic")]
    public bool gameLost = false;

    public List<Alien> aliens = new List<Alien> { };

    // sideways speed with the full fleet, and with only one alien left (set by each wave)
    public float startSpeed = .1f;
    public float lastAlienSpeed = .4f;

    public int pointsPerBurger = 10;
    public int score = 0;

    [Header("Movement patterns")]
    // the fleet switches between March, Wave and Zigzag this often (seconds)
    public float patternDuration = 6f;
    // Wave: how far (units) and how fast (radians per second) the fleet ripples up and down
    public float waveHeight = .7f;
    public float waveSpeed = 3f;
    // Zigzag: chance per physics step of turning around early, and the shortest time between turns
    public float zigzagChance = .03f;
    public float zigzagMinTime = .6f;
    // Swoop: seconds between dives with the full fleet and with one burger left (set by each wave)
    public float swoopIntervalStart = 4f;
    public float swoopIntervalEnd = 1.2f;
    public float swoopDuration = 2.4f;
    public float swoopDepth = 8f;
    // swoops never come closer than this above the ship's firing line
    public float swoopClearance = 4f;
    public float swoopFireMultiplier = 5f;

    [Header("Independent movement")]
    // in waves with independent movement every burger wanders on its own, inside a band this tall
    // whose top starts at roamStartTop and sinks toward the ship at roamDescent units per second
    public float roamStartTop = 10.5f;
    public float roamBandHeight = 5f;
    public float roamDescent = .15f;
    // how long a burger keeps one heading before picking a new one (a random time between these, seconds)
    public float roamTurnMin = 1f;
    public float roamTurnMax = 3f;
    // set by each wave
    public bool independentMovement = false;
    float roamBandTop;

    enum Pattern { March, Wave, Zigzag }
    public string currentPattern = "March";
    Pattern pattern = Pattern.March;
    float patternEndTime;

    int startingAlienCount;
    float direction = 1;
    float leftBound = -16;
    float rightBound = 16;
    float downTime = .1f;
    bool movingDown = false;

    // the formation moves as one; each burger sits in its own slot relative to it
    Vector3 formation = Vector3.zero;
    // 0 = no wave, 1 = full wave; eased so switching patterns doesn't make the fleet jump
    float waveAmount = 0;
    float lastTurnTime;
    float nextSwoopTime;

    // builds a fresh formation for a wave and resets the fleet's movement
    public void SpawnWave(WaveSettings wave)
    {
        startSpeed = wave.startSpeed;
        lastAlienSpeed = wave.lastBurgerSpeed;
        swoopIntervalStart = wave.swoopIntervalStart;
        swoopIntervalEnd = wave.swoopIntervalEnd;
        independentMovement = wave.independentMovement;
        roamBandTop = roamStartTop;

        for (int i = 0; i < wave.columns; i++)
        {
            for (int j = 0; j < wave.rows; j++)
            {
                Vector3 start = new Vector3(2 * i - 12, 10 - 2 * j, 0);
                if (independentMovement)
                {
                    // spread across the whole width and the wander band
                    start.x = wave.columns > 1 ? Mathf.Lerp(-13f, 13f, (float)i / (wave.columns - 1)) : 0;
                    start.y = wave.rows > 1 ? Mathf.Lerp(roamBandTop, roamBandTop - roamBandHeight, (float)j / (wave.rows - 1)) : roamBandTop;
                }
                GameObject alienGO = Instantiate<GameObject>(alienPrefab);
                alienGO.transform.position = start;
                Alien alien = alienGO.GetComponent<Alien>();
                alien.fleet = this;
                alien.slot = start;
                alien.roamPosition = start;
                alien.roamPace = Random.Range(.6f, 1.4f);
                ChooseRoamHeading(alien, Time.fixedTime);
                aliens.Add(alien);
            }
        }
        startingAlienCount = aliens.Count;

        formation = Vector3.zero;
        direction = 1;
        CancelInvoke(nameof(startSidewaysMotion));
        movingDown = false;
        pattern = Pattern.March;
        currentPattern = independentMovement ? "Independent" : pattern.ToString();
        waveAmount = 0;
        //the game clock keeps counting across waves and restarts, so time everything from now
        lastTurnTime = Time.fixedTime;
        patternEndTime = Time.fixedTime + patternDuration;
        nextSwoopTime = Time.fixedTime + swoopIntervalStart;
        RefreshCounter();
    }

    public void AlienDestroyed(Alien alien, bool captured)
    {
        aliens.Remove(alien);
        if (captured)
        {
            score += pointsPerBurger;
            loot.BurgerCaught(alien.transform.position);
        }
        RefreshCounter();
        if (aliens.Count == 0 && !gameLost)
        {
            if (waves.CurrentWave.hasBoss)
            {
                bossFight.Begin();
            }
            else
            {
                waves.WaveCleared();
            }
        }
    }

    public void AddScore(int points)
    {
        score += points;
        RefreshCounter();
    }

    public void RefreshCounter()
    {
        alienCounter.Show(waves.WaveNumber, score, aliens.Count);
    }

    void FixedUpdate()
    {
        if (gameLost)
        {
            return;
        }
        if (spaceship == null)
        {
            LoseGame("You ran out of health.");
            return;
        }
        float firingLine = spaceship.transform.position.y;
        float t = Time.fixedTime;

        //speed up as aliens are destroyed
        float destroyed = 0;
        if (startingAlienCount > 1)
        {
            destroyed = (float)(startingAlienCount - aliens.Count) / (startingAlienCount - 1);
        }
        float speed = direction * Mathf.Lerp(startSpeed, lastAlienSpeed, destroyed);

        if (independentMovement)
        {
            RoamBurgers(t, Mathf.Lerp(startSpeed, lastAlienSpeed, destroyed));
        }
        else
        {
            MoveFormation(t, speed);
        }

        //send a burger on a dive toward the ship every so often
        if (t >= nextSwoopTime && aliens.Count > 0)
        {
            StartSwoop(aliens[Random.Range(0, aliens.Count)], firingLine, t);
            nextSwoopTime = t + Mathf.Lerp(swoopIntervalStart, swoopIntervalEnd, destroyed);
        }

        //place every burger: its slot in the formation, plus the wave, plus any swoop
        foreach (Alien alien in aliens)
        {
            Vector3 position;
            if (independentMovement)
            {
                position = alien.roamPosition;
            }
            else
            {
                position = formation + alien.slot;
                position.y += Mathf.Sin(t * waveSpeed + alien.slot.x * .5f) * waveHeight * waveAmount;
            }
            if (alien.swooping)
            {
                float p = (t - alien.swoopStart) / swoopDuration;
                if (p >= 1)
                {
                    alien.swooping = false;
                    alien.fireRateMultiplier = 1;
                }
                else
                {
                    //down and toward the ship, then back up to the slot
                    float dive = Mathf.Sin(p * Mathf.PI);
                    position.x += (alien.swoopTargetX - position.x) * dive;
                    position.y -= alien.swoopDepth * dive;
                }
            }
            alien.transform.position = position;
        }

        //the game can't be won once an alien is entirely below
        //the point the ship fires its darts from (darts only travel upwards)
        foreach (Alien alien in aliens)
        {
            if (alien.body.bounds.max.y < firingLine)
            {
                LoseGame("A burger got past you.");
                return;
            }
        }
    }

    // the whole fleet moves as one, switching between March, Wave and Zigzag
    void MoveFormation(float t, float speed)
    {
        //switch to a different pattern every so often
        if (t >= patternEndTime)
        {
            pattern = (Pattern)(((int)pattern + Random.Range(1, 3)) % 3);
            currentPattern = pattern.ToString();
            patternEndTime = t + patternDuration;
        }
        waveAmount = Mathf.MoveTowards(waveAmount, pattern == Pattern.Wave ? 1 : 0, Time.fixedDeltaTime);

        //move the formation
        if (!movingDown)
        {
            formation.x += speed;
        }
        else
        {
            formation.y += -startSpeed;
        }

        //change direction at the edges (by formation slot, so swooping burgers don't count)
        float left = float.MaxValue, right = float.MinValue;
        foreach (Alien alien in aliens)
        {
            float x = formation.x + alien.slot.x;
            left = Mathf.Min(left, x);
            right = Mathf.Max(right, x);
        }
        if (speed > 0 && right >= rightBound || speed < 0 && left <= leftBound)
        {
            direction = -direction;
            movingDown = true;
            lastTurnTime = t;
            Invoke(nameof(startSidewaysMotion), downTime);
        }
        else if (pattern == Pattern.Zigzag && !movingDown && t - lastTurnTime > zigzagMinTime && Random.value < zigzagChance)
        {
            //turn around early, without dropping
            direction = -direction;
            lastTurnTime = t;
        }
    }

    // every burger wanders on its own inside a band that slowly sinks toward the ship
    void RoamBurgers(float t, float stepSpeed)
    {
        roamBandTop -= roamDescent * Time.fixedDeltaTime;
        float bandBottom = roamBandTop - roamBandHeight;
        foreach (Alien alien in aliens)
        {
            if (t >= alien.roamNextTurn)
            {
                ChooseRoamHeading(alien, t);
            }
            Vector3 position = alien.roamPosition;
            position.x += alien.roamDirection.x * stepSpeed * alien.roamPace;
            position.y += alien.roamDirection.y * stepSpeed * alien.roamPace;
            //bounce off the sides and off the top and bottom of the band
            if (position.x > rightBound)
            {
                position.x = rightBound;
                alien.roamDirection.x = -Mathf.Abs(alien.roamDirection.x);
            }
            if (position.x < leftBound)
            {
                position.x = leftBound;
                alien.roamDirection.x = Mathf.Abs(alien.roamDirection.x);
            }
            if (position.y > roamBandTop)
            {
                position.y = roamBandTop;
                alien.roamDirection.y = -Mathf.Abs(alien.roamDirection.y);
            }
            if (position.y < bandBottom)
            {
                position.y = bandBottom;
                alien.roamDirection.y = Mathf.Abs(alien.roamDirection.y);
            }
            alien.roamPosition = position;
        }
    }

    // a new heading for one burger: mostly sideways, left or right, with some up or down
    void ChooseRoamHeading(Alien alien, float t)
    {
        float side = Random.value < .5f ? -1 : 1;
        float angle = Random.Range(-50f, 50f) * Mathf.Deg2Rad;
        alien.roamDirection = new Vector2(side * Mathf.Cos(angle), Mathf.Sin(angle));
        alien.roamNextTurn = t + Random.Range(roamTurnMin, roamTurnMax);
    }

    void StartSwoop(Alien alien, float firingLine, float t)
    {
        if (alien.swooping)
        {
            return;
        }
        //never dive so deep that a swoop counts as getting past the ship
        float slotY = independentMovement ? alien.roamPosition.y : formation.y + alien.slot.y;
        float depth = Mathf.Min(swoopDepth, slotY - (firingLine + swoopClearance));
        if (depth < 1)
        {
            return;
        }
        alien.swooping = true;
        alien.swoopStart = t;
        alien.swoopDepth = depth;
        alien.swoopTargetX = spaceship.transform.position.x;
        alien.fireRateMultiplier = swoopFireMultiplier;
    }

    void LoseGame(string reason)
    {
        gameLost = true;
        screens.EndGame(false, reason + "   Wave " + waves.WaveNumber + "   Score: " + score);
    }

    void startSidewaysMotion()
    {
        movingDown = false;
    }
}
