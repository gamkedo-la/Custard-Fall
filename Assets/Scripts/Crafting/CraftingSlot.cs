using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class CraftingSlot : MonoBehaviour
{
    [SerializeField] private Image Icon;
    [SerializeField] private CraftableItem Item;


    private void Awake()
    {
        if (Item != null)
        {
            Icon.sprite = Item.Item.Icon;
        }
    }
}