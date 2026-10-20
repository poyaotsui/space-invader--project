using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BuffType { MoveSpeed, ExtraShot, ExtraHeart }

[Serializable]
public class BuffInfo
{
    public BuffType type;
    public string title;
    [TextArea] public string description;
    public Sprite icon;
}

// shown before each run: offers a few random buffs as cards and applies the one picked
public class BuffPicker : MonoBehaviour
{
    [Header("Inscribed")]
    // every buff that can be offered; the cards show a random selection of these
    public List<BuffInfo> buffs = new List<BuffInfo>();
    public GameObject screen;
    public BuffCard[] cards;
    public Spaceship spaceship;
    public float moveSpeedBonus = .2f;

    [Header("Dynamic")]
    public string pickedBuff = "";

    List<BuffInfo> offered = new List<BuffInfo>();
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

    // shows the cards; onPicked runs once a buff has been chosen and applied
    public void Show(Action picked)
    {
        onPicked = picked;
        List<BuffInfo> pool = new List<BuffInfo>(buffs);
        offered.Clear();
        while (offered.Count < cards.Length && pool.Count > 0)
        {
            int i = UnityEngine.Random.Range(0, pool.Count);
            offered.Add(pool[i]);
            pool.RemoveAt(i);
        }
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].gameObject.SetActive(i < offered.Count);
            if (i < offered.Count)
            {
                cards[i].Show(offered[i]);
            }
        }
        Highlight(-1);
        screen.SetActive(true);
    }

    void Update()
    {
        if (!screen.activeSelf || Keyboard.current == null)
        {
            return;
        }
        Keyboard k = Keyboard.current;
        // number keys pick directly; arrows move the highlight and Enter picks it
        if (k.digit1Key.wasPressedThisFrame || k.numpad1Key.wasPressedThisFrame) Pick(0);
        else if (k.digit2Key.wasPressedThisFrame || k.numpad2Key.wasPressedThisFrame) Pick(1);
        else if (k.digit3Key.wasPressedThisFrame || k.numpad3Key.wasPressedThisFrame) Pick(2);
        else if (k.leftArrowKey.wasPressedThisFrame) Highlight(highlighted <= 0 ? offered.Count - 1 : highlighted - 1);
        else if (k.rightArrowKey.wasPressedThisFrame) Highlight((highlighted + 1) % offered.Count);
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
        if (!screen.activeSelf || index < 0 || index >= offered.Count)
        {
            return;
        }
        Apply(offered[index]);
        pickedBuff = offered[index].title;
        screen.SetActive(false);
        onPicked();
    }

    void Apply(BuffInfo buff)
    {
        switch (buff.type)
        {
            case BuffType.MoveSpeed:
                spaceship.speed *= 1 + moveSpeedBonus;
                break;
            case BuffType.ExtraShot:
                spaceship.shotsPerPress++;
                break;
            case BuffType.ExtraHeart:
                spaceship.AddMaxHealth(1);
                break;
        }
    }
}
