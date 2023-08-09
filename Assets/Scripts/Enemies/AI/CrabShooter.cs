using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(LookAtObject))]
[RequireComponent(typeof(ProjectileSpawner))]
public class CrabShooter : Mob
{

    protected LookAtObject lookAtObject;
    protected ProjectileSpawner projectileSpawner;
    protected Inhalable _inhalable;

    protected StateOfMind stateOfMind = StateOfMind.OwnBusiness;
    [SerializeField]
    private float surprisedDelay = .7f;

    protected override void Awake()
    {
        base.Awake();
        lookAtObject = GetComponent<LookAtObject>();
        projectileSpawner = GetComponent<ProjectileSpawner>();
        _inhalable = GetComponent<InhalableProjectile>();
    }

    protected override void Start()
    {
        base.Start();
        tracker.onTargetEnter += MaybeGetAngry;
        tracker.onTargetExit += MaybeCalmDown;
        _inhalable.onInhaling += ReactToInhaling;
    }

    private void ReactToInhaling(Inhaler inhaler)
    {
        if (stateOfMind == StateOfMind.OwnBusiness)
        {
            OnStateOfMindChange(StateOfMind.Surprised);
        }
    }

    protected override void MaybeCalmDown()
    {
        projectileSpawner.enabled = false;
        lookAtObject.enabled = false;
        stateOfMind = StateOfMind.OwnBusiness;
    }

    protected override void MaybeGetAngry()
    {
        OnStateOfMindChange(StateOfMind.Surprised);
    }

    private IEnumerator GetAngryAfterDelay()
    {
        yield return new WaitForSeconds(surprisedDelay);
        OnStateOfMindChange(StateOfMind.Angry);
    }
    
    private void OnStateOfMindChange(StateOfMind newStateOfMind)
    {
        switch (newStateOfMind)
        {
            case StateOfMind.OwnBusiness:
                StopLookAtTarget();
                StopRangedAttack();
                break;
            case StateOfMind.Surprised:
                StartLookAtTarget();
                StopRangedAttack();
                StartCoroutine(GetAngryAfterDelay());
                break;
            case StateOfMind.Angry:
                StartRangedAttack();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newStateOfMind), newStateOfMind, null);
        }

        stateOfMind = newStateOfMind;
    }

    private void StartLookAtTarget()
    {
        lookAtObject.enabled = true;
    }
    private void StopLookAtTarget()
    {
        lookAtObject.enabled = false;
    }
    private void StartRangedAttack()
    {
        projectileSpawner.enabled = true;
    }
    private void StopRangedAttack()
    {
        projectileSpawner.enabled = false;
    }


    public enum StateOfMind
    {
        OwnBusiness, Surprised, Angry
    }
}