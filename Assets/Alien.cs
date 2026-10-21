using UnityEngine;

public class Alien : MonoBehaviour
{
    [Header("Inscribed")]

    public GameObject bulletPrefab;
    public AlienFleet fleet;

    static float bulletSpawnRate = .002f;
    public float lastShot = 0;
    public static float shotCooldown = .1f;

    // looked up once here instead of every physics step by the fleet
    public Collider body { get; private set; }

    // OnCollisionEnter can fire once per collider touched, so only count the first hit
    bool hit = false;

    // set by the fleet: this burger's place in the formation, and its current dive
    [HideInInspector] public Vector3 slot;
    [HideInInspector] public bool swooping = false;
    [HideInInspector] public float swoopStart;
    [HideInInspector] public float swoopDepth;
    [HideInInspector] public float swoopTargetX;
    // swooping burgers fire more often
    [HideInInspector] public float fireRateMultiplier = 1;

    // independent movement: this burger's own position, heading and pace, set by the fleet
    [HideInInspector] public Vector3 roamPosition;
    [HideInInspector] public Vector2 roamDirection;
    [HideInInspector] public float roamPace = 1;
    [HideInInspector] public float roamNextTurn;

    void Awake()
    {
        body = GetComponent<Collider>();
    }

    void FixedUpdate()
    {
        Vector3 position = transform.position;

        if(UnityEngine.Random.value < bulletSpawnRate * fireRateMultiplier && Time.time - lastShot > shotCooldown){
            GameObject bullet = Instantiate<GameObject>(bulletPrefab);
            bullet.transform.position = position;
            lastShot = Time.time;
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        //a box that has already caught a burger this physics step can't catch another
        Dart dart = coll.gameObject.GetComponent<Dart>();
        if (hit || (dart != null && dart.captured))
        {
            return;
        }
        hit = true;

        //a takeout box catches the burger and scores; ramming the ship costs it health;
        //anything else just destroys both
        Spaceship ship = coll.gameObject.GetComponent<Spaceship>();
        fleet.AlienDestroyed(this, dart != null);
        if (dart != null)
        {
            dart.Capture();
        }
        else if (ship != null)
        {
            ship.TakeHit();
        }
        else
        {
            Destroy(coll.gameObject);
        }
        Destroy(gameObject);
    }
}
