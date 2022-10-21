using System;
using Unity.VisualScripting;
using UnityEngine;


public class SingleCookieShard : InhaleListener
{
    public override void Init()
    {
        base.Init();

        AddToInhaleQueue(new Resource("Cookie shard"), .55f);
    }
    
}