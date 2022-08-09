using System;
using Unity.VisualScripting;
using UnityEngine;


public class LargeCreamyConeTree : InhaleListener
{
    public override void Init()
    {
        base.Init();

        AddToInhaleQueue(new Resource("Creamy Cone Tree"), 1.5f);
        AddToInhaleQueue(new Resource("Creamy Cone Tree"), 1.5f);
        AddToInhaleQueue(new Resource("Creamy Cone Tree"), 1.5f);
        AddToInhaleQueue(new Resource("Creamy Cone Tree"), 2f);
        AddToInhaleQueue(new Resource("Creamy Cone Tree"), 3f);
    }

    public override void OnResourceInhaled(Inhaler inhaler, Resource resource, int amount)
    {
        base.OnResourceInhaled(inhaler, resource, amount);
        if (GetRemainingResourcesCount() == 0)
        {
            gameObject.SetActive(false);
            Debug.Log("got large creamy cone tree");
        }
    }
}