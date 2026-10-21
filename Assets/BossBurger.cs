using System.Collections;
using UnityEngine;

public class BossBurger : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject bulletPrefab;
    public int maxHealth = 20;
    // where it settles after flying in from above the screen, and how long that takes
    public float fightHeight = 5.5f;
    public float enterTime = 2f;
    // sideways speed (units per second) at full health; it doubles as its health runs out
    public float moveSpeed = 4f;
    public float edgeMargin = 4f;
    // seconds between volleys at full health and when nearly beaten
    public float fireInterval = 1.4f;
    public float lowHealthFireInterval = .7f;
    // bullets per volley; two more once it's below half health
    public int bulletsPerVolley = 5;
    // sideways drift per step of the outermost bullets in a volley
    public float spread = .08f;

    [Header("Dynamic")]
    public int health;
    public BossFight fight;

    bool entering = true;
    bool dead = false;
    float direction = 1;
    float nextVolley;
    Vector3 baseScale;
    // grows on each hit and shrinks back, so hits make the burger "flinch"
    float punch = 0;

    void Start()
    {
        health = maxHealth;
        baseScale = transform.localScale;
        StartCoroutine(Enter());
    }

    IEnumerator Enter()
    {
        Vector3 start = transform.position;
        Vector3 end = new Vector3(start.x, fightHeight, start.z);
        for (float t = 0; t < enterTime; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0, 1, t / enterTime));
            yield return null;
        }
        transform.position = end;
        entering = false;
        nextVolley = Time.time + 1f;
    }

    void FixedUpdate()
    {
        if (entering || dead)
        {
            return;
        }
        punch = Mathf.MoveTowards(punch, 0, Time.fixedDeltaTime);
        transform.localScale = baseScale * (1 + punch);

        // 0 at full health, 1 when nearly beaten
        float rage = 1 - (float)health / maxHealth;

        Vector3 position = transform.position;
        position.x += direction * moveSpeed * (1 + rage) * Time.fixedDeltaTime;
        Camera cam = Camera.main;
        float leftEdge = cam.ViewportToWorldPoint(Vector3.zero).x + edgeMargin;
        float rightEdge = cam.ViewportToWorldPoint(Vector3.one).x - edgeMargin;
        if (position.x > rightEdge)
        {
            position.x = rightEdge;
            direction = -1;
        }
        if (position.x < leftEdge)
        {
            position.x = leftEdge;
            direction = 1;
        }
        position.y = fightHeight + Mathf.Sin(Time.fixedTime * 2) * .5f;
        transform.position = position;

        if (Time.time >= nextVolley)
        {
            FireVolley();
            nextVolley = Time.time + Mathf.Lerp(fireInterval, lowHealthFireInterval, rage);
        }
    }

    void FireVolley()
    {
        int count = health <= maxHealth / 2 ? bulletsPerVolley + 2 : bulletsPerVolley;
        for (int i = 0; i < count; i++)
        {
            // -1 for the leftmost bullet, 1 for the rightmost
            float side = count == 1 ? 0 : (float)i / (count - 1) * 2 - 1;
            GameObject bullet = Instantiate<GameObject>(bulletPrefab);
            bullet.transform.position = transform.position + Vector3.down * 2f;
            bullet.GetComponent<AlienBullet>().sideways = side * spread;
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        Dart dart = coll.gameObject.GetComponent<Dart>();
        if (dart == null || dart.captured)
        {
            return;
        }
        // the takeout box takes a bite and closes
        dart.Capture();
        if (entering || dead)
        {
            return;
        }
        health--;
        fight.BossHit(health, maxHealth);
        if (health <= 0)
        {
            dead = true;
            StartCoroutine(Die());
            return;
        }
        punch = .08f;
    }

    IEnumerator Die()
    {
        //spin and shrink away
        float dieTime = 1.2f;
        for (float t = 0; t < dieTime; t += Time.deltaTime)
        {
            transform.localScale = baseScale * (1 - t / dieTime);
            transform.Rotate(0, 0, 540 * Time.deltaTime);
            yield return null;
        }
        fight.BossDefeated();
        Destroy(gameObject);
    }
}
