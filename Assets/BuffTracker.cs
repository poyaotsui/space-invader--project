using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// the list on the right side of the screen showing every buff picked this run
public class BuffTracker : MonoBehaviour
{
    [Header("Inscribed")]
    // a hidden row with "Icon", "Name" and "Count" children; each buff gets a copy
    public GameObject rowTemplate;

    class Row
    {
        public TextMeshProUGUI count;
        public int stacks;
    }

    Dictionary<BuffType, Row> rows = new Dictionary<BuffType, Row>();

    void Awake()
    {
        rowTemplate.SetActive(false);
    }

    public void Add(BuffInfo buff)
    {
        Row row;
        if (!rows.TryGetValue(buff.type, out row))
        {
            GameObject go = Instantiate(rowTemplate, rowTemplate.transform.parent);
            go.SetActive(true);
            go.transform.Find("Icon").GetComponent<Image>().sprite = buff.icon;
            go.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = buff.title;
            row = new Row { count = go.transform.Find("Count").GetComponent<TextMeshProUGUI>() };
            rows[buff.type] = row;
        }
        // picking the same buff again stacks it
        row.stacks++;
        row.count.text = "x" + row.stacks;
    }
}
