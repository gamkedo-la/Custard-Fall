using System;
using Unity.VisualScripting;
using UnityEngine;


public class GlowOrbItem : InhaleListener
{
    public override void Init()
    {
        base.Init();

        AddToInhaleQueue(new Resource("glow orb"), 1f);
    }
    
}