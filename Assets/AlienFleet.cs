using UnityEngine;
using System.Collections.Generic;

public class AlienFleet : MonoBehaviour
{
    [Header("Inscribed")]

    public GameObject alienPrefab;

    public AlienCounter alienCounter;

    public Spaceship spaceship;

    public GameScreens screens;

    [Header("Dynamic")]
    public bool gameLost = false;

    public List<Alien> aliens = new List<Alien> { };

    // sideways speed with the full fleet, and with only one alien left
    public float startSpeed = .1f;
    public float lastAlienSpeed = .4f;

    int startingAlienCount;
    float direction = 1;
    float leftBound = -16;
    float rightBound = 16;
    float downTime = .1f;
    bool movingDown = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                GameObject alienGO = Instantiate<GameObject>(alienPrefab);
                alienGO.transform.position = new Vector3(2 * i - 12, 10 - 2 * j, 0);
                Alien alien = alienGO.GetComponent<Alien>();
                alien.fleet = this;
                aliens.Add(alien);
            }
        }
        startingAlienCount = aliens.Count;
        alienCounter.SetRemaining(aliens.Count);
    }

    public void AlienDestroyed(Alien alien)
    {
        aliens.Remove(alien);
        alienCounter.SetRemaining(aliens.Count);
        if (aliens.Count == 0 && !gameLost)
        {
            screens.EndGame(true, "You destroyed every alien.");
        }
    }

    void FixedUpdate()
    {
        if (gameLost)
        {
            return;
        }

        //speed up as aliens are destroyed
        float destroyed = 0;
        if (startingAlienCount > 1)
        {
            destroyed = (float)(startingAlienCount - aliens.Count) / (startingAlienCount - 1);
        }
        float speed = direction * Mathf.Lerp(startSpeed, lastAlienSpeed, destroyed);

        //move all aliens
        foreach (Alien alien in aliens)
        {
            Vector3 position = alien.transform.position;
            if (!movingDown)
            {
                position.x += speed;
            }
            else
            {
                position.y += -startSpeed;
            }
            alien.transform.position = position;
        }

        //change alien direction
        float left = 0, right = 0;
        foreach (Alien alien in aliens)
        {
            Vector3 position = alien.transform.position;
            if (position.x > right)
            {
                right = position.x;
            }
            if (position.x < left)
            {
                left = position.x;
            }
        }
        if (speed > 0 && right >= rightBound || speed < 0 && left <= leftBound)
        {
            direction = -direction;
            movingDown = true;
            Invoke("startSidewaysMotion", downTime);
        }

        //the game can't be won once the ship is gone, or once an alien
        //is entirely below the point the ship fires its darts from
        //(darts only travel upwards)
        if (spaceship == null)
        {
            LoseGame("Your spaceship was destroyed.");
            return;
        }
        float firingLine = spaceship.transform.position.y;
        foreach (Alien alien in aliens)
        {
            if (alien.body.bounds.max.y < firingLine)
            {
                LoseGame("An alien got past your spaceship.");
                return;
            }
        }
    }

    void LoseGame(string reason)
    {
        gameLost = true;
        screens.EndGame(false, reason);
    }

    void startSidewaysMotion()
    {
        movingDown = false;
    }
}
