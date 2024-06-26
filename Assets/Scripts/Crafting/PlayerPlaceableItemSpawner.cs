﻿using System;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerPlaceableItemSpawner : CraftReceiver
{

    [SerializeField]
    private Player player;
    
    public override void TakePlaceableItem(PlaceableItem item, Func<bool> canPlaceMoreCheck, Func<bool> onItemPlaced)
    {
        player.EnterPlaceMode(item, canPlaceMoreCheck, onItemPlaced);
    }
}