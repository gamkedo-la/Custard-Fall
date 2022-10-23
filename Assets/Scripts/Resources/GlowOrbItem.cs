using System;
using Unity.VisualScripting;
using UnityEngine;


public class GlowOrbItem : InhaleListener
{
    private bool stationary = false;

    
    public override void Init()
    {
        base.Init();

        // exists only at night
        TimeManager.onMorningStarted += (sender, arg) =>
        {
            if(!stationary){}
                Remove();
        };

        AddToInhaleQueue(new Resource("glow orb"), 1f);
    }

    public override void OnResourceInhaledAndMaybeRemove(Inhaler inhaler, Resource resource, int amount)
    {
        var player = inhaler.owner.GetComponent<Player>();
        if (player)
        {
            // XD
            player.TakeDamage(-10);
        }
        base.OnResourceInhaledAndMaybeRemove(inhaler, resource, amount);
    }
}