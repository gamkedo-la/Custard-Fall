using UnityEngine;

[RequireComponent(typeof(LookAtObject))]
[RequireComponent(typeof(ProjectileSpawner))]
public class CrabShooter : Mob
{

    protected LookAtObject lookAtObject;
    protected ProjectileSpawner projectileSpawner;
    
    protected override void Awake()
    {
        base.Awake();
        lookAtObject = GetComponent<LookAtObject>();
        projectileSpawner = GetComponent<ProjectileSpawner>();
    }

    protected override void Start()
    {
        base.Start();
        _tracker.onTargetEnter += MaybeGetAngry;
        _tracker.onTargetExit += MaybeCalmDown;
    }
    
    protected override void MaybeCalmDown()
    {
        projectileSpawner.enabled = false;
        lookAtObject.enabled = false;
    }

    protected override void MaybeGetAngry()
    {
        projectileSpawner.enabled = true;
        lookAtObject.enabled = true;
    }
}