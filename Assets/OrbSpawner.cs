using System;
using System.Collections;
using System.Collections.Generic;
using Custard;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class OrbSpawner : MonoBehaviour
{
    [SerializeField] private GameObject healthOrbPrefab;
    [SerializeField] private GameObject radianceOrbPrefab;
    [SerializeField] private float spawnHeight = 1.5f;

    [FormerlySerializedAs("timeBetweenRadianceOrbsChecks")] [SerializeField]
    private float timeBetweenOrbsChecks = 5f;

    [SerializeField] private float despawnDistance = 40f;
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private float spawnInnerRadius = 6f;
    [SerializeField] private int spawnRetries = 20;
    [SerializeField] private int maxConcurrentOrbs = 7;

    private readonly List<InhalableFloatingOrb> _spawnedOrbs = new List<InhalableFloatingOrb>();

    private Player _player;
    [SerializeField] private WorldCells worldCells;
    [SerializeField] private CustardState custardState;
    [SerializeField] private float usualHealthToRadianceFraction = .4f;
    [SerializeField] private float emergencyHealthToRadianceFraction = .8f;

    [SerializeField] private float radiusOffsetInMoveDirection = 10f;

    void Start()
    {
        _player = FindObjectOfType<Player>();
        if (!_player)
        {
            Debug.Log("No player found!");
        }

        if (!worldCells)
        {
            Debug.Log("No WorldCells found!");
        }


        if (!custardState)
        {
            Debug.Log("No CustardState found!");
        }

        StartCoroutine(SpawnRadianceOrbs());
        // StartCoroutine(SpawnHealthOrbs());
        StartCoroutine(Cleanup());
    }

    private IEnumerator Cleanup()
    {
        while (true)
        {
            yield return new WaitForSeconds(10);
            var playerPosition = _player.gameObject.transform.position;
            var playerPositionVector = new Vector2(playerPosition.x, playerPosition.z);

            List<GameObject> tobeDestroyed = new List<GameObject>();
            _spawnedOrbs.RemoveAll(orb =>
            {
                bool delete = false;
                if (!orb)
                {
                    delete = true;
                }
                else
                {
                    var position = orb.transform.position;
                    delete = orb.IsUsedUp() ||
                             Vector2.Distance(new Vector2(position.x, position.z), playerPositionVector) >
                             despawnDistance;
                    if (delete)
                    {
                        tobeDestroyed.Add(orb.gameObject);
                    }
                }

                return delete;
            });
            foreach (var orb in tobeDestroyed)
            {
                Destroy(orb);
            }
        }
    }

    private IEnumerator SpawnRadianceOrbs()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenOrbsChecks);

            if (_spawnedOrbs.Count > maxConcurrentOrbs)
                continue;

            int retries = spawnRetries;
            var playerPosition = _player.gameObject.transform.position;


            var moveDirection = _player.transform.forward;
            var spawnCenter = new Vector2(playerPosition.x, playerPosition.z) +
                              new Vector2(moveDirection.x, moveDirection.y) * radiusOffsetInMoveDirection;
            for (; retries > 0; retries--)
            {
                var randVector = Random.insideUnitCircle * spawnRadius;
                if (randVector.magnitude < spawnInnerRadius)
                    continue;

                var spawnPos = randVector + spawnCenter;
                var cellPosition = worldCells.GetCellPosition(spawnPos);
                if (custardState.GetCurrentCustardLevelAt(cellPosition) == 1)
                {
                    var height = worldCells.GetHeightAt(cellPosition) + spawnHeight;
                    var position = new Vector3(spawnPos.x, height, spawnPos.y);

                    SpawnMostNeededOrb(position, out var spawnedOrb);

                    break;
                }
            }
        }
    }

    private void SpawnMostNeededOrb(Vector3 position, out GameObject spawnedOrb)
    {
        if (_player.currentHealth < _player.maxHealth * 2.0f / 3 && Random.value <= emergencyHealthToRadianceFraction ||
            _player.currentHealth < _player.maxHealth && Random.value <= usualHealthToRadianceFraction)
        {
            spawnedOrb = Instantiate(healthOrbPrefab, position, Quaternion.identity);
            _spawnedOrbs.Add(spawnedOrb.GetComponent<InhalableFloatingOrb>());
        }
        else
        {
            spawnedOrb = Instantiate(radianceOrbPrefab, position, Quaternion.identity);
            _spawnedOrbs.Add(spawnedOrb.GetComponent<InhalableFloatingOrb>());
        }
    }
}