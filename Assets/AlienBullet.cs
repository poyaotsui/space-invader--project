using UnityEngine;

public class AlienBullet : MonoBehaviour
{
    [Header("Inscribed")]

    static float speed = .1f;

    void FixedUpdate()
    {
        Vector3 position = transform.position;
        position.y -= speed;
        transform.position = position;
        if(position.y < -10)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        Destroy(gameObject);
        Destroy(coll.gameObject);
    }
}
