using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOnHit : MonoBehaviour
{
    public int damage = 15;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("hit...");
        var damageTaker = other.GetComponentInParent<DamageTaker>();
        if (damageTaker)
        {
            Debug.Log($"damage hit {damage}");
            damageTaker.TakeDamage(damage);
            RemoveSelf();
        }
    }

    private void RemoveSelf()
    {
        Destroy(gameObject);
    }
}