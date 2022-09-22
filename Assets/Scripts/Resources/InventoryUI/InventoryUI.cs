using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


/* This object updates the inventory UI. */

public class InventoryUI : MonoBehaviour
{
	Inventory inventory;

	public Transform itemsParent;   // The parent object of all the items
	public GameObject inventoryUI;  // The entire UI
	public Sprite[] iconArt;
	private InventorySlot[] iconList;
	private Dictionary<string, int> resnameToIconIndex = new Dictionary<string, int>();

	private bool isShowHideInventoryUi;

	void Start()
	{
		resnameToIconIndex["None"] = 0;
		resnameToIconIndex["Cookie shard"] = 1;
		//Cookie Mine counts as x5 cookie shards. Not handling yet
		resnameToIconIndex["Creamy Cone Tree"] = 3;
		//other tree don't have resource yet
		resnameToIconIndex["Waffle wall"] = 4; //using tree line
		resnameToIconIndex["Colletible"] = 6;

		inventory = Inventory.instance;
		inventory.onItemChangedCallback += UpdateUI;
		iconList = GetComponentsInChildren<InventorySlot>();
	}

	void Update()
	{
		// Check to see if we should open/close the inventory
		if (isShowHideInventoryUi)//Input.GetButtonDown("Inventory"))
		{
			inventoryUI.SetActive(!inventoryUI.activeSelf);
			isShowHideInventoryUi = false;
		}
	}
	
	public void OnInventory(InputValue context)
	{
		// Check to see if we should open/close the inventory
		// action is called twice with pressed true, so using frame based switch as a workaround
		isShowHideInventoryUi = true;
	}

	
	void UpdateUI()
	{
		Debug.Log("UPDATING UI");
		List<Resource> items=inventory.GetResourceList();
		//var newAmount = inventory.GetResourceAmount(resource);
		for(int ii = 0; ii < items.Count; ii++)
        {
			Debug.Log("have:"+items[ii].Name);
			int iconIndex = resnameToIconIndex[items[ii].Name];
			Debug.Log("icon " + iconIndex);
			iconList[ii].AddItem(iconArt[iconIndex]);
        }
		/*for (int i = 0; i < iconList.Length; i++)
        {
			//To Do: Display Icons Based on inventory, currently just shows putting images on slots
			iconList[i].AddItem(iconArt[i % iconArt.Length]);
        }*/
	}
}
