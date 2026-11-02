using System;
using UnityEngine;
using UnityEngine.InputSystem;

// shown after the title screen (and after a game over): pick Easy, Medium or Hard
public class DifficultyPicker : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject screen;
    public DifficultyCard[] cards;
    public DifficultyManager manager;

    Action onPicked;
    int highlighted = -1;

    void Awake()
    {
        screen.SetActive(false);
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].Setup(this, i);
        }
    }

    // shows the cards; onPicked runs once a difficulty has been chosen and applied
    public void Show(Action picked)
    {
        onPicked = picked;
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].Show(manager.levels[i]);
        }
        // the last difficulty you played is highlighted to start with
        Highlight(DifficultyManager.lastChoice);
        screen.SetActive(true);
    }

    void Update()
    {
        if (!screen.activeSelf || Keyboard.current == null)
        {
            return;
        }
        Keyboard k = Keyboard.current;
        // number keys pick directly; left/right (or A/D) move the highlight and Enter picks it
        if (k.digit1Key.wasPressedThisFrame || k.numpad1Key.wasPressedThisFrame) Pick(0);
        else if (k.digit2Key.wasPressedThisFrame || k.numpad2Key.wasPressedThisFrame) Pick(1);
        else if (k.digit3Key.wasPressedThisFrame || k.numpad3Key.wasPressedThisFrame) Pick(2);
        else if (k.leftArrowKey.wasPressedThisFrame || k.aKey.wasPressedThisFrame) Highlight(highlighted <= 0 ? cards.Length - 1 : highlighted - 1);
        else if (k.rightArrowKey.wasPressedThisFrame || k.dKey.wasPressedThisFrame) Highlight((highlighted + 1) % cards.Length);
        else if ((k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame) && highlighted >= 0) Pick(highlighted);
    }

    public void Highlight(int index)
    {
        highlighted = index;
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].SetHighlighted(i == index);
        }
    }

    public void Pick(int index)
    {
        if (!screen.activeSelf || index < 0 || index >= cards.Length)
        {
            return;
        }
        manager.Apply(index);
        screen.SetActive(false);
        onPicked();
    }
}
