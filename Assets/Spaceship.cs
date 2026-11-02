using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Spaceship : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject dartPrefab;

    // the ship fires on its own, once every this many seconds
    public float fireInterval = 1.5f;
    public float lastShot = 0;
    public float speed = 10f;
    // takeout boxes thrown per shot (buffs can raise it), and the gap between them
    public int boxesPerShot = 1;
    public float shotSpacing = 1.5f;

    // how many hits you can take; the ship is destroyed on the last one
    public int maxHealth = 3;
    // after a hit you can't be hit again for this long, so one burst of bullets can't take all your health
    public float invincibleTime = 1f;
    public HealthBar healthBar;

    // the ray gun pickup swaps the boxes for rapid laser bolts for a few seconds
    public GameObject laserPrefab;
    public float laserDuration = 5f;
    public float laserInterval = .15f;
    // laser bolts sit closer together than boxes when several go out at once
    public float laserSpacing = .6f;

    // the character shouts for a moment each time it throws
    public SpriteRenderer characterRenderer;
    public Sprite normalSprite;
    public Sprite shootingSprite;
    public float shoutTime = .15f;

    // the bubble shown around the ship while it holds a shield (from a patty pickup)
    public GameObject shieldBubble;
    // after a shield absorbs a hit, you can't be hit again for this long
    public float shieldBreakGrace = .4f;

    // Shift dashes the ship sideways in the direction it last moved, then has to recharge;
    // nothing can hit you mid-dash
    public float dashDistance = 6.5f;
    public float dashTime = .12f;
    public float dashCooldown = 2f;

    [Header("Dynamic")]
    public int health;
    // a shield soaks up the next hit completely; picking up another while holding one doesn't stack
    public bool hasShield { get; private set; }
    bool invincible = false;
    // -1 left, 1 right: the way a dash goes when no direction is held
    float facing = 1;
    float lastDash = -999;
    bool dashing = false;
    float laserUntil = -999;

    public bool HasLaser { get { return Time.time < laserUntil; } }

    // 0 right after a dash, 1 once it's ready again (the dash circle reads this)
    public float DashCharge { get { return Mathf.Clamp01((Time.time - lastDash) / dashCooldown); } }

    //how far the ship's mesh sticks out to the left and right of its position
    float leftExtent;
    float rightExtent;

    // Awake still runs while the ship is disabled behind the start screen, so the bar is filled from the beginning
    void Awake()
    {
        health = maxHealth;
        healthBar.Show(health, maxHealth);
        shieldBubble.SetActive(false);
    }

    // lettuce: restores health, up to the maximum
    public void Heal(int amount)
    {
        if (health <= 0)
        {
            return;
        }
        health = Mathf.Min(maxHealth, health + amount);
        healthBar.Show(health, maxHealth);
    }

    // patty: the next hit does no damage
    public void GiveShield()
    {
        hasShield = true;
        shieldBubble.SetActive(true);
    }

    // ray gun: lasers for a few seconds; picking up another restarts the timer
    public void GiveLaser()
    {
        laserUntil = Time.time + laserDuration;
    }

    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
        health += amount;
        healthBar.Show(health, maxHealth);
    }

    public void TakeHit()
    {
        if (invincible || dashing || health <= 0)
        {
            return;
        }
        if (hasShield)
        {
            hasShield = false;
            shieldBubble.SetActive(false);
            StartCoroutine(ShieldGrace());
            return;
        }
        health--;
        healthBar.Show(health, maxHealth);
        if (health <= 0)
        {
            Destroy(gameObject);
            return;
        }
        StartCoroutine(Invincibility());
    }

    // a short breather after the shield pops, so the same burst of bullets can't go straight through
    IEnumerator ShieldGrace()
    {
        invincible = true;
        yield return new WaitForSeconds(shieldBreakGrace);
        invincible = false;
    }

    IEnumerator Invincibility()
    {
        //blink while invincible
        invincible = true;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        float blinkTime = .1f;
        for (float t = 0; t < invincibleTime; t += blinkTime)
        {
            foreach (Renderer r in renderers)
            {
                r.enabled = !r.enabled;
            }
            yield return new WaitForSeconds(blinkTime);
        }
        foreach (Renderer r in renderers)
        {
            r.enabled = true;
        }
        invincible = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(r.bounds);
        }
        leftExtent = transform.position.x - bounds.min.x;
        rightExtent = bounds.max.x - transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        // arrow keys or A / D
        if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
        {
            Vector3 position = transform.position;
            position.x += speed * Time.deltaTime;
            transform.position = position;
            facing = 1;
        }
        if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
        {
            Vector3 position = transform.position;
            position.x -= speed * Time.deltaTime;
            transform.position = position;
            facing = -1;
        }
        // no dashing while the game is paused
        if (Keyboard.current.shiftKey.wasPressedThisFrame && !dashing && DashCharge >= 1 && Time.timeScale > 0)
        {
            StartCoroutine(Dash(facing));
        }
        keepOnScreen();
        if (Time.time - lastShot >= (HasLaser ? laserInterval : fireInterval))
        {
            Fire();
        }
        characterRenderer.sprite = Time.time - lastShot < shoutTime ? shootingSprite : normalSprite;
    }

    IEnumerator Dash(float direction)
    {
        dashing = true;
        lastDash = Time.time;
        for (float t = 0; t < dashTime; t += Time.deltaTime)
        {
            Vector3 position = transform.position;
            position.x += direction * dashDistance / dashTime * Time.deltaTime;
            transform.position = position;
            keepOnScreen();
            yield return null;
        }
        dashing = false;
    }

    void Fire()
    {
        // several boxes (or laser bolts) go out side by side, centred on the ship
        bool lasering = HasLaser;
        float spacing = lasering ? laserSpacing : shotSpacing;
        for (int i = 0; i < boxesPerShot; i++)
        {
            Vector3 position = transform.position;
            position.x += (i - (boxesPerShot - 1) / 2f) * spacing;
            GameObject bullet = Instantiate<GameObject>(lasering ? laserPrefab : dartPrefab);
            bullet.transform.position = position;
        }
        lastShot = Time.time;
    }

    void keepOnScreen()
    {
        //the screen edges are measured every frame so this still works if the window is resized
        Camera cam = Camera.main;
        float screenLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        float screenRight = cam.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;

        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, screenLeft + leftExtent, screenRight - rightExtent);
        transform.position = position;
    }

}


