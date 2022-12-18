using System;
using Unity.VisualScripting;
using UnityEngine;


public class GlowOrbItem : Inhalable
{
    private bool stationary = false;


    protected override void Start()
    {
        base.Start();
        // exists only at night
        if(TimeManager.Instance.IsDayTime)
            Remove();
        
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if(TimeManager.Instance.IsDayTime)
            Remove();
    }

    public override void Init()
    {
        base.Init();

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