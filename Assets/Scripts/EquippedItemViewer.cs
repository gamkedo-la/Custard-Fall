using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquippedItemViewer : MonoBehaviour
{
    
    [SerializeField] Image equippedImage;
    [SerializeField] Sprite defaultSprite;
    [SerializeField] Player player;
    GameObject currentItemInHand = null;

    private void Update() {
        if(currentItemInHand == player.itemInHand){
            return;
        }

        InventoryIcon invIcon;

        if(player.itemInHand.TryGetComponent<InventoryIcon>(out invIcon)){
            equippedImage.sprite = invIcon.sprite;
            return;
        }

        equippedImage.sprite = defaultSprite;


    }
}
