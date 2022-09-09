using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;

    public void AddItem (Sprite newIcon)
     {
        icon.sprite = newIcon;
        Debug.Log("Item");
    }

     public void ClearSlot ()
     {
         icon.sprite = null;
     }
    
}