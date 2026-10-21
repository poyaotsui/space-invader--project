using UnityEngine;

public enum LootType { Lettuce, Patty }

// a pickup that falls from a caught burger; touching it with the ship collects it
public class LootDrop : MonoBehaviour
{
    [Header("Inscribed")]
    public LootType type;
    // units per second
    public float fallSpeed = 2.5f;
    // degrees per second the pickup spins around
    public float spinSpeed = 120;
    public Transform visual;

    void FixedUpdate()
    {
        Vector3 position = transform.position;
        position.y -= fallSpeed * Time.fixedDeltaTime;
        transform.position = position;
        if (position.y < -10)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        visual.Rotate(0, spinSpeed * Time.deltaTime, 0, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        Spaceship ship = other.GetComponentInParent<Spaceship>();
        if (ship == null)
        {
            return;
        }
        switch (type)
        {
            case LootType.Lettuce:
                ship.Heal(1);
                break;
            case LootType.Patty:
                ship.GiveShield();
                break;
        }
        Destroy(gameObject);
    }
}
