using UnityEngine;
using UnityEngine.InputSystem;

public class Spaceship : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject dartPrefab;

    public static float shotCooldown = .05f;
    public float lastShot = 0;
    public float speed = 10f;

    //how far the ship's mesh sticks out to the left and right of its position
    float leftExtent;
    float rightExtent;

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
        if (Keyboard.current.spaceKey.wasPressedThisFrame && Time.time - lastShot > shotCooldown)
        {
            Vector3 position = transform.position;
            GameObject bullet = Instantiate<GameObject>(dartPrefab);
            bullet.transform.position = position;
            lastShot = Time.time;
        }
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


