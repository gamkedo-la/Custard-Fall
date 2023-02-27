using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;


[RequireComponent(typeof(PlaceableItemReference))]
public class ParametrizedInhaleable : Inhalable
{
    [SerializeField] private string resourceName;
    
    private PlaceableItem _placeableItem;
    
    public override void Init()
    {
        base.Init();

        AddToInhaleQueue(new Resource(resourceName, _placeableItem), 1.5f);
    }

    private void Awake()
    {
        _placeableItem = GetComponent<PlaceableItemReference>().Item();
    }
}