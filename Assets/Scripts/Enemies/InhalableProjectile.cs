using System;
using System.Collections.Generic;
using UnityEngine;

public class InhalableProjectile : Inhalable
{

    public static EventHandler onProjectileInhale;
    public static HashSet<GameObject> allCurrentProjectiles = new();
    

    void OnPickup()
    {
        Debug.Log("picked up projectile");
        onProjectileInhale?.Invoke(this, null);
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
        Debug.Log($"remaining projectiles in Level {allCurrentProjectiles.Count}");
    }
    
}
