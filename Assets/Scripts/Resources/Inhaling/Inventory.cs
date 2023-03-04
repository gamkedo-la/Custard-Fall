using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;


public class Inventory : MonoBehaviour
{
    #region Singleton

    public static Inventory instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of Inventory found!");
            return;
        }

        instance = this;
    }

    #endregion

    public delegate void OnItemChanged();

    public OnItemChanged onItemChangedCallback;
    private InventoryUI UIRef;

    private readonly HashSet<InventorySlot> _slots = new HashSet<InventorySlot>();

    public void SetUIRef(InventoryUI getRef)
    {
        UIRef = getRef;
    }

    public int AddOrSubResourceAmount(Resource resource, int amount)
    {
        InventorySlot slot = null;
        if (amount >= 0)
            Debug.Log("Adding " + amount + " " + resource.Name);
        else
            Debug.Log("Removing " + amount + " " + resource.Name);

        // inelegant way to do it, but correct way wasn't working, so brute force :)
        // (not happening often enough for this to be any sort of performance snag)
        foreach (InventorySlot val in _slots)
        {
            if (val.Resource.Name == resource.Name)
            {
                slot = val;
            }
        }

        if (slot != null)
        {
            slot.Amount = amount + slot.Amount;
            if (slot.Amount <= 0)
            {
                Debug.Log("Remaining "+slot.Amount );
                slot.Amount = 0;
                _slots.Remove(slot);
                Debug.Log("Has the slot been removed? "+!_slots.TryGetValue(slot,out _) );

                if (onItemChangedCallback != null)
                    onItemChangedCallback.Invoke();
            }
        }
        else if (amount > 0)
        {
            slot = new InventorySlot(resource, amount, _slots.Count);
            _slots.Add(slot);

            if (onItemChangedCallback != null)
                onItemChangedCallback.Invoke();
        }

        UIRef.UpdateUI();
        if (slot == null)
        {
            return 0;
        }
        else
        {
            return slot.Amount;
        }
    }

    public int GetResourceAmount(Resource resource)
    {
        if(resource == null || resource.Name == null)
            Debug.Log("Resource already null");
        if (_slots.TryGetValue(InventorySlot.Of(resource), out InventorySlot slot))
            return slot.Amount;
        else
            return 0;
    }

    public List<InventorySlot> GetResourceList()
    {
        List<InventorySlot> local = new List<InventorySlot>();
        HashSet<InventorySlot>.Enumerator em = _slots.GetEnumerator();
        while (em.MoveNext())
        {
            local.Add(em.Current);
        }
        em.Dispose();

        return local;
    }

    public class InventorySlot
    {
        public Resource Resource { get; }

        public int Amount { get; set; }

        public int SortIndex { get; set; }

        public static InventorySlot Of(Resource resource)
        {
            return new InventorySlot(resource, 0, 0);
        }

        public InventorySlot(Resource resource, int amount, int sortIndex)
        {
            Resource = resource;
            Amount = amount;
            SortIndex = sortIndex;
        }

        protected bool Equals(InventorySlot other)
        {
            return Equals(Resource.Name, other.Resource.Name);
        }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((InventorySlot) obj);
        }
        
        public override int GetHashCode()
        {
            return (Resource != null ? Resource.GetHashCode() : 0);
        }
    }
}