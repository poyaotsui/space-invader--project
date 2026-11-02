using UnityEngine;

// Pixel burger face: blinks now and then, opens its mouth when it fires,
// winces when hit, and bobs gently. Sits on the burger's sprite.
public class BurgerFace : MonoBehaviour
{
    [Header("Inscribed")]
    public Sprite idle;
    public Sprite blink;
    public Sprite chomp;
    public Sprite ouch;
    public Sprite angry;
    public Sprite angryChomp;
    // how far it squashes and stretches while bobbing, and how fast
    public float bobAmount = .04f;
    public float bobSpeed = 4f;

    // the giant burger frowns once it's below half health
    [HideInInspector] public bool mad = false;

    SpriteRenderer sr;
    Vector3 baseScale;
    float bobOffset;
    float nextBlink;
    float blinkUntil;
    Sprite held;
    float heldUntil;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        // so a whole fleet doesn't blink and bob in sync
        bobOffset = Random.value * 10f;
        nextBlink = Time.time + Random.Range(1f, 4f);
    }

    void Update()
    {
        if (Time.time >= nextBlink)
        {
            blinkUntil = Time.time + .12f;
            nextBlink = Time.time + Random.Range(2f, 5f);
        }

        Sprite face;
        if (Time.time < heldUntil)
        {
            face = held;
        }
        else if (mad)
        {
            face = angry;
        }
        else if (Time.time < blinkUntil)
        {
            face = blink;
        }
        else
        {
            face = idle;
        }
        sr.sprite = face;

        float bob = Mathf.Sin((Time.time + bobOffset) * bobSpeed) * bobAmount;
        transform.localScale = new Vector3(baseScale.x * (1 + bob), baseScale.y * (1 - bob), baseScale.z);
    }

    // open mouth for a moment, used when the burger fires
    public void Chomp()
    {
        Hold(mad ? angryChomp : chomp, .3f);
    }

    // wince, used when the burger is hit
    public void Ouch(float duration = .35f)
    {
        Hold(ouch, duration);
    }

    void Hold(Sprite sprite, float duration)
    {
        held = sprite;
        heldUntil = Time.time + duration;
    }
}
