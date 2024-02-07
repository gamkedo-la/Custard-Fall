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

    private void FixedUpdate()
    {
        equippedImage.sprite = player.PlaceModeItemReference ? player.PlaceModeItemReference.Icon : defaultSprite;
    }
}