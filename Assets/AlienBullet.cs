using UnityEngine;

public class AlienBullet : MonoBehaviour
{
    [Header("Inscribed")]

    static float speed = .1f;
    // sideways movement per step; the giant burger fans its volleys out with this
    [HideInInspector] public float sideways = 0;

    void FixedUpdate()
    {
        Vector3 position = transform.position;
        position.y -= speed;
        position.x += sideways;
        transform.position = position;
        if(position.y < -10)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        Destroy(gameObject);
        Spaceship ship = coll.gameObject.GetComponent<Spaceship>();
        if (ship != null)
        {
            ship.TakeHit();
        }
        else
        {
            Destroy(coll.gameObject);
        }
    }
}
