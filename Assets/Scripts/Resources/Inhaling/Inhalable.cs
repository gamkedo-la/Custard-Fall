using System;
using System.Collections.Generic;
using UnityEngine;

public class Inhalable : MonoBehaviour, WorldItem
{
    public WorldCells worldCells;
    public bool hasInhaleFocus;
    public GameObject optVisual;
    private readonly Queue<InhaleQueueItem> _inhaleQueue = new Queue<InhaleQueueItem>();
    private Inhaler _currentInhaler = null;
    private float _currentInhaleStrength;
    private Wobble _wobble;
    private bool _wobbleInitialized;

    public string interactionMessage;
    private Coords cellPosition;

    protected bool _usedUp = false;

    [SerializeField] private InhaleItemProfile[] inhaleItemsProfile;

    public void Inhale(Inhaler inhaler, float strength)
    {
        // sub classes can decide if strength is enough
        if (OnInhaleStart(inhaler, strength))
        {
            hasInhaleFocus = true;
            _currentInhaler = inhaler;
            _currentInhaleStrength = strength;
        }
    }

    protected virtual void Start()
    {
        if (optVisual)
        {
            _wobble = optVisual.GetComponent<Wobble>();
        }
        else
        {
            _wobble = GetComponent<Wobble>();
        }

        _wobbleInitialized = _wobble != null;
        Init();
        ReFillInhaleQueue();
        _usedUp = false;
    }

    public virtual void Init()
    {
        // register as world item
        var transformPosition = gameObject.transform.position;
        cellPosition = worldCells.GetCellPosition(transformPosition);
        List<WorldItem> worldItemsInCell;
        if (!WorldItems.itemsInCell.TryGetValue(cellPosition, out worldItemsInCell))
        {
            worldItemsInCell = new List<WorldItem>();
            WorldItems.itemsInCell.Add(cellPosition, worldItemsInCell);
        }

        worldItemsInCell.Add(this);
    }

    private void ReFillInhaleQueue()
    {
        foreach (var profile in inhaleItemsProfile)
        {
            AddToInhaleQueue(new Resource(profile.resourceName, profile.itemReference), profile.timeToTake);
        }
    }

    public void ClearInhaleQueue()
    {
        _inhaleQueue.Clear();
    }

    protected virtual void FixedUpdate()
    {
        if (IsUsedUp() || _inhaleQueue.Count == 0)
            return;

        if (_currentInhaler)
        {
            InhaleQueueItem item = _inhaleQueue.Peek();

            item.inhaleThresholdSeconds -= Time.deltaTime;
            if (item.inhaleThresholdSeconds <= 0 && (!_wobbleInitialized || _wobble.IsAtMaxWobble()))
            {
                _inhaleQueue.Dequeue();
                OnResourceInhaledAndMaybeRemove(_currentInhaler, item.resource, item.amount);
            }

            // inhaler needs to be present next update (keep calling Inhale)
            _currentInhaler = null;
            _currentInhaleStrength = 0;

            if (_wobble)
            {
                _wobble.DoWobble();
            }
        }
        else if (hasInhaleFocus)
        {
            hasInhaleFocus = false;
        }
    }

    public virtual bool OnInhaleStart(Inhaler inhaler, float strength)
    {
        // inhale if strength non zero
        // override this in sub class if required
        return strength > 0;
    }

    public virtual void OnResourceInhaledAndMaybeRemove(Inhaler inhaler, Resource resource, int amount)
    {
        inhaler.OnResourceInhaled(resource, amount);

        if (GetRemainingResourcesCount() == 0)
        {
            Remove();
        }
    }

    public int GetRemainingResourcesCount()
    {
        return _inhaleQueue.Count;
    }

    public void AddToInhaleQueue(Resource resource, float inhaleThresholdSeconds)
    {
        _inhaleQueue.Enqueue(new InhaleQueueItem(inhaleThresholdSeconds, resource));
    }

    class InhaleQueueItem
    {
        public Resource resource;
        public bool goesToInventory = true;
        public int amount = 1;
        public float inhaleThresholdSeconds;

        public InhaleQueueItem(float inhaleThresholdSeconds, Resource resource)
        {
            this.resource = resource;
            this.inhaleThresholdSeconds = inhaleThresholdSeconds;
        }

        public InhaleQueueItem(float inhaleThresholdSeconds, Resource resource, int amount)
        {
            this.resource = resource;
            this.inhaleThresholdSeconds = inhaleThresholdSeconds;
            this.amount = amount;
        }
    }

    protected virtual void Remove()
    {
        // cleanup
        List<WorldItem> worldItemsInCell;
        if (WorldItems.itemsInCell.TryGetValue(cellPosition, out worldItemsInCell))
        {
            worldItemsInCell.Remove(this);
        }

        GameObject go = gameObject;
        go.SetActive(false);
        _usedUp = true;
    }

    public bool IsUsedUp()
    {
        return _usedUp;
    }

    public void SetUsedUp(bool isUsedUp)
    {
        this._usedUp = isUsedUp;
    }

    public void Reset()
    {
        ReFillInhaleQueue();
        _usedUp = false;
    }

    private void OnDestroy()
    {
        ClearInhaleQueue();
    }

    [Serializable]
    public class InhaleItemProfile
    {
        public string resourceName;
        public PlaceableItem itemReference;
        public float timeToTake = 1.5f;
    }
}