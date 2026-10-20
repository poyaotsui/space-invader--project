using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// one card on the buff picker: click it (or hover then press Enter) to take that buff
public class BuffCard : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Inscribed")]
    public Image frame;
    public Image icon;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public TextMeshProUGUI number;
    public Color frameColor = new Color(0.8f, 0.42f, 0.22f);
    public Color highlightColor = new Color(1f, 0.82f, 0.3f);
    public float highlightScale = 1.06f;

    BuffPicker picker;
    int index;

    public void Setup(BuffPicker owner, int cardIndex)
    {
        picker = owner;
        index = cardIndex;
        number.text = (cardIndex + 1).ToString();
    }

    public void Show(BuffInfo buff)
    {
        icon.sprite = buff.icon;
        title.text = buff.title;
        description.text = buff.description;
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
