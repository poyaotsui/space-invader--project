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

    void Awake()
    {
        body = GetComponent<Collider>();
    }

    void FixedUpdate()
    {
        Vector3 position = transform.position;

        if(UnityEngine.Random.value < bulletSpawnRate && Time.time - lastShot > shotCooldown){
            GameObject bullet = Instantiate<GameObject>(bulletPrefab);
            bullet.transform.position = position;
            lastShot = Time.time;
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        fleet.AlienDestroyed(this);
        Destroy(gameObject);
        Destroy(coll.gameObject);
    }
}
