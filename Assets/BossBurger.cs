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

    [Header("Jump attack")]
    // average seconds between jump attacks (set by each wave; 0 = never jumps)
    public float slamInterval = 12f;
    // extra bounces after the first landing (set by each wave)
    public int slamBounces = 1;
    // how long it winds up, tracking you, before it drops
    public float telegraphTime = 1f;
    // how long the drop takes
    public float dropTime = .35f;
    // how high the bounces go (each one is lower than the last), how long each takes,
    // and the most it can slide sideways toward you during one
    public float bounceHeight = 6f;
    public float bounceTime = .6f;
    public float bounceDrift = 5f;
    // how fast the landing marker follows you while the boss winds up (units per second)
    public float trackSpeed = 12f;
    // after the last landing it climbs back up in this long
    public float riseTime = .9f;
    public Sprite markerSprite;

    [Header("Dynamic")]
    public int health;
    public BossFight fight;

    bool entering = true;
    bool dead = false;
    bool slamming = false;
    float direction = 1;
    float nextVolley;
    float nextSlam;
    Vector3 baseScale;
    // grows on each hit and shrinks back, so hits make the burger "flinch"
    float punch = 0;
    Coroutine slamRoutine;
    BoxCollider body;
    // the red mark on the floor showing where the boss is about to land
    SpriteRenderer marker;
    Vector3 cameraStart;

    void Start()
    {
        health = maxHealth;
        baseScale = transform.localScale;
        body = GetComponent<BoxCollider>();
        cameraStart = Camera.main.transform.position;
        GameObject markerGO = new GameObject("SlamMarker");
        marker = markerGO.AddComponent<SpriteRenderer>();
        marker.sprite = markerSprite;
        markerGO.SetActive(false);
        StartCoroutine(Enter());
    }

    void OnDestroy()
    {
        // never leave the camera shaken or the marker behind
        if (Camera.main != null)
        {
            Camera.main.transform.position = cameraStart;
        }
        if (marker != null)
        {
            Destroy(marker.gameObject);
        }
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
        // the first jump comes a bit sooner than the average
        nextSlam = Time.time + slamInterval * .7f;
    }

    void FixedUpdate()
    {
        // while it's jumping, the jump coroutine moves it and it holds its fire
        if (entering || dead || slamming)
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

        if (slamInterval > 0 && Time.time >= nextSlam && fight.fleet.spaceship != null)
        {
            slamRoutine = StartCoroutine(Slam(rage));
            return;
        }

        if (Time.time >= nextVolley)
        {
            FireVolley();
            nextVolley = Time.time + Mathf.Lerp(fireInterval, lowHealthFireInterval, rage);
        }
    }

    // the jump attack: wind up above you, drop to the floor, bounce back up and down a few times
    // chasing you, then climb back to where it started. Landing on you costs a heart.
    IEnumerator Slam(float rage)
    {
        slamming = true;
        Spaceship ship = fight.fleet.spaceship;
        Camera cam = Camera.main;
        float floorY = cam.ViewportToWorldPoint(Vector3.zero).y;
        // the boss's centre when its bottom edge sits on the bottom of the screen
        float groundY = floorY + body.size.y * baseScale.y * .5f - body.center.y * baseScale.y;
        float leftLimit = cam.ViewportToWorldPoint(Vector3.zero).x + edgeMargin;
        float rightLimit = cam.ViewportToWorldPoint(Vector3.one).x - edgeMargin;

        // 1. wind up: it rises a little and shakes while the marker follows you,
        //    then the marker stops and flashes so you know where it's coming down
        Vector3 start = transform.position;
        float aimX = Mathf.Clamp(ship.transform.position.x, leftLimit, rightLimit);
        float bossX = start.x;
        marker.gameObject.SetActive(true);
        for (float t = 0; t < telegraphTime; t += Time.deltaTime)
        {
            float p = t / telegraphTime;
            if (p < .65f && ship != null)
            {
                aimX = Mathf.MoveTowards(aimX, Mathf.Clamp(ship.transform.position.x, leftLimit, rightLimit), trackSpeed * Time.deltaTime);
            }
            bossX = Mathf.MoveTowards(bossX, aimX, 8f * Time.deltaTime);
            transform.position = new Vector3(bossX + Random.Range(-.08f, .08f),
                start.y + Mathf.Sin(p * Mathf.PI * .5f) * .9f + Random.Range(-.05f, .05f), start.z);
            ShowMarker(aimX, floorY, p < .65f ? .5f : (Mathf.Sin(t * 40) > 0 ? .85f : .3f));
            yield return null;
        }

        // 2. the drop
        Vector3 from = transform.position;
        Vector3 ground = new Vector3(aimX, groundY, from.z);
        bool hitPlayer = false;
        for (float t = 0; t < dropTime; t += Time.deltaTime)
        {
            float p = t / dropTime;
            transform.position = Vector3.Lerp(from, ground, p * p);
            hitPlayer = CheckLanding(ship, hitPlayer);
            yield return null;
        }
        transform.position = ground;
        hitPlayer = CheckLanding(ship, hitPlayer);
        yield return Impact(floorY);

        // 3. bounces: each one is lower than the last and slides toward you, with the landing marked
        float height = bounceHeight;
        for (int i = 0; i < slamBounces && !dead; i++)
        {
            hitPlayer = false;
            from = transform.position;
            float landX = from.x;
            if (ship != null)
            {
                landX = Mathf.MoveTowards(from.x, Mathf.Clamp(ship.transform.position.x, leftLimit, rightLimit), bounceDrift);
            }
            for (float t = 0; t < bounceTime; t += Time.deltaTime)
            {
                float p = t / bounceTime;
                Vector3 position = new Vector3(Mathf.Lerp(from.x, landX, p), groundY + 4f * height * p * (1 - p), from.z);
                transform.position = position;
                ShowMarker(landX, floorY, .5f);
                hitPlayer = CheckLanding(ship, hitPlayer);
                yield return null;
            }
            transform.position = new Vector3(landX, groundY, from.z);
            hitPlayer = CheckLanding(ship, hitPlayer);
            yield return Impact(floorY);
            height *= .6f;
        }

        // 4. climb back up to its usual height and carry on
        marker.gameObject.SetActive(false);
        from = transform.position;
        Vector3 hover = new Vector3(Mathf.Clamp(from.x, leftLimit, rightLimit), fightHeight, from.z);
        for (float t = 0; t < riseTime; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(from, hover, Mathf.SmoothStep(0, 1, t / riseTime));
            yield return null;
        }
        transform.position = hover;
        transform.localScale = baseScale;
        slamming = false;
        nextVolley = Time.time + 1f;
        nextSlam = Time.time + Random.Range(.8f, 1.2f) * slamInterval * Mathf.Lerp(1f, .65f, rage);
    }

    // landing on the player costs a heart, once per landing (a shield or the post-hit breather still apply)
    bool CheckLanding(Spaceship ship, bool alreadyHit)
    {
        if (alreadyHit || ship == null)
        {
            return alreadyHit;
        }
        Physics.SyncTransforms();
        Collider shipBody = ship.GetComponentInChildren<BoxCollider>();
        if (!body.bounds.Intersects(shipBody.bounds))
        {
            return false;
        }
        ship.TakeHit();
        return true;
    }

    // the boss squashes flat, the camera shakes and the marker bursts outward
    IEnumerator Impact(float floorY)
    {
        float time = .3f;
        Camera cam = Camera.main;
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            float p = t / time;
            float squash = Mathf.Sin(p * Mathf.PI) * .22f;
            transform.localScale = new Vector3(baseScale.x * (1 + squash), baseScale.y * (1 - squash), baseScale.z);
            cam.transform.position = cameraStart + (Vector3)Random.insideUnitCircle * .35f * (1 - p);
            ShowMarker(transform.position.x, floorY, .6f * (1 - p), 1 + p * 1.2f);
            yield return null;
        }
        transform.localScale = baseScale;
        cam.transform.position = cameraStart;
    }

    // the red landing mark: a flat red glow on the floor, wide enough to cover everywhere the boss
    // would hit you (its own width plus half of yours on each side)
    void ShowMarker(float x, float floorY, float alpha, float widen = 1f)
    {
        marker.transform.position = new Vector3(x, floorY + .9f, .4f);
        marker.transform.localScale = new Vector3(body.size.x * baseScale.x * 1.4f * widen, 2f, 1f);
        marker.color = new Color(1f, .15f, .08f, alpha);
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
            // beaten mid-jump: stop the jump and tidy up what it was doing
            if (slamRoutine != null)
            {
                StopCoroutine(slamRoutine);
            }
            marker.gameObject.SetActive(false);
            Camera.main.transform.position = cameraStart;
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
