using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


/* This object updates the inventory UI. */

public class InventoryUI : MonoBehaviour
{
    Inventory inventoryContainer;

    public Transform itemsParent; // The parent object of all the items
    public GameObject inventoryUI; // The entire UI
    public Sprite[] iconArt;
    private InventorySlot[] inventorySlots;
    private Dictionary<string, int> resnameToIconIndex = new Dictionary<string, int>();

    private bool _isShowHideInventoryUi;

    void Start()
    {
        resnameToIconIndex["None"] = 0;
        resnameToIconIndex["Cookie shard"] = 1;
        //Cookie Mine counts as x5 cookie shards. Not handling yet
        resnameToIconIndex["Creamy Cone Tree"] = 3;
        //other tree don't have resource yet
        resnameToIconIndex["Waffle wall"] = 4;
        resnameToIconIndex["Tree Tall"] = 5;
        resnameToIconIndex["Collectible"] = 6;
        resnameToIconIndex["glow orb"] = 7;

        inventoryContainer = Inventory.instance;
        inventoryContainer.SetUIRef(this);
        inventoryContainer.onItemChangedCallback += UpdateUI;
        inventorySlots = GetComponentsInChildren<InventorySlot>();
    }

    void Update()
    {
        // Check to see if we should open/close the inventoryContainer
        if (_isShowHideInventoryUi)
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
            _isShowHideInventoryUi = false;
        }
    }

    public void OnInventory(InputValue context)
    {
        // Check to see if we should open/close the inventoryContainer
        // action is called twice with pressed true, so using frame based switch as a workaround
        _isShowHideInventoryUi = true;
    }


    public void UpdateUI()
    {
        // Debug.Log("UPDATING UI");
        List<Inventory.InventorySlot> items = inventoryContainer.GetResourceList();

        // blank them all out first
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].AddItem(iconArt[0], 0);
        }

        int slotIndex = 0;
        for (int ii = 0; ii < items.Count; ii++)
        {
            var resource = items[ii].Resource;
            int iconIndex;
            try
            {
                iconIndex= resnameToIconIndex[resource.Name];
            }
            catch (KeyNotFoundException e)
            {
                // just some fallback
                iconIndex= resnameToIconIndex["Waffle wall"];
            }
            if (resource.PlaceableItem)
            {
                inventorySlots[slotIndex].AddItem(resource, items[ii].Amount);
            }
            else
            {
                inventorySlots[slotIndex].AddItem(iconArt[iconIndex], items[ii].Amount);
            }

            slotIndex++;
        }
    }
}