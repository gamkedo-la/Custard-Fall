using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI count;

    [SerializeField] private Resource resource;
    public Resource Resource => resource;

    [SerializeField] private Button Button;

    [SerializeField] [Tooltip("Don't forget to set this field!")]
    private Player Player;

    private Inventory _inventory;

    private void Start()
    {
        _inventory = Player.GetComponent<Inventory>();
    }


    public void AddItem(Sprite newIcon, int showCount)
    {
        Button.enabled = false;
        icon.sprite = newIcon;
        if (showCount > 0)
        {
            count.text = "" + showCount;
        }
        else
        {
            count.text = "";
        }
    }

    public void AddItem(Resource newResource, int showCount)
    {
        Button.enabled = true;
        resource = newResource;
        icon.sprite = newResource.PlaceableItem.Icon;
        if (showCount > 0)
        {
            count.text = "" + showCount;
        }
        else
        {
            count.text = "";
        }
    }

    public void ClearSlot()
    {
        Button.enabled = false;
        icon.sprite = null;
        resource = null;
        count.text = "";
    }

    public void TakeItem()
    {
        Player.EnterPlaceMode(resource.PlaceableItem, ()=>_inventory.GetResourceAmount(resource)>0,() => _inventory.AddOrSubResourceAmount(resource, -1) > 0);
    }
}