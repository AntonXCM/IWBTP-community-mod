using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
struct Item
{
    public GameObject item;
    public GameObject[] itemSupplements;
    public int itemClass;
}
public class DecorItems : MonoBehaviour
{
    [SerializeField] private Item[] items;
    public int itemCount;
    private int r, l;
    private void Awake()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].item)
            {
                ActivateItem(i, PlayerPrefs.GetInt("DecorItem" + i) == 1);
                if (PlayerPrefs.GetInt("DecorItem" + i) == 1 && items[i].itemClass == 2) r++;
                if (PlayerPrefs.GetInt("DecorItem" + i) == 1 && items[i].itemClass == 3) l++;
            }
        }
        itemCount = items.Length;
        if (items[77].item && r == 0) ActivateItem(77, true);
        if (items[78].item && l == 0) ActivateItem(78, true);
    }
    public void SelectItem(int numberOfItem)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].itemClass == items[numberOfItem].itemClass && i != numberOfItem)
            {
                ActivateItem(i, false);
                PlayerPrefs.SetInt("DecorItem" + i, 0);
            }
        }
        if(items[numberOfItem].itemClass < 2 || items[numberOfItem].itemClass >= 2 && items[numberOfItem].item.activeInHierarchy == false)
        {
            bool enable = !items[numberOfItem].item.activeInHierarchy;
            ActivateItem(numberOfItem, enable);
        }
        if (items[numberOfItem].item.activeInHierarchy) PlayerPrefs.SetInt("DecorItem" + numberOfItem, 1);
        else PlayerPrefs.SetInt("DecorItem" + numberOfItem, 0);
    }
    public void ActivateItem(int i, bool state)
    {
        items[i].item.SetActive(state);
        foreach(var j in items[i].itemSupplements) j.SetActive(state);
    }
}
