using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RadianceDispenser : MonoBehaviour
{
    [FormerlySerializedAs("coziness")] [SerializeField] private float radiance;

    private RadianceManager _radianceManager;
    
    public EventHandler<bool> onActivated;

    public float Radiance
    {
        get => radiance;
        set => radiance = value;
    }


    private void Start()
    {
        _radianceManager = RadianceManager.Instance;
        _radianceManager.RegisterDispenser(this);
    }

    private void OnEnable()
    {
        if (_radianceManager)
            _radianceManager.RegisterDispenser(this);
    }

    private void OnDisable()
    {
        if (_radianceManager)
            _radianceManager.UnregisterDispenser(this);
    }
}