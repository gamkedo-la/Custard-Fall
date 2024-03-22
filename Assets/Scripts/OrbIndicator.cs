using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrbIndicator : MonoBehaviour
{

    public static OrbIndicator Instance;
    
    [SerializeField] private VfxInstance indicatorVfx;
    [SerializeField] private Player player;
    [SerializeField] private Camera camera;
    [SerializeField] private float checkRate = 0.4f;
    [SerializeField] private float maxDistance = 64f;
    [SerializeField] private float minDistance = 24f;
    [SerializeField] private float offset = 26f;

    private List<Transform> recentSpawns = new();

    public void AddCandidate(Transform candidate)
    {
        recentSpawns.Add(candidate);
    }

    private void Start()
    {
        StartCoroutine(IndicateSpawn());
        Instance = this;
    }
    private IEnumerator IndicateSpawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkRate);

            var nearestSpawns = FilterNearestSpawns(recentSpawns);
            foreach (var nearestSpawn in nearestSpawns)
            {
                var indicatorPos = GetScreenEdgePosition(nearestSpawn);
                Debug.DrawLine(indicatorPos , indicatorPos + Vector3.up * 20, Color.red, 100000);
                VfxInstance.Spawn(indicatorVfx, indicatorPos, Quaternion.identity);
            }

            recentSpawns.Clear();
        }
    }

    private IEnumerable<Transform> FilterNearestSpawns(List<Transform> spawns)
    {
        var playerPos = player.transform.position;
        var candidates = new List<Tuple<float, Transform>>();
        foreach (var spawn in spawns)
        {
            var distance = Vector3.Distance(playerPos, spawn.position);

            if (distance < minDistance)
            {
                Debug.Log($"nearest spawn too near: {distance}");
                return new List<Transform>();
            }
            
            candidates.Add(new Tuple<float, Transform>(distance, spawn));
            Debug.Log($"nearest spawn candidate has distance of {distance}");
        }

        var nearestSpawns =
            from candidate in candidates
            orderby candidate.Item1
            where candidate.Item1 < maxDistance && candidate.Item1 > minDistance
            select candidate.Item2;
        
        return nearestSpawns;
    }

    private Vector3 GetScreenEdgePosition(Transform nearestSpawn)
    {
        var playerPos = player.transform.position;
        return playerPos + (nearestSpawn.position - playerPos).normalized * offset;
    }
}