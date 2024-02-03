using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class InhalableProjectile : Inhalable
{
    public static EventHandler onProjectileInhale;
    public static HashSet<GameObject> allCurrentProjectiles = new();
    [SerializeField] private GameObject plusOnePrefab;


    void OnPickup()
    {
        onProjectileInhale?.Invoke(this, null);
        if (plusOnePrefab)
            Instantiate(plusOnePrefab,
                new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z),
                Quaternion.Euler(-90f, 0f, 0f));
        Destroy(gameObject);
    }

    public override void OnResourceInhaledAndMaybeRemove(Inhaler inhaler, Resource resource, int amount)
    {
        Debug.Log("inhaled projectile");
        if (GetRemainingResourcesCount() == 0)
        {
            gameObject.SetActive(false);
            OnPickup();
        }
    }

    public override void Init()
    {
        allCurrentProjectiles.Add(gameObject);
    }

    void OnDestroy()
    {
        allCurrentProjectiles.Remove(gameObject);
    }
}