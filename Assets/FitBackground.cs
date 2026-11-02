using UnityEngine;

// keeps the background picture covering the whole camera view; if the window is a different
// shape from the picture, the edges are cropped rather than leaving gaps
public class FitBackground : MonoBehaviour
{
    SpriteRenderer picture;

    void Awake()
    {
        picture = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        Camera cam = Camera.main;
        float viewHeight = cam.orthographicSize * 2;
        float viewWidth = viewHeight * cam.aspect;
        Vector2 size = picture.sprite.bounds.size;
        float scale = Mathf.Max(viewWidth / size.x, viewHeight / size.y);
        transform.localScale = new Vector3(scale, scale, 1);
        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, transform.position.z);
    }
}
