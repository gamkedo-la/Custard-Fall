using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;


[RequireComponent(typeof(PlaceableItemReference))]
public class ParametrizedInhaleable : Inhalable
{
    [SerializeField] private string resourceName;
    [SerializeField] private float timeToInhaleWhole = 2.0f;

    private PlaceableItem _placeableItem;

    public override void Init()
    {
        base.Init();

        if (timeToInhaleWhole != 0)
        {
            AddToInhaleQueue(new Resource(resourceName, _placeableItem), timeToInhaleWhole);
        }
    }

    private void Awake()
    {
        _placeableItem = GetComponent<PlaceableItemReference>().Item();
    }
}