using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;

    public void AddItem ()
     {
        //icon.sprite = item.icon;
        Debug.Log("Item");
    }

     public void ClearSlot ()
     {
         icon.sprite = null;
     }
    
}