using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Inscribed")]
    // the first segment; copies of it are made so there is one per point of max health
    public Image segmentTemplate;
    // tints for the salad picture: full colour while you have that health, greyed out once it's lost
    public Color fullColor = Color.white;
    public Color emptyColor = new Color(0.25f, 0.25f, 0.25f, 0.5f);

    List<Image> segments = new List<Image>();

    public void Show(int health, int maxHealth)
    {
        if (segments.Count == 0)
        {
            segments.Add(segmentTemplate);
        }
        while (segments.Count < maxHealth)
        {
            segments.Add(Instantiate(segmentTemplate, segmentTemplate.transform.parent));
        }
        for (int i = 0; i < segments.Count; i++)
        {
            segments[i].gameObject.SetActive(i < maxHealth);
            segments[i].color = i < health ? fullColor : emptyColor;
        }
    }
}
