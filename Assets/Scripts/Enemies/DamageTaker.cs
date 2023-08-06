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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int f)
    {
        player.TakeDamage(f, DamageImplication.Health);
    }
}
