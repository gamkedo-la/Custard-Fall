using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/* This object updates the inventory UI. */

public class InventoryUI : MonoBehaviour
{

	public Transform itemsParent;   // The parent object of all the items
	public GameObject inventoryUI;  // The entire UI

	private bool isShowHideInventoryUi;

	void Start()
	{
		
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
		
	}
}
