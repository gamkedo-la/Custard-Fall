using System;
using Unity.VisualScripting;
using UnityEngine;


public class Seat : Inhalable
{
    
    private PlaceableItem _placeableItem;
    
    public override void Init()
    {
        base.Init();

        AddToInhaleQueue(new Resource("Seat", _placeableItem), 1.5f);
    }

    private void Awake()
    {
        _placeableItem = GetComponent<PlaceableItemReference>().Item();
    }
}