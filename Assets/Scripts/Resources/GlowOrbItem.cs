using System;
using UnityEngine;
using UnityEngine.UIElements;

public class GlowOrbItem : Inhalable
{
    public bool selfPlaced = true;
    private PlaceableItem _placeableItem;
    public int healingAmount = 15;
    [SerializeField] private VfxInstance vfxIn;
    [SerializeField] private VfxInstance vfxOut;
    [SerializeField] private Transform vfxSpawn;

    protected override void Start()
    {
        base.Start();
        // exists only at night
        if (!selfPlaced)
        {
            if(TimeManager.Instance.IsDayTime)
            {
                Remove();
            } else
            {
                OrbIndicator.Instance.AddCandidate(transform);
                if (vfxIn)
                {
                    VfxInstance.Spawn(vfxIn, vfxSpawn.position, Quaternion.identity);
                }
            }
        }
    }

    private void Awake()
    {
        _placeableItem = GetComponent<PlaceableItemReference>().Item();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!selfPlaced && TimeManager.Instance.IsDayTime)
        {
            if (vfxOut)
            {
                VfxInstance.Spawn(vfxOut, vfxSpawn.position, Quaternion.identity);
            }
            Remove();
        }
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