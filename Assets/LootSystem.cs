using UnityEngine;

// every few burgers you catch, there's a chance one drops a pickup:
// lettuce heals, a patty gives a shield, and a star lets you pick a buff
public class LootSystem : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject lettucePrefab;
    public GameObject pattyPrefab;
    public GameObject starPrefab;
    // the star opens the buff cards, so the drop needs to know about the game's screens
    public GameScreens screens;
    // a drop is rolled on every Nth burger caught
    public int everyBurgers = 5;
    // chance that the roll produces a drop
    [Range(0, 1)] public float dropChance = .6f;
    // of the drops, how many are stars (the rare one)
    [Range(0, 1)] public float starShare = .25f;
    // of the rest, how many are lettuce (the others are patties)
    [Range(0, 1)] public float lettuceShare = .5f;

    [Header("Dynamic")]
    public int burgersCaught = 0;

    public void BurgerCaught(Vector3 position)
    {
        burgersCaught++;
        if (burgersCaught % everyBurgers != 0 || Random.value >= dropChance)
        {
            return;
        }
        Drop(position);
    }

    public GameObject Drop(Vector3 position)
    {
        GameObject prefab;
        if (Random.value < starShare)
        {
            prefab = starPrefab;
        }
        else
        {
            prefab = Random.value < lettuceShare ? lettucePrefab : pattyPrefab;
        }
        GameObject drop = Instantiate<GameObject>(prefab);
        drop.transform.position = position;
        drop.GetComponent<LootDrop>().screens = screens;
        return drop;
    }
}
