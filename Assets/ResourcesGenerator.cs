using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ResourcesGenerator : MonoBehaviour
{
    [SerializeField] private Tidesmanager tidesmanager;
    [SerializeField] private Player player;
    [SerializeField] private WorldCells worldCells;
    private TimeManager _timeManager;

    [SerializeField] private float fractionNormalDay = .3f;
    [SerializeField] private float fractionFullmoon = .8f;
    [SerializeField] private List<SpreadItem> resourceDensityIn16X16 = new List<SpreadItem>();

    private readonly Dictionary<String, Queue<WorldItem>> _generatedItemsPool =
        new Dictionary<string, Queue<WorldItem>>();

    private Dictionary<String, HashSet<WorldItem>>[,] _chunks;

    private const int NumberOfChunksX = WorldCells.BlocksWidth / 16;
    private const int NumberOfChunksY = WorldCells.BlocksHeight / 16;

    private HashSet<Coords> activeChunks = new HashSet<Coords>();

    // Start is called before the first frame update
    void Start()
    {
        InitItemsPool();

        StartCoroutine(CoroutineFirstFillUpItems());
    }
    
    private IEnumerator CoroutineFirstFillUpItems()
    {
        yield return new WaitForSeconds(1);
            FillUpItems(fractionNormalDay);
            UpdateActiveItems();
    }

    private void Awake()
    {
        _timeManager = TimeManager.Instance;
        TimeManager.onMorningStarted += (sender, arg) => FillUpItems(fractionNormalDay);
    }

    private void InitItemsPool()
    {
        _chunks = new Dictionary<String, HashSet<WorldItem>>[NumberOfChunksX, NumberOfChunksY];

        for (int chunkX = 0; chunkX < NumberOfChunksX; chunkX++)
        for (int chunkY = 0; chunkY < NumberOfChunksY; chunkY++)
        {
            // init dictionary for later
            var chunks = new Dictionary<String, HashSet<WorldItem>>();
            _chunks[chunkX, chunkY] = chunks;
            // create sufficient items for the item pool
            foreach (var item in resourceDensityIn16X16)
            {
                for (int i = 0; i < item.quantityIn16X16 + item.variance; i++)
                {
                    var instantiated = GameObject.Instantiate(item.prefab);
                    instantiated.SetUsedUp(true);
                    instantiated.gameObject.SetActive(false);
                    if (!_generatedItemsPool.TryGetValue(item.Id(), out var sameTypeItems))
                    {
                        sameTypeItems = new Queue<WorldItem>();
                        _generatedItemsPool.Add(item.Id(), sameTypeItems);
                    }

                    sameTypeItems.Enqueue(instantiated);
                }
            }
        }

        Debug.Log("item pool initialized");
    }

    public void FillUpItems(float fraction)
    {
        Debug.Log("filling up items!");
        TriggerHouseKeeping();

        for (int chunkX = 0; chunkX < NumberOfChunksX; chunkX++)
        for (int chunkY = 0; chunkY < NumberOfChunksY; chunkY++)
        {
            FillUpItemsForChunk(chunkX, chunkY, fraction);
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void TriggerHouseKeeping()
    {
        for (int chunkX = 0; chunkX < NumberOfChunksX; chunkX++)
        for (int chunkY = 0; chunkY < NumberOfChunksY; chunkY++)
        {
            var chunk = _chunks[chunkX, chunkY];
            HashSet<WorldItem> removed = new HashSet<WorldItem>();
            foreach (var occupiedItemsById in chunk)
            {
                var occupiedItems = occupiedItemsById.Value;
                foreach (var occupiedItem in occupiedItems)
                {
                    if (occupiedItem.IsUsedUp())
                    {
                        removed.Add(occupiedItem);
                        _generatedItemsPool[occupiedItemsById.Key].Enqueue(occupiedItem);
                    }
                }

                var removeWhereCount = occupiedItems.RemoveWhere(m => m.IsUsedUp());
                if (removed.Count != removeWhereCount)
                    Debug.Log("Some items could not be removed from chunk");
            }
        }
    }

    private void FillUpItemsForChunk(int chunkX, int chunkY, float fraction)
    {
        Dictionary<string, HashSet<WorldItem>> chunk = _chunks[chunkX, chunkY];
        foreach (var item in resourceDensityIn16X16)
        {
            // we accept at least the fraction
            int minAcceptableAmount =
                Mathf.CeilToInt((item.quantityIn16X16 + Mathf.Round(Random.value * 2 * item.variance - item.variance)) *
                                fraction);
            HashSet<WorldItem> itemsInChunk;
            if (!chunk.TryGetValue(item.Id(), out itemsInChunk))
            {
                itemsInChunk = new HashSet<WorldItem>();
                chunk.Add(item.Id(),itemsInChunk);
            }

            var availableItemsInPool = _generatedItemsPool.GetValueOrDefault(item.Id());
            for (int i = itemsInChunk.Count; i < minAcceptableAmount; i++)
            {
                if (availableItemsInPool.TryDequeue(out WorldItem poolItem))
                {
                    PlaceItemRandomized(chunkX, chunkY, poolItem, i, minAcceptableAmount);
                    itemsInChunk.Add(poolItem);
                }
            }
        }
    }

    private void PlaceItemRandomized(int chunkX, int chunkY, WorldItem poolItem, int i, int max)
    {
        poolItem.Reset();
        
        var noiseX = Mathf.PerlinNoise(i/(float)max, chunkX/(float)NumberOfChunksX);
        var noiseY = Mathf.PerlinNoise(i/(float)max, chunkY/(float)NumberOfChunksY);
        var cellX = Mathf.RoundToInt((chunkX +noiseX) * 15);
        var cellY = Mathf.RoundToInt((chunkY + noiseY) * 15);
        var worldPosition = WorldCells.GetWorldPosition(cellX, cellY);
        worldPosition += new Vector2((noiseX - .5f)*.9f, (noiseY - .5f)*.9f);
        var newPosition = new Vector3(worldPosition.x, worldCells.GetTerrainHeightAt(cellX, cellY) + .35f, worldPosition.y);
        ((MonoBehaviour) poolItem).gameObject.transform.position = newPosition;
        
        // TODO delete debug line
        Debug.DrawRay(newPosition, Vector3.up * 10, Color.gray, 240);
    }

    private void FixedUpdate()
    {
        UpdateActiveItems();
    }

    public void UpdateActiveItems()
    {
        var cellPosition = worldCells.GetCellPosition(player.transform.position);
        var chunkX = cellPosition.X / 16;
        var chunkY = cellPosition.Y / 16;

        HashSet<Coords> currentChunks = new HashSet<Coords>();

        for (int x = -1; x < 1; x++)
        for (int y = -1; y < 1; y++)
            currentChunks.Add(Coords.Of(chunkX - x, chunkY - y));

        ActivateChunks(currentChunks);
    }

    private void ActivateChunks(HashSet<Coords> currentChunks)
    {
        // deactivate old, not part of current chunks
        foreach (var activeChunk in activeChunks)
        {
            if (activeChunk.X is < 0 or >= NumberOfChunksX || activeChunk.Y is < 0 or >= NumberOfChunksX)
                continue;
            
            if (currentChunks.Contains(activeChunk))
                continue;
            
            foreach (var occupiedItems in _chunks[activeChunk.X, activeChunk.Y].Values)
            {
                foreach (var occupiedItem in occupiedItems)
                {
                    ((MonoBehaviour) occupiedItem).gameObject.SetActive(false);
                }
            }
        }

        // deactivate all in current chunks
        foreach (var currentChunk in currentChunks)
        {
            if (currentChunk.X is < 0 or >= NumberOfChunksX || currentChunk.Y is < 0 or >= NumberOfChunksX)
                continue;
            
            if (activeChunks.Contains(currentChunk))
                continue;

            foreach (var occupiedItems in _chunks[currentChunk.X, currentChunk.Y].Values)
            {
                foreach (var occupiedItem in occupiedItems)
                {
                    if (!occupiedItem.IsUsedUp())
                        ((MonoBehaviour) occupiedItem).gameObject.SetActive(true);
                }
            }
        }

        activeChunks = currentChunks;
    }


    [Serializable]
    public class SpreadItem
    {
        public InhaleListener prefab;
        public int quantityIn16X16;
        public int variance;

        public String Id()
        {
            return prefab.name;
        }
    }
}