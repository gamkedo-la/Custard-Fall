using System;
using Unity.VisualScripting;
using UnityEngine;


public class LargeCreamyConeTree : InhaleListener
{
    private void Start()
    {
        AddObstacleToWorld(2);
    }

    private void AddObstacleToWorld(int height)
    {
        var position = gameObject.transform.position;
        var cellPosition = worldCells.GetCellPosition(position.x, position.z);
        var centerTerrainHeight = worldCells.GetTerrainHeightAt(cellPosition);
        for (int i = -2; i < 2; i++)
        for (int j = -2; j < 2; j++)
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
        base.Init();

        AddToInhaleQueue(new Resource("Creamy Cone Tree"), 1.5f);
        AddToInhaleQueue(new Resource("Creamy Cone Tree"), 1.5f);
        AddToInhaleQueue(new Resource("Creamy Cone Tree"), 1.5f);
        AddToInhaleQueue(new Resource("Creamy Cone Tree"), 1.5f);
        AddToInhaleQueue(new Resource("Creamy Cone Tree"), 2f);
        AddToInhaleQueue(new Resource("Creamy Cone Tree"), 2f);
        AddToInhaleQueue(new Resource("Creamy Cone Tree"), 2f);
        AddToInhaleQueue(new Resource("Creamy Cone Tree"), 3f);
    }

    public override void OnResourceInhaledAndMaybeRemove(Inhaler inhaler, Resource resource, int amount)
    {
        if (GetRemainingResourcesCount() == 0)
        {
            AddObstacleToWorld(-2);
        }

        base.OnResourceInhaledAndMaybeRemove(inhaler, resource, amount);
    }
}