using System;
using Unity.VisualScripting;
using UnityEngine;


public class GrapplingHookCollectible : Inhalable
{

    [SerializeField]
    private Player _player;

    [SerializeField] private GameObject _grappleBtnUi; 
    
    public override void Init()
    {
        base.Init();

        AddToInhaleQueue(new Resource("trusty grappling hook", null), .8f);
    }

    public override void OnResourceInhaledAndMaybeRemove(Inhaler inhaler, Resource resource, int amount)
    {
        _player.ownsGrapplingHook = true;
        _grappleBtnUi.SetActive(true);
        gameObject.SetActive(false);
    }
}