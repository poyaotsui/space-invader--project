using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Spaceship : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject dartPrefab;

    public static float shotCooldown = .05f;
    public float lastShot = 0;
    public float speed = 10f;
    // takeout boxes fired per press (buffs can raise it), and the gap between them
    public int shotsPerPress = 1;
    public float shotSpacing = 1.5f;

    // how many hits you can take; the ship is destroyed on the last one
    public int maxHealth = 3;
    // after a hit you can't be hit again for this long, so one burst of bullets can't take all your health
    public float invincibleTime = 1f;
    public HealthBar healthBar;

    // the character shouts while the shoot key is held down
    public SpriteRenderer characterRenderer;
    public Sprite normalSprite;
    public Sprite shootingSprite;

    [Header("Dynamic")]
    public int health;
    bool invincible = false;

    //how far the ship's mesh sticks out to the left and right of its position
    float leftExtent;
    float rightExtent;

    // Awake still runs while the ship is disabled behind the start screen, so the bar is filled from the beginning
    void Awake()
    {
        health = maxHealth;
        healthBar.Show(health, maxHealth);
    }

    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
        health += amount;
        healthBar.Show(health, maxHealth);
    }

    public void TakeHit()
    {
        if (invincible || health <= 0)
        {
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
        if (Keyboard.current.rightArrowKey.isPressed)
        {
            Vector3 position = transform.position;
            position.x += speed * Time.deltaTime;
            transform.position = position;
        }
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            Vector3 position = transform.position;
            position.x -= speed * Time.deltaTime;
            transform.position = position;
        }
        keepOnScreen();
        characterRenderer.sprite = Keyboard.current.spaceKey.isPressed ? shootingSprite : normalSprite;
        if (Keyboard.current.spaceKey.wasPressedThisFrame && Time.time - lastShot > shotCooldown)
        {
            Fire();
        }
    }

    void Fire()
    {
        // several boxes go out side by side, centred on the ship
        for (int i = 0; i < shotsPerPress; i++)
        {
            Vector3 position = transform.position;
            position.x += (i - (shotsPerPress - 1) / 2f) * shotSpacing;
            GameObject bullet = Instantiate<GameObject>(dartPrefab);
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


