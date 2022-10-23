using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Volcano : InhaleListener
{
    private void Awake()
    {
        AddObstacleToWorld(2);
    }

    private void AddObstacleToWorld(int height)
    {
        var position = gameObject.transform.position;
        var cellPosition = worldCells.GetCellPosition(position.x, position.z);
        for (int i = -4; i < 4; i++)
        for (int j = -4; j < 4; j++)
            // TODO prevent height overflow to heigher places if in corner
            worldCells.WriteWorldItemHeight(Coords.Of(cellPosition.X + i, cellPosition.Y + j), worldCells.GetWorldItemHeightAt(cellPosition) + height);
    }

    public override void Init()
    {
        
    }

    public override void OnResourceInhaledAndMaybeRemove(Inhaler inhaler, Resource resource, int amount)
    {
       // do not do anything
    }
}