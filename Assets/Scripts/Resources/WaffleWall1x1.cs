using System;
using Unity.VisualScripting;
using UnityEngine;


public class WaffleWall1x1 : Inhalable
{
    private WorldCells _worldCells;
    private PlaceableItem _placeableItem;
    private const int Height = 1;

    public override void Init()
    {
        base.Init();

        AddToInhaleQueue(new Resource("Waffle wall", _placeableItem), 2f);
    }

    private void Awake()
    {
        AddObstacleToWorld(Height);
        var placeableItemReference = GetComponent<PlaceableItemReference>();
        _placeableItem = placeableItemReference.Item();
    }

    private void AddObstacleToWorld(int height)
    {
        var position = gameObject.transform.position;
        var cellPosition = worldCells.GetCellPosition(position.x, position.z);
        worldCells.WriteWorldItemHeight(cellPosition, worldCells.GetWorldItemHeightAt(cellPosition) + height);
    }

    public override void OnResourceInhaledAndMaybeRemove(Inhaler inhaler, Resource resource, int amount)
    {
        if (GetRemainingResourcesCount() == 0)
        {
            AddObstacleToWorld(-Height);
        }

        base.OnResourceInhaledAndMaybeRemove(inhaler, resource, amount);
    }
}