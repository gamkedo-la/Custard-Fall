using System;
using UnityEngine;


public abstract class CraftReceiver: MonoBehaviour
{

    public abstract void TakePlaceableItem(PlaceableItem item, Func<bool> canPlaceMoreCheck, Func<bool> onItemPlaced);

}