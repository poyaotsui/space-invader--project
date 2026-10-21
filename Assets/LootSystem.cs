using UnityEngine;

// every few burgers you catch, there's a chance one drops a pickup: lettuce heals, a patty gives a shield
public class LootSystem : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject lettucePrefab;
    public GameObject pattyPrefab;
    // a drop is rolled on every Nth burger caught
    public int everyBurgers = 5;
    // chance that the roll produces a drop
    [Range(0, 1)] public float dropChance = .5f;
    // of the drops, how many are lettuce (the rest are patties)
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
        GameObject prefab = Random.value < lettuceShare ? lettucePrefab : pattyPrefab;
        GameObject drop = Instantiate<GameObject>(prefab);
        drop.transform.position = position;
        return drop;
    }
}
