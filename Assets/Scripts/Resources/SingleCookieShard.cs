using System;
using Unity.VisualScripting;
using UnityEngine;


public class SingleCookieShard : Inhalable
{
    public override void Init()
    {
        base.Init();

        AddToInhaleQueue(new Resource("Cookie shard"), .55f);
    }
    
}