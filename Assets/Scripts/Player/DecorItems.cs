using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
struct Item
{
    public GameObject item;
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
                if (PlayerPrefs.GetInt("DecorItem" + i) == 1) items[i].item.SetActive(true);
                else items[i].item.SetActive(false);
                if (PlayerPrefs.GetInt("DecorItem" + i) == 1 && items[i].itemClass == 2) r++;
                if (PlayerPrefs.GetInt("DecorItem" + i) == 1 && items[i].itemClass == 3) l++;
            }
        }
        itemCount = items.Length;
        if (items[77].item && r == 0) items[77].item.SetActive(true);
        if (items[78].item && l == 0) items[78].item.SetActive(true);
    }
    public void SelectItem(int numberOfItem)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].itemClass == items[numberOfItem].itemClass && i != numberOfItem)
            {
                items[i].item.SetActive(false);
                PlayerPrefs.SetInt("DecorItem" + i, 0);
            }
        }
        if(items[numberOfItem].itemClass < 2 || items[numberOfItem].itemClass >= 2 && items[numberOfItem].item.activeInHierarchy == false) items[numberOfItem].item.SetActive(!items[numberOfItem].item.activeInHierarchy);
        if (items[numberOfItem].item.activeInHierarchy) PlayerPrefs.SetInt("DecorItem" + numberOfItem, 1);
        else PlayerPrefs.SetInt("DecorItem" + numberOfItem, 0);
    }
}
