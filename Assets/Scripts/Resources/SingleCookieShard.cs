using System;
using Unity.VisualScripting;
using UnityEngine;


public class SingleCookieShard : Inhalable
{
    private PlaceableItem _placeableItem;

    public override void Init()
    {
        base.Init();

        AddToInhaleQueue(new Resource("Cookie shard", _placeableItem), .55f);
    }


    private void Awake()
    {
        _placeableItem = GetComponent<PlaceableItemReference>().Item();
    }
}