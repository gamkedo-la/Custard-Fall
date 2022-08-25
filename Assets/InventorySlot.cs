using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;

     InventorySlot item;

    public void AddItem (InventorySlot newInventorySlot)
     {
         item = new InventorySlot();

         //icon.sprite = item.icon;
         icon.enabled = true;
     }

     public void ClearSlot ()
     {
         item = null;

         icon.sprite = null;
         icon.enabled = false;
     }
    
}