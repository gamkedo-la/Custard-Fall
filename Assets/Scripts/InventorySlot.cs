using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI count;

    public void AddItem (Sprite newIcon, int showCount)
     {
        icon.sprite = newIcon;
        if(showCount>0) {
            count.text = "" + showCount;
        } else {
            count.text = "";
        }
        // Debug.Log("Item");
    }

     public void ClearSlot ()
     {
         icon.sprite = null;
     }
    
}