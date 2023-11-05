using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTaker : MonoBehaviour
{

    private Player player;
    void Awake()
    {
        player = GetComponent<Player>();
    }

    public void TakeDamage(int f)
    {
        player.TakeDamage(f, DamageImplication.Health);
    }
}
