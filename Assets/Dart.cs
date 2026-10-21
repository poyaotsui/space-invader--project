using UnityEngine;

public class Dart : MonoBehaviour
{
    [Header("Inscribed")]
    static float speed = .3f;

    // the takeout box lid, hinged at the back; open while flying, shut (no rotation) once it catches a burger
    public Transform lidHinge;

    // once a box has caught something it can't catch anything else
    public bool captured { get; private set; }
    int capturedSteps = 0;

    void FixedUpdate()
    {
        Vector3 position = transform.position;
        if (captured)
        {
            //float away and shrink with the burger inside
            position.y += speed / 4;
            transform.position = position;
            transform.localScale *= .93f;
            capturedSteps++;
            if (capturedSteps > 30)
            {
                Destroy(gameObject);
            }
            return;
        }

        position.y += speed;
        transform.position = position;
        if(position.y > 10)
        {
            Destroy(gameObject);
        }
    }

    public void Capture()
    {
        captured = true;
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }
        GetComponent<Rigidbody>().isKinematic = true;
        if (lidHinge != null)
        {
            lidHinge.localRotation = Quaternion.identity;
        }
    }

}
