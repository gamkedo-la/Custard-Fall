using System;
using UnityEngine;

public class Resource : WorldItem
{

    public readonly String Name;
    [SerializeField]
    private PlaceableItem placeableItem;
    public PlaceableItem PlaceableItem => placeableItem;
    private bool _usedUp = false;

    public Resource(string name, PlaceableItem placeableItem)
    {
        Name = name;
        this.placeableItem = placeableItem;
    }
    
    public Resource(PlaceableItem placeableItem)
    {
        Name = placeableItem.ResourceName;
        this.placeableItem = placeableItem;
    }

    private bool Equals(Resource other)
    {
        return Name == other.Name;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Resource) obj);
    }

    public override int GetHashCode()
    {
        return (Name != null ? Name.GetHashCode() : 0);
    }
    
    public bool IsUsedUp()
    {
        return _usedUp;
    }

    public void SetUsedUp(bool isUsedUp)
    {
        _usedUp = isUsedUp;
    }

    public void Reset()
    {
        _usedUp = false;
    }
}