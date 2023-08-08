using System;
using UnityEngine;

[RequireComponent(typeof(DistanceTracker))]
public class Mob : MonoBehaviour
{
    protected DistanceTracker tracker;

    protected virtual void Awake()
    {
        tracker = GetComponent<DistanceTracker>();
    }

    protected virtual void Start()
    {
        tracker.onTargetEnter += MaybeGetAngry;
        tracker.onTargetExit += MaybeCalmDown;
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