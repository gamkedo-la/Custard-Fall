using System;
using System.Collections;
using System.Collections.Generic;
using Custard;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using Random = UnityEngine.Random;

// TODO use FullmoonFraction at fullmoon
public class ResourcesGenerator : MonoBehaviour
{
    [SerializeField] private Tidesmanager tidesmanager;
    [SerializeField] private Player player;
    [SerializeField] private WorldCells worldCells;
    [SerializeField] private CustardState _custardState;
    private TimeManager _timeManager;
    
    [SerializeField] private List<SpreadItemDefinition> resourceDensityIn16X16 = new List<SpreadItemDefinition>();

    private readonly Dictionary<String, Queue<WorldItem>> _generatedItemsPool =
        new Dictionary<string, Queue<WorldItem>>();

    private Dictionary<String, HashSet<WorldItem>>[,] _chunks;

    private const int NumberOfChunksX = WorldCells.BlocksWidth / 16;
    private const int NumberOfChunksY = WorldCells.BlocksHeight / 16;

    private bool _canBeInitiated = false;
    private bool _isFullMoon = false;

    private HashSet<Coords> _activeChunks = new HashSet<Coords>();

    // Start is called before the first frame update
    void Start()
    {
        InitItemsPool();

        StartCoroutine(CoroutineFirstFillUpItems());
    }

    private IEnumerator CoroutineFirstFillUpItems()
    {
        yield return new WaitForSeconds(1);
        _canBeInitiated = true;
        FillUpItems();
        yield return new WaitForSeconds(4);
        FillUpItems();
    }

    private void Awake()
    {
        _timeManager = TimeManager.Instance;
        TimeManager.onMorningStarted += (sender, arg) => FillUpItems();
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
                for (int i = 0; i < item.quantityIn16X16 + item.variance * 2; i++)
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

    public void FillUpItems()
    {
        if (!_canBeInitiated)
            return;
        
        Debug.Log("filling up items!");
        TriggerHouseKeeping();

        for (int chunkX = 0; chunkX < NumberOfChunksX; chunkX++)
        for (int chunkY = 0; chunkY < NumberOfChunksY; chunkY++)
        {
            FillUpItemsForChunk(chunkX, chunkY);
        }

        foreach (var itemDefinition in resourceDensityIn16X16)
        {
            if (itemDefinition.oneTime)
                itemDefinition.spawnAgain = false;
        }

        UpdateActiveItems(true);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void TriggerHouseKeeping()
    {
        if (!_canBeInitiated)
            return;

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

    private void FillUpItemsForChunk(int chunkX, int chunkY)
    {
        Dictionary<string, HashSet<WorldItem>> chunk = _chunks[chunkX, chunkY];
        foreach (var item in resourceDensityIn16X16)
        {
            if(!item.spawnAgain)
                continue;

            // we accept at least the fraction
            int minAcceptableAmount =
                Mathf.CeilToInt((item.quantityIn16X16 + Mathf.Round(Random.value * 2 * item.variance - item.variance)) *
                                item.fractionNormalDay);
            HashSet<WorldItem> itemsInChunk;
            if (!chunk.TryGetValue(item.Id(), out itemsInChunk))
            {
                itemsInChunk = new HashSet<WorldItem>();
                chunk.Add(item.Id(), itemsInChunk);
            }

            var randomSeed = Random.value*100;
            var availableItemsInPool = _generatedItemsPool.GetValueOrDefault(item.Id());
            for (int i = itemsInChunk.Count; i < minAcceptableAmount; i++)
            {
                if (availableItemsInPool.TryDequeue(out WorldItem poolItem))
                {
                    PlaceItemRandomized(chunkX, chunkY, poolItem, i, minAcceptableAmount, randomSeed, item);
                    itemsInChunk.Add(poolItem);
                }
            }
        }
    }

    private void PlaceItemRandomized(int chunkX, int chunkY, WorldItem poolItem, int i, int total, float seed,
        SpreadItemDefinition itemDefinition)
    {
        poolItem.Reset();

        var numTrials = 3;
        for (int j = 0; j < numTrials; j++)
        {
            var noiseX = Mathf.PerlinNoise(i * 16 * 2 + Random.value + chunkX + seed, chunkX * 2 - i + seed);
            var noiseY = Mathf.PerlinNoise(i * 16 * 2 + Random.value + chunkY + seed, chunkY * 2 - i + seed);
            var cellX = Mathf.RoundToInt((chunkX + noiseX) * 15);
            var cellY = Mathf.RoundToInt((chunkY + noiseY) * 15);
            var coords = Coords.Of(cellX, cellY);
            
            if (WorldCells.IsOutOfBounds(coords))
                continue;
            // some items can only spawn in custard
            var currentCustardLevel = _custardState.GetCurrentCustardLevelAt(coords);
            if (currentCustardLevel < itemDefinition.minCustardLevel || currentCustardLevel > itemDefinition.maxCustardLevel)
                continue;

            var worldPosition = WorldCells.GetWorldPosition(cellX, cellY);
            worldPosition += new Vector2(Random.value - .5f, Random.value - .5f);


            var newPosition = new Vector3(worldPosition.x, worldCells.GetTerrainHeightAt(cellX, cellY) + .35f,
                worldPosition.y);
            var go = ((MonoBehaviour) poolItem).gameObject;
            go.transform.position = newPosition;

            // TODO delete debug line
            Debug.DrawRay(newPosition, Vector3.up * 10, Color.gray, 30);

            return;
        }
    }

    private void FixedUpdate()
    {
        UpdateActiveItems(false);
    }

    public void UpdateActiveItems(bool force)
    {
        if (!_canBeInitiated)
            return;

        if (force)
            _activeChunks.Clear();

        var cellPosition = worldCells.GetCellPosition(player.transform.position);
        var chunkX = cellPosition.X / 16;
        var chunkY = cellPosition.Y / 16;

        HashSet<Coords> currentChunks = new HashSet<Coords>();

        var halfWindowSize = 2;
        for (int x = -halfWindowSize; x < halfWindowSize; x++)
        for (int y = -halfWindowSize; y < halfWindowSize; y++)
        {
            var currentChunkX = chunkX - x;
            var currentChunkY = chunkY - y;
            if (currentChunkX is >= 0 and < NumberOfChunksX && currentChunkY is >= 0 and < NumberOfChunksY)
            {
                currentChunks.Add(Coords.Of(currentChunkX, currentChunkY));
            }
        }

        ActivateChunks(currentChunks);
    }

    private void ActivateChunks(HashSet<Coords> currentChunks)
    {
        // deactivate old, not part of current chunks
        foreach (var activeChunk in _activeChunks)
        {
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
            if (_activeChunks.Contains(currentChunk))
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

        _activeChunks = currentChunks;
    }

    [Serializable]
    public class SpreadItemDefinition
    {
        public InhaleListener prefab;
        public int quantityIn16X16;
        public int variance;
        public bool oneTime;
        [NonSerialized]
        public bool spawnAgain = true;
        

        public string internalName;

        public float fractionNormalDay = .7f;
        public float fractionFullmoon = 1f;
        public int minCustardLevel = 1;
        public int maxCustardLevel = 42;

        public string Id()
        {
            return string.IsNullOrEmpty(internalName) ? prefab.name : internalName;
        }
    }
    
}