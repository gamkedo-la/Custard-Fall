using System;
using UnityEngine;

public class Resource : WorldItem
{

    public readonly String Name;

    public Resource(string name)
    {
        Name = name;
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
}