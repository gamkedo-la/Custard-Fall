﻿using System;
using Unity.VisualScripting;
using UnityEngine;


public class SmallCreamyConeTree : InhaleListener
{
    public override void Init()
    {
        base.Init();

        AddToInhaleQueue(new Resource("Creamy Cone Tree"), .6f);
    }

    public override void OnResourceInhaled(Inhaler inhaler, Resource resource, int amount)
    {
        base.OnResourceInhaled(inhaler, resource, amount);
        if (GetRemainingResourcesCount() == 0)
        {
            gameObject.SetActive(false);
            Debug.Log("got the creamy cone tree");
        }
    }
}