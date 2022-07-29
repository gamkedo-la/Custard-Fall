using System;
using System.Collections.Generic;
using UnityEngine;

public class InhaleListener : MonoBehaviour, WorldItem
{
    public WorldCells worldCells;
    public bool hasInhaleFocus;
    private readonly List<InhaleQueueItem> _inhaleQueue = new List<InhaleQueueItem>();
    private Inhaler _currentInhaler = null;
    private float _currentInhaleStrength;
    private Wobble _wobble;

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

    private void Start()
    {
        _wobble = GetComponent<Wobble>();
        Init();
    }

    public virtual void Init()
    {
        // register as world item
        var transformPosition = gameObject.transform.position;
        Coords cellPosition = worldCells.GetCellPosition(transformPosition.x, transformPosition.z);
        List<WorldItem> worldItemsInCell;
        if (!WorldItems.itemsInCell.TryGetValue(cellPosition, out worldItemsInCell))
        {
            worldItemsInCell = new List<WorldItem>();
            WorldItems.itemsInCell.Add(cellPosition, worldItemsInCell);
        }

        worldItemsInCell.Add(this);
    }

    private void FixedUpdate()
    {
        if (_inhaleQueue.Count == 0)
            return;

        if (_currentInhaler)
        {
            var item = _inhaleQueue[0];
            item.inhaleThresholdSeconds -= Time.deltaTime;
            if (item.inhaleThresholdSeconds <= 0 && (!_wobble || _wobble.IsAtMaxWobble()))
            {
                _inhaleQueue.RemoveAt(0);
                OnResourceInhaled(_currentInhaler, item.resource, item.amount);
            }

            // inhaler needs to be present next update (keep calling Inhale)
            _currentInhaler = null;
            _currentInhaleStrength = 0;

            if (_wobble && hasInhaleFocus)
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

    public virtual void OnResourceInhaled(Inhaler inhaler, Resource resource, int amount)
    {
        inhaler.OnResourceInhaled(resource, amount);
    }

    public int GetRemainingResourcesCount()
    {
        return _inhaleQueue.Count;
    }

    public void AddToInhaleQueue(Resource resource, float inhaleThresholdSeconds)
    {
        _inhaleQueue.Add(new InhaleQueueItem(inhaleThresholdSeconds, resource));
    }

    class InhaleQueueItem
    {
        public Resource resource;
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
}