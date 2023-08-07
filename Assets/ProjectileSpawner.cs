using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    [SerializeField] private float interval = 1.5f;
    [SerializeField] private float maxLifespan = 5f;
    [SerializeField] private GameObject spawnable;
    [SerializeField] private GameObject spawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ShootPeriodically());
    }

    [ContextMenu("Spawn Projectile")]
    private void SpawnProjectile()
    {
        Instantiate(spawnable, spawnPoint.transform.position, transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private IEnumerator ShootPeriodically()
    {
        while (true)
        {
            SpawnProjectile();
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator KillMe()
    {
        yield return new WaitForSeconds(maxLifespan);
        Destroy(gameObject);
    }
}