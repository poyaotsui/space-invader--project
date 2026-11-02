using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// one card on the difficulty screen: click it (or hover then press Enter) to start on that difficulty
public class DifficultyCard : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Inscribed")]
    public Image frame;
    // the coloured strip across the top of the card
    public Image band;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public TextMeshProUGUI number;
    // three dots along the bottom: how many are filled shows how hard it is
    public Image[] dots;
    public Color frameColor = new Color(0.8f, 0.42f, 0.22f);
    public Color highlightColor = new Color(1f, 0.82f, 0.3f);
    public Color emptyDotColor = new Color(1f, 1f, 1f, .18f);
    public float highlightScale = 1.06f;

    DifficultyPicker picker;
    int index;

    public void Setup(DifficultyPicker owner, int cardIndex)
    {
        picker = owner;
        index = cardIndex;
        number.text = (cardIndex + 1).ToString();
    }

    public void Show(DifficultyLevel level)
    {
        title.text = level.name.ToUpper();
        description.text = level.description;
        band.color = level.color;
        for (int i = 0; i < dots.Length; i++)
        {
            dots[i].color = i <= index ? level.color : emptyDotColor;
        }
    }

    public void SetHighlighted(bool on)
    {
        frame.color = on ? highlightColor : frameColor;
        transform.localScale = Vector3.one * (on ? highlightScale : 1);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        picker.Highlight(index);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        picker.Pick(index);
    }
}
