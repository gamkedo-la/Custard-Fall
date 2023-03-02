using System;
using Unity.VisualScripting;
using UnityEngine;


public class Pole : Inhalable
{
    private WorldCells _worldCells;
    private PlaceableItem _placeableItem;
    private const int Height = 2;
    private bool hasRegisteredHeight;

    private void Awake()
    {
        AddObstacleToWorld(Height);
        var placeableItemReference = GetComponent<PlaceableItemReference>();
        _placeableItem = placeableItemReference.Item();
    }

    private void AddObstacleToWorld(int height)
    {
        hasRegisteredHeight = height > 0;
        var position = gameObject.transform.position;
        var cellPosition = worldCells.GetCellPosition(position.x, position.z);
        worldCells.WriteWorldItemHeight(cellPosition, worldCells.GetWorldItemHeightAt(cellPosition) + height);
    }

    protected override void Remove()
    {
        AddObstacleToWorld(-Height);
        base.Remove();
    }

    private void OnDestroy()
    {
        if (hasRegisteredHeight)
        {
            AddObstacleToWorld(-Height);
        }
    }
}