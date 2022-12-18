using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Volcano : Inhalable
{
    protected override void Start()
    {
        base.Start();
        AddObstacleToWorld(2);
    }

    private void AddObstacleToWorld(int height)
    {
        var position = gameObject.transform.position;
        var cellPosition = worldCells.GetCellPosition(position.x, position.z);
        var centerTerrainHeight = worldCells.GetTerrainHeightAt(cellPosition);
        for (int i = -4; i < 4; i++)
        for (int j = -4; j < 4; j++)
        {
            var localX = cellPosition.X + i;
            var localY = cellPosition.Y + j;
            var localTerrainHeight = worldCells.GetTerrainHeightAt(localX, localY);
            var difference = localTerrainHeight - centerTerrainHeight;
            if (localTerrainHeight >= centerTerrainHeight && difference < height)
                worldCells.WriteWorldItemHeight(Coords.Of(localX, localY),
                    worldCells.GetWorldItemHeightAt(cellPosition) + height - difference);
        }
    }

    public override void Init()
    {
    }

    public override void OnResourceInhaledAndMaybeRemove(Inhaler inhaler, Resource resource, int amount)
    {
        // do not do anything
    }
}