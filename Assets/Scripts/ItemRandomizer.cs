using System;
using System.Collections;
using System.Collections.Generic;
using Custard;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

// TODO use FullmoonFraction at fullmoon
public class ItemRandomizer : MonoBehaviour
{
    [SerializeField] private Tidesmanager tidesmanager;
    [SerializeField] private Player player;
    [SerializeField] private WorldCells worldCells;
    [SerializeField] private CustardState _custardState;
    private TimeManager _timeManager;

    [FormerlySerializedAs("resourceDensityIn16X16")] [SerializeField]
    private List<RandomizedItem> randomizedItems = new List<RandomizedItem>();

    private readonly Dictionary<RandomizedItem, Queue<WorldItem>> _generatedItemsPool =
        new Dictionary<RandomizedItem, Queue<WorldItem>>();

    private Dictionary<RandomizedItem, HashSet<WorldItem>>[,] _chunks;

    private const int NumberOfChunksX = WorldCells.BlocksWidth / 16;
    private const int NumberOfChunksY = WorldCells.BlocksHeight / 16;

    private bool _canBeInitiated = false;

    private HashSet<Coords> _activeChunks = new HashSet<Coords>();
    private Coords _playerChunk = Coords.Of(255, 255);
    [SerializeField] private int itemsDisplayChunkRadius = 3;

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
        FillUpRegularItems();
    }

    private void Awake()
    {
        _timeManager = TimeManager.Instance;
        TimeManager.onMorningStarted += (sender, arg) => { FillUpRegularItems(); };
        TimeManager.onNightStarted += (sender, arg) => { StartCoroutine(FillUpNightItems()); };
    }

    public void FillUpRegularItems()
    {
        List<RandomizedItem> nonSpecialtemTypes = new List<RandomizedItem>();
        foreach (var itemType in randomizedItems)
        {
            if (!itemType.spawnAtNight)
                nonSpecialtemTypes.Add(itemType);
        }

        FillUpItems(nonSpecialtemTypes);
    }

    private IEnumerator FillUpNightItems()
    {
        List<RandomizedItem> nightItemTypes = new List<RandomizedItem>();
        foreach (var itemType in randomizedItems)
        {
            if (itemType.spawnAtNight)
                nightItemTypes.Add(itemType);
        }

        if (nightItemTypes.Count != 0)
        {
            yield return new WaitForSeconds(5f + Random.value * 16);
            FillUpItems(nightItemTypes);
        }
    }

    private void InitItemsPool()
    {
        _chunks = new Dictionary<RandomizedItem, HashSet<WorldItem>>[NumberOfChunksX, NumberOfChunksY];

        for (int chunkX = 0; chunkX < NumberOfChunksX; chunkX++)
        for (int chunkY = 0; chunkY < NumberOfChunksY; chunkY++)
        {
            // init dictionary for later
            var chunks = new Dictionary<RandomizedItem, HashSet<WorldItem>>();
            _chunks[chunkX, chunkY] = chunks;
            // create sufficient items for the item pool
            foreach (var item in randomizedItems)
            {
                for (int i = 0; i < item.quantityIn16X16 + item.variance * 2; i++)
                {
                    var instantiated = Instantiate(item.prefab);
                    var glowOrbItem = instantiated.GetComponent<GlowOrbItem>();
                    if (glowOrbItem)
                    {
                        // TODO refactor (generalize)
                        glowOrbItem.GetComponent<GlowOrbItem>().selfPlaced = false;
                    }

                    instantiated.SetUsedUp(true);
                    instantiated.gameObject.SetActive(false);
                    if (!_generatedItemsPool.TryGetValue(item, out var sameTypeItems))
                    {
                        sameTypeItems = new Queue<WorldItem>();
                        _generatedItemsPool.Add(item, sameTypeItems);
                    }

                    sameTypeItems.Enqueue(instantiated);
                }
            }
        }

        Debug.Log("item pool initialized");
    }

    private void FillUpItems(List<RandomizedItem> itemTypes)
    {
        if (!_canBeInitiated)
            return;
        Debug.Log("filling up items!");

        DoChunkHouseKeeping();
        for (int chunkX = 0; chunkX < NumberOfChunksX; chunkX++)
        for (int chunkY = 0; chunkY < NumberOfChunksY; chunkY++)
        {
            FillUpItemsForChunk(chunkX, chunkY, itemTypes);
        }

        foreach (var itemDefinition in itemTypes)
        {
            if (itemDefinition.oneTime)
                itemDefinition.spawnAgain = false;
        }

        UpdateActiveItems(true);
    }

    public void DoChunkHouseKeeping()
    {
        if (!_canBeInitiated)
            return;

        for (int chunkX = 0; chunkX < NumberOfChunksX; chunkX++)
        for (int chunkY = 0; chunkY < NumberOfChunksY; chunkY++)
        {
            var chunkItems = _chunks[chunkX, chunkY];
            HashSet<WorldItem> removed = new HashSet<WorldItem>();
            foreach (var occupiedItemsById in chunkItems)
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

    private void FillUpItemsForChunk(int chunkX, int chunkY, List<RandomizedItem> itemTypes)
    {
        var chunkThePlayerIsIn = peekCurrentPlayerChunk();
        Dictionary<RandomizedItem, HashSet<WorldItem>> chunk = _chunks[chunkX, chunkY];
        foreach (var item in itemTypes)
        {
            if (!item.spawnAgain || item.dontSpawnNearPlayer && (chunkX == chunkThePlayerIsIn.X &&
                                                                 chunkY == chunkThePlayerIsIn.Y ||
                                                                 Vector3.Distance(
                                                                     new Vector2(player.transform.position.x,
                                                                         player.transform.position.z),
                                                                     worldCells.GetWorldPosition(chunkX, chunkY)) <
                                                                 item.minDistanceToPlayer))
                continue;

            // minAcceptableAmount for this round; we try to fill up complying to definition
            int minAcceptableAmount =
                Mathf.RoundToInt(item.quantityIn16X16 + Mathf.Round((Random.value - .5f) * 2 * item.variance));
            if (minAcceptableAmount <= 0)
                continue;

            HashSet<WorldItem> itemsInChunk;
            if (!chunk.TryGetValue(item, out itemsInChunk))
            {
                itemsInChunk = new HashSet<WorldItem>();
                chunk.Add(item, itemsInChunk);
            }

            var randomSeed = Random.value * 16;
            var availableItemsInPool = _generatedItemsPool.GetValueOrDefault(item);
            // fill up
            for (int i = itemsInChunk.Count; i < minAcceptableAmount; i++)
            {
                if (Random.value > getFraction(item))
                    continue;

                if (availableItemsInPool.TryDequeue(out WorldItem poolItem))
                {
                    if (PlaceItemRandomized(chunkX, chunkY, poolItem, i, minAcceptableAmount, randomSeed, item))
                    {
                        itemsInChunk.Add(poolItem);
                    }
                    else
                    {
                        availableItemsInPool.Enqueue(poolItem);
                    }
                }
            }
        }
    }

    private static float getFraction(RandomizedItem item)
    {
        return item.fractionNormalDay;
    }

    private bool PlaceItemRandomized(int chunkX, int chunkY, WorldItem poolItem, int i, int total, float seed,
        RandomizedItem item)
    {
        // Calling Reset is important before spawning item from pool somewhere in the world!
        poolItem.Reset();

        for (int j = 0; j < item.numberRetries; j++)
        {
            var noiseX =
                Mathf.Clamp(Mathf.PerlinNoise(i * 16 * 3 + Random.value * 8 + chunkX + seed, chunkX * 2 - i) * 2, 0f,
                    1f);
            var noiseY =
                Mathf.Clamp(Mathf.PerlinNoise(i * 16 * 3 + Random.value * 8 + chunkY + seed, chunkY * 2 - i) * 2, 0f,
                    1f);
            var cellX = Mathf.RoundToInt((chunkX + noiseX) * 15);
            var cellY = Mathf.RoundToInt((chunkY + noiseY) * 15);
            var coords = Coords.Of(cellX, cellY);

            if (WorldCells.IsOutOfBounds(coords))
                continue;

            // dont spawn on other items
            if (worldCells.GetWorldItemHeightAt(coords) != 0)
                continue;
            // some items can only spawn in custard
            var currentCustardLevel = _custardState.GetCurrentCustardLevelAt(coords);
            if (currentCustardLevel < item.minCustardLevel ||
                currentCustardLevel > item.maxCustardLevel)
                continue;
            var terrainHeight = worldCells.GetTerrainHeightAt(cellX, cellY);
            if (terrainHeight < item.minTerrainLevel ||
                terrainHeight > item.maxTerrainLevel)
                continue;

            var worldPosition = worldCells.GetWorldPosition(cellX, cellY);
            worldPosition += new Vector2(Random.value - .5f, Random.value - .5f) * .5f;

            var newPosition = new Vector3(worldPosition.x, terrainHeight + .35f,
                worldPosition.y);
            var go = ((MonoBehaviour) poolItem).gameObject;
            go.transform.position = newPosition;

            // TODO delete debug line
            Debug.DrawRay(newPosition, Vector3.up * 10, Color.gray, 60);

            return true;
        }

        return false;
    }

    private void FixedUpdate()
    {
        UpdateActiveItems(false);
    }

    public void UpdateActiveItems(bool force)
    {
        if (!_canBeInitiated)
            return;

        var currentPlayerChunk = peekCurrentPlayerChunk();

        if (force)
            _activeChunks.Clear();
        else if (currentPlayerChunk.X == _playerChunk.X && currentPlayerChunk.Y == _playerChunk.Y)
            // still same chunk
            return;

        _playerChunk = currentPlayerChunk;
        HashSet<Coords> currentChunks = new HashSet<Coords>();
        var halfWindowSize = itemsDisplayChunkRadius;
        for (int x = -halfWindowSize; x < halfWindowSize; x++)
        for (int y = -halfWindowSize; y < halfWindowSize; y++)
        {
            var currentChunkX = _playerChunk.X - x;
            var currentChunkY = _playerChunk.Y - y;
            if (currentChunkX is >= 0 and < NumberOfChunksX && currentChunkY is >= 0 and < NumberOfChunksY)
            {
                currentChunks.Add(Coords.Of(currentChunkX, currentChunkY));
            }
        }

        ActivateChunks(currentChunks);
    }

    private Coords peekCurrentPlayerChunk()
    {
        var cellPosition = worldCells.GetCellPosition(player.transform.position);
        var chunkX = cellPosition.X / 16;
        var chunkY = cellPosition.Y / 16;
        var currentPlayerChunk = Coords.Of(chunkX, chunkY);
        return currentPlayerChunk;
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
    public class RandomizedItem
    {
        public Inhalable prefab;
        public int quantityIn16X16;
        public int variance;
        public bool oneTime;
        [NonSerialized] public bool spawnAgain = true;

        public bool spawnAtNight = false;


        private string internalName;

        public float fractionNormalDay = .7f;
        public float fractionFullmoon = 1f;
        public int minCustardLevel = 1;
        public int maxCustardLevel = 42;
        public int minTerrainLevel = 0;
        public int maxTerrainLevel = 14;
        public bool dontSpawnNearPlayer = false;
        public float minDistanceToPlayer = 0f;
        public int numberRetries = 3;

        public string Id()
        {
            return string.IsNullOrEmpty(internalName) ? prefab.name : internalName;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj) || obj != null && obj.GetType()
                == typeof(RandomizedItem) && ((RandomizedItem) obj).Id() == Id();
        }

        public override int GetHashCode()
        {
            return Id().GetHashCode();
        }
    }
}