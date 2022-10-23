using System;
using Unity.VisualScripting;
using UnityEngine;


public class LargeCreamyConeTree : InhaleListener
{
    private void Awake()
    {
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
        base.OnResourceInhaledAndMaybeRemove(inhaler, resource, amount);
        if (GetRemainingResourcesCount() == 0)
        {
            gameObject.SetActive(false);
        }
    }
}