using System;
using Unity.VisualScripting;
using UnityEngine;


public class SmallCreamyConeTree : InhaleListener
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
        for (int i = -1; i < 1; i++)
        for (int j = -1; j < 1; j++)
            // TODO prevent height overflow to heigher places if in corner
            worldCells.WriteWorldItemHeight(Coords.Of(cellPosition.X + i, cellPosition.Y + j), worldCells.GetWorldItemHeightAt(cellPosition) + height);
    }
    
    public override void Init()
    {
        base.Init();

        AddToInhaleQueue(new Resource("Creamy Cone Tree"), .6f);
        AddToInhaleQueue(new Resource("Creamy Cone Tree"), .6f);
        AddToInhaleQueue(new Resource("Creamy Cone Tree"), .6f);
        AddToInhaleQueue(new Resource("Creamy Cone Tree"), .8f);
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