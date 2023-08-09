using System;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;


public class GrapplingHookCollectible : Inhalable
{

    [SerializeField]
    private Player _player;

    [SerializeField] private GameObject _grappleBtnUi;
    [SerializeField] private GameObject plusOnePrefab;

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
        
        if (plusOnePrefab)
            Instantiate(plusOnePrefab,
                new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z),
                Quaternion.Euler(-90f, 0f, 0f));
    }
}