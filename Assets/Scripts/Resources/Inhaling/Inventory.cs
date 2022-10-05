using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;


public class Inventory : MonoBehaviour
{
    #region Singleton

    public static Inventory instance;

    void Awake ()
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

    private readonly SortedSet<InventorySlot> _slots = new SortedSet<InventorySlot>(InventorySlot.SortIndexComparer);
    public void SetUIRef(InventoryUI getRef)
    {
        UIRef = getRef;
    }

    public int AddOrSubResourceAmount(Resource resource, int amount)
    {
        InventorySlot slot = null;

        // inelegant way to do it, but correct way wasn't working, so brute force :)
        // (not happening often enough for this to be any sort of performance snag)
        foreach (InventorySlot val in _slots) {
            if (val.Resource.Name == resource.Name) {
                slot = val;
            }
        }

        if (slot != null)
        {
            // Debug.Log("already had: "+resource.Name);
            slot.Amount = amount + slot.Amount;
            if (slot.Amount <= 0)
            {
                slot.Amount = 0;
                _slots.Remove(slot);

                if (onItemChangedCallback != null)
                    onItemChangedCallback.Invoke();
            }
        }
        else if(amount > 0)
        {
            slot = new InventorySlot(resource, amount, _slots.Count);
            _slots.Add(slot);

            // Debug.Log("new resource type: " + resource.Name);

            if (onItemChangedCallback != null)
                onItemChangedCallback.Invoke();

        }
        UIRef.UpdateUI();
        return slot.Amount;

    }

    public int GetResourceAmount(Resource resource)
    {
        if (_slots.TryGetValue(InventorySlot.Of(resource), out InventorySlot slot))
            return slot.Amount;
        else
            return 0;
    }

    public List<InventorySlot> GetResourceList()
    {
        List<InventorySlot> local = new List<InventorySlot>();
        //_slots.ToList().ForEach(s => local.Add(s.name));
        SortedSet<InventorySlot>.Enumerator em = _slots.GetEnumerator();
        while (em.MoveNext())
        {
            local.Add(em.Current);
            // Debug.Log(" item: " + em.Current.Resource.Name + "(" + em.Current.Amount + ")");
        }
        return local;
    }

    public class InventorySlot
    {
        public Resource Resource { get;}

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

        private sealed class SortIndexEqualityComparer : IComparer<InventorySlot>
        {
            public int Compare(InventorySlot x, InventorySlot y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.SortIndex.CompareTo(y.SortIndex);
            }
        }

        public static IComparer<InventorySlot> SortIndexComparer { get; } = new SortIndexEqualityComparer();
    }
}