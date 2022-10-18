using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuTabs : MonoBehaviour
{
    public GameObject craftingTab;
    public GameObject inventoryTab;

    public GameObject craftingItems;
    public GameObject inventoryItems;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HideAllTabs()
    {
        craftingItems.SetActive(false);
        inventoryItems.SetActive(false);
    }

    public void ShowCraftingTab()
    {
        HideAllTabs();
        craftingItems.SetActive(true);
    }

    public void ShowInventoryTab()
    {
        HideAllTabs();
        inventoryItems.SetActive(true);
    }
}
