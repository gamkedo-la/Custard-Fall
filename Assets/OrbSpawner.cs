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
    [SerializeField] private float spawnHeight = 1f;
    [SerializeField] private float timeBetweenHealthOrbsChecks = 5f;
    [SerializeField] private float timeBetweenRadianceOrbsChecks = 5f;
    [SerializeField] private float despawnDistance = 40f;
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private float spawnInnerRadius = 6f;
    [SerializeField] private int spawnRetries = 20;
    [SerializeField] private int maxConcurrentOrbs = 7;

    private readonly List<InhalableFloatingOrb> _spawnOrbs = new List<InhalableFloatingOrb>();

    private Player _player;
    [SerializeField] private WorldCells worldCells;
    [SerializeField] private CustardState custardState;

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
            _spawnOrbs.RemoveAll(orb =>
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
            yield return new WaitForSeconds(timeBetweenRadianceOrbsChecks);

            if (_spawnOrbs.Count > maxConcurrentOrbs)
                continue;

            int retries = spawnRetries;
            var playerPosition = _player.gameObject.transform.position;
            var playerPositionVector = new Vector2(playerPosition.x, playerPosition.z);
            for (; retries > 0; retries--)
            {
                var randVector = Random.insideUnitCircle * spawnRadius;
                if (randVector.magnitude < spawnInnerRadius)
                    continue;

                var spawnPos = randVector + playerPositionVector;
                var cellPosition = worldCells.GetCellPosition(spawnPos);
                if (custardState.GetCurrentCustardLevelAt(cellPosition) == 1)
                {
                    var height = worldCells.GetHeightAt(cellPosition) + spawnHeight;
                    var position = new Vector3(spawnPos.x, height, spawnPos.y);
                    var spawnedOrb = Instantiate(radianceOrbPrefab, position, Quaternion.identity);
                    var inhalableFloatingOrb = spawnedOrb.GetComponent<InhalableFloatingOrb>();
                    _spawnOrbs.Add(inhalableFloatingOrb);
                    break;
                }
            }
        }
    }

    private IEnumerator SpawnHealthOrbs()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenHealthOrbsChecks);
        }
    }
}