using System;
using Unity.VisualScripting;
using UnityEngine;


public class CookieMine : Inhalable
{
    public override void Init()
    {
        base.Init();

        AddToInhaleQueue(new Resource("Cookie shard"), 1.5f);
        AddToInhaleQueue(new Resource("Cookie shard"), 1.5f);
        AddToInhaleQueue(new Resource("Cookie shard"), 1.5f);
        AddToInhaleQueue(new Resource("Cookie shard"), 2f);
        AddToInhaleQueue(new Resource("Cookie shard"), 3f);
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