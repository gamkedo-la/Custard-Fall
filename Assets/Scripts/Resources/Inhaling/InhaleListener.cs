﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class InhaleListener : MonoBehaviour, WorldItem
{
    public WorldCells worldCells;
    public bool hasInhaleFocus;
    private readonly List<InhaleQueueItem> _inhaleQueue = new List<InhaleQueueItem>();
    private Inhaler _currentInhaler = null;
    private float _currentInhaleStrength;

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
            if (item.inhaleThresholdSeconds <= 0)
            {
                _inhaleQueue.RemoveAt(0);
                OnResourceInhaled(_currentInhaler, item.resource);
            }

            // inhaler needs to be present next update (keep calling Inhale)
            _currentInhaler = null;
            _currentInhaleStrength = 0;
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

    public virtual void OnResourceInhaled(Inhaler inhaler, Resource resource)
    {
        inhaler.OnResourceInhaled(resource);
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
        public float inhaleThresholdSeconds;

        public InhaleQueueItem(float inhaleThresholdSeconds, Resource resource)
        {
            this.resource = resource;
            this.inhaleThresholdSeconds = inhaleThresholdSeconds;
        }
    }
}