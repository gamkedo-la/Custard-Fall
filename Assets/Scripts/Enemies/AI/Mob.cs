using System;
using UnityEngine;

[RequireComponent(typeof(DistanceTracker))]
public class Mob : MonoBehaviour
{
    protected DistanceTracker _tracker;

    protected virtual void Awake()
    {
        _tracker = GetComponent<DistanceTracker>();
    }

    protected virtual void Start()
    {
        _tracker.onTargetEnter += MaybeGetAngry;
        _tracker.onTargetExit += MaybeCalmDown;
    }

    protected virtual void MaybeCalmDown()
    {
        Debug.Log("calming down");
    }

    protected virtual void MaybeGetAngry()
    {
        Debug.Log("getting angry");
    }
}