using System;
using Unity.VisualScripting;
using UnityEngine;


public class CookieMine : InhaleListener
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

    public override void OnResourceInhaled(Inhaler inhaler, Resource resource, int amount)
    {
        base.OnResourceInhaled(inhaler, resource, amount);
        if (GetRemainingResourcesCount() == 0)
        {
            gameObject.SetActive(false);
            Debug.Log("cookie mine depleted");
        }
    }
}