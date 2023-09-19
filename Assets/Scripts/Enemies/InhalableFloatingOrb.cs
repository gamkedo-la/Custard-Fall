using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class InhalableFloatingOrb : Inhalable
{
    public static EventHandler onInhale;
    public static HashSet<GameObject> allCurrentDynamicInhaleables = new();
    [SerializeField] private GameObject plusOnePrefab;


    void OnPickup()
    {
        Debug.Log("picked up orb");
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
        Debug.Log("inhaled orb");

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
        Debug.Log($"remaining orbs in Level {allCurrentDynamicInhaleables.Count}");
    }
}