using System;
using UnityEngine;

public class GlowOrbItem : Inhalable
{
    public bool selfPlaced = true;
    private PlaceableItem _placeableItem;
    public int healingAmount = 15;

    protected override void Start()
    {
        base.Start();
        // exists only at night
        if (!selfPlaced && TimeManager.Instance.IsDayTime)
            Remove();
    }

    private void Awake()
    {
        _placeableItem = GetComponent<PlaceableItemReference>().Item();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!selfPlaced && TimeManager.Instance.IsDayTime)
            Remove();
    }

    public override void Init()
    {
        base.Init();

        AddToInhaleQueue(new Resource("glow orb", _placeableItem), 1f);
    }

    public override void OnResourceInhaledAndMaybeRemove(Inhaler inhaler, Resource resource, int amount)
    {
        if (!selfPlaced)
        {
            var player = inhaler.owner.GetComponent<Player>();
            if (player)
            {
                // XD
                player.TakeDamage(-healingAmount, DamageImplication.Health);
            }
        }

        base.OnResourceInhaledAndMaybeRemove(inhaler, resource, amount);
    }
}