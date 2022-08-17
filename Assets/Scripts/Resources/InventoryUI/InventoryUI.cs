using UnityEngine;

/* This object updates the inventory UI. */

public class InventoryUI : MonoBehaviour
{

	public Transform itemsParent;   // The parent object of all the items
	public GameObject inventoryUI;  // The entire UI


	void Start()
	{
		
	}

	void Update()
	{
		// Check to see if we should open/close the inventory
		if (Input.GetButtonDown("Inventory"))
		{
			inventoryUI.SetActive(!inventoryUI.activeSelf);
		}
	}

	
	void UpdateUI()
	{
		
	}
}
