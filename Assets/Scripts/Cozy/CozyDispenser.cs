using System;
using System.Collections.Generic;
using UnityEngine;

public class CozyDispenser : MonoBehaviour
{
    [SerializeField] private float coziness;

    private CozinessManager cozinessManager;

    public float Coziness
    {
        get => coziness;
        set => coziness = value;
    }


    private void Start()
    {
        cozinessManager = CozinessManager.Instance;
        cozinessManager.RegisterDispenser(this);
    }

    private void OnEnable()
    {
        if (cozinessManager)
            cozinessManager.RegisterDispenser(this);
    }

    private void OnDisable()
    {
        if (cozinessManager)
            cozinessManager.UnregisterDispenser(this);
    }
}