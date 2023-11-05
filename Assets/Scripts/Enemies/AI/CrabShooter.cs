using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LookAtObject))]
[RequireComponent(typeof(ProjectileSpawner))]
public class CrabShooter : Mob
{
    private LookAtObject _lookAtObject;
    private ProjectileSpawner _projectileSpawner;
    private Inhalable _inhalable;
    private StateOfMind _stateOfMind = StateOfMind.OwnBusiness;

    [SerializeField] private float surprisedDelay = .7f;

    protected override void Awake()
    {
        base.Awake();
        _lookAtObject = GetComponent<LookAtObject>();
        _projectileSpawner = GetComponent<ProjectileSpawner>();
        _inhalable = GetComponent<InhalableProjectile>();
    }

    protected override void Start()
    {
        base.Start();
        tracker.onTargetEnter += MaybeGetAngry;
        tracker.onTargetEnter += LookAtTarget;
        tracker.onTargetExit += MaybeCalmDown;
        _inhalable.onInhaling += ReactToInhaling;
    }

    private void LookAtTarget(GameObject target)
    {
        _lookAtObject.UpdateTarget(target.transform);
    }

    private void ReactToInhaling(Inhaler inhaler)
    {
        if (_stateOfMind == StateOfMind.OwnBusiness)
        {
            OnStateOfMindChange(StateOfMind.Surprised);
        }
    }

    protected override void MaybeCalmDown()
    {
        _projectileSpawner.enabled = false;
        _lookAtObject.enabled = false;
        _stateOfMind = StateOfMind.OwnBusiness;
    }

    protected override void MaybeGetAngry(GameObject target)
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

        _stateOfMind = newStateOfMind;
    }

    private void StartLookAtTarget()
    {
        _lookAtObject.enabled = true;
    }

    private void StopLookAtTarget()
    {
        _lookAtObject.enabled = false;
    }

    private void StartRangedAttack()
    {
        _projectileSpawner.enabled = true;
    }

    private void StopRangedAttack()
    {
        _projectileSpawner.enabled = false;
    }


    private enum StateOfMind
    {
        OwnBusiness,
        Surprised,
        Angry
    }
}