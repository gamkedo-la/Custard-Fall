using System;
using Unity.VisualScripting;
using UnityEngine;


public class SmallCreamyConeTree : InhaleListener
{
    
    private void Awake()
    {
        AddObstacleToWorld(2);
    }

    private void AddObstacleToWorld(int height)
    {
        var position = gameObject.transform.position;
        var cellPosition = worldCells.GetCellPosition(position.x, position.z);
        worldCells.WriteWorldItemHeight(cellPosition, worldCells.GetWorldItemHeightAt(cellPosition) + height);
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
        base.OnResourceInhaledAndMaybeRemove(inhaler, resource, amount);
        if (GetRemainingResourcesCount() == 0)
        {
            AddObstacleToWorld(-2);
            gameObject.SetActive(false);
        }
    }
}