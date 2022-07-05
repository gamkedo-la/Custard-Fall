using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;


public class Inventory : MonoBehaviour
{
    private readonly SortedSet<InventorySlot> _slots = new SortedSet<InventorySlot>(Comparer<InventorySlot>.Default);

    public int AddOrSubResourceAmount(Resource resource, int amount)
    {
        var slot = new InventorySlot(resource, amount, _slots.Count);
        
        if (_slots.TryGetValue(slot, out slot))
        {
            slot.Amount = amount + slot.Amount;
            if (slot.Amount <= 0)
            {
                _slots.Remove(slot);
            }
        }
        else if(amount > 0)
        {
            _slots.Add(slot);
        }

        return slot.Amount;
    }

    public int GetResourceAmount(Resource resource)
    {
        if (_slots.TryGetValue(InventorySlot.Of(resource), out InventorySlot slot))
            return slot.Amount;
        else
            return 0;
    }


    private class InventorySlot
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
            return Equals(Resource, other.Resource);
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

        private sealed class SortIndexEqualityComparer : IEqualityComparer<InventorySlot>
        {
            public bool Equals(InventorySlot x, InventorySlot y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.SortIndex == y.SortIndex;
            }

            public int GetHashCode(InventorySlot obj)
            {
                return obj.SortIndex;
            }
        }

        public static IEqualityComparer<InventorySlot> SortIndexComparer { get; } = new SortIndexEqualityComparer();
    }
}