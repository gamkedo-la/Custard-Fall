using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    [SerializeField] private float interval = 1.5f;
    [SerializeField] private GameObject spawnable;
    [SerializeField] private GameObject spawnPoint;
    private Coroutine _spawner;
    

    [ContextMenu("Spawn Projectile")]
    private void SpawnProjectile()
    {
        Instantiate(spawnable, spawnPoint.transform.position, transform.rotation);
    }

    private void OnEnable()
    {
        _spawner = StartCoroutine(ShootPeriodically());
    }

    private void OnDisable()
    {
        StopCoroutine(_spawner);
    }

    private IEnumerator ShootPeriodically()
    {
        while (true)
        {
            SpawnProjectile();
            yield return new WaitForSeconds(interval);
        }
    }
}