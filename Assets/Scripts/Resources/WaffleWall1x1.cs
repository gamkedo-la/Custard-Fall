using System;
using Unity.VisualScripting;
using UnityEngine;


public class WaffleWall1x1 : InhaleListener
{
    private WorldCells _worldCells;
    
    public override void Init()
    {
        base.Init();

        AddToInhaleQueue(new Resource("Waffle wall"), 2f);
        AddObstacleToWorld(2);
    }

    private void AddObstacleToWorld(int height)
    {
        var position = gameObject.transform.position;
        var cellPosition = worldCells.GetCellPosition(position.x, position.z);
        worldCells.WriteWorldItemHeight(cellPosition, worldCells.GetWorldItemHeightAt(cellPosition) + height);
    }

    public override void OnResourceInhaled(Inhaler inhaler, Resource resource, int amount)
    {
        base.OnResourceInhaled(inhaler, resource, amount);
        if (GetRemainingResourcesCount() == 0)
        {
            AddObstacleToWorld(-2);
            gameObject.SetActive(false);
            Debug.Log("got the waffle wall piece");
        }
    }
}