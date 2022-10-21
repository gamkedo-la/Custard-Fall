using System;
using Unity.VisualScripting;
using UnityEngine;


public class SingleCookieShard : InhaleListener
{
    public override void Init()
    {
        base.Init();

        AddToInhaleQueue(new Resource("Cookie shard"), .6f);
    }

    public override void OnResourceInhaled(Inhaler inhaler, Resource resource, int amount)
    {
        base.OnResourceInhaled(inhaler, resource, amount);
    }
}