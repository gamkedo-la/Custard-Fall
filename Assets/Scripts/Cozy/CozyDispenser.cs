using System;
using System.Collections.Generic;
using UnityEngine;

public class CozyDispenser : MonoBehaviour
{
    [SerializeField] private float coziness;
    [SerializeField] private int maxEffective;

    private CozinessManager cozinessManager;

    public float Coziness => coziness;


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