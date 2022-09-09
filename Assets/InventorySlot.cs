using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public Sprite SingleCookieShard;

    public void AddItem ()
     {
         //icon.sprite = item.icon;
         icon.enabled = true;
     }

     public void ClearSlot ()
     {
         icon.sprite = null;
         icon.enabled = false;
     }
    
}