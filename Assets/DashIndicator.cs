using UnityEngine;
using UnityEngine.UI;

// the little circle above the health bar: it fills back up while the dash recharges
public class DashIndicator : MonoBehaviour
{
    [Header("Inscribed")]
    public Spaceship spaceship;
    // a radial-filled image on top of a dim copy of the same circle
    public Image fill;
    public Color chargingColor = new Color(0.55f, 0.75f, 1f, 0.8f);
    public Color readyColor = new Color(0.35f, 0.9f, 1f, 1f);

    void Update()
    {
        // the ship is destroyed on game over, so just leave the circle as it was
        if (spaceship == null)
        {
            return;
        }
        float charge = spaceship.DashCharge;
        fill.fillAmount = charge;
        fill.color = charge >= 1 ? readyColor : chargingColor;
    }
}
