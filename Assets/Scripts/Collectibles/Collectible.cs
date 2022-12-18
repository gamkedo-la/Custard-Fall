using System;
using UnityEngine;

public class Collectible : Inhalable
{

    public static EventHandler<int> onCollectiblePickup;


    static int ID = 0;

    int id;

    public override void Init()
    {
        base.Init();
        id = ID++;
        AddToInhaleQueue(new Resource("Collectible"), .6f);
    }

    void OnPickup()
    {
        onCollectiblePickup?.Invoke(this, id);
        Destroy(gameObject);
    }

    public override void OnResourceInhaledAndMaybeRemove(Inhaler inhaler, Resource resource, int amount)
    {
        base.OnResourceInhaledAndMaybeRemove(inhaler, resource, amount);
        if (GetRemainingResourcesCount() == 0)
        {
            gameObject.SetActive(false);
            // Debug.Log($"Got Collectible {id}");
            OnPickup();
        }
    }
}
