using UnityEngine;
using TMPro;

public class AlienCounter : MonoBehaviour
{
    private TextMeshProUGUI uiText;

    // Awake runs before any Start, so the fleet can set the count from its own Start
    void Awake()
    {
        uiText = GetComponent<TextMeshProUGUI>();
    }

    // the text is only rebuilt when the count changes, not every frame
    public void Show(int wave, int score, int remaining)
    {
        uiText.text = "Wave " + wave + "     Score: " + score.ToString("#,0") + "     Burgers Left: " + remaining.ToString("#,0");
    }
}
