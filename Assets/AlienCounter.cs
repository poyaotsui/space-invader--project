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
    public void SetRemaining(int remaining)
    {
        uiText.text = "Remaining Aliens: " + remaining.ToString("#,0");
    }

    public void ShowGameOver(int remaining)
    {
        uiText.text = "Game Over! Remaining Aliens: " + remaining.ToString("#,0");
    }
}
