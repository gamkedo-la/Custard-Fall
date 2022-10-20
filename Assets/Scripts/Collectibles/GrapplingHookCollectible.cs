using System;
using Unity.VisualScripting;
using UnityEngine;


public class GrapplingHookCollectible : InhaleListener
{

    [SerializeField]
    private Player _player;

    [SerializeField] private GameObject _grappleBtnUi; 
    
    public override void Init()
    {
        base.Init();

        AddToInhaleQueue(new Resource("trusty grappling hook"), .8f);
    }

    public override void OnResourceInhaled(Inhaler inhaler, Resource resource, int amount)
    {
        _player.ownsGrapplingHook = true;
        _grappleBtnUi.SetActive(true);
        gameObject.SetActive(false);
    }
}