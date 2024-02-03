using System;
using System.Collections.Generic;
using UnityEngine;

public class InhalableFloatingOrb : Inhalable
{
    public static EventHandler onInhale;
    public static HashSet<GameObject> allCurrentDynamicInhaleables = new();
    [SerializeField] private GameObject plusOnePrefab;


    void OnPickup()
    {
        onInhale?.Invoke(this, null);
        if (plusOnePrefab)
        {
            var transformPosition = transform.position;
            Instantiate(plusOnePrefab,
                new Vector3(transformPosition.x, transformPosition.y + 1.5f, transformPosition.z),
                Quaternion.Euler(-90f, 0f, 0f));
        }

        Destroy(gameObject);
    }

    public override void OnResourceInhaledAndMaybeRemove(Inhaler inhaler, Resource resource, int amount)
    {
        if (GetRemainingResourcesCount() == 0)
        {
            gameObject.SetActive(false);
            OnPickup();
        }
    }

    public override void Init()
    {
        allCurrentDynamicInhaleables.Add(gameObject);
    }

    void OnDestroy()
    {
        allCurrentDynamicInhaleables.Remove(gameObject);
    }
}