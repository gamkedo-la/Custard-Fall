using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtObject : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Mode lookAtMode;
    [SerializeField] float delay = .3f;

    private Vector3 _targetPosition;
    private Vector3 _targetPositionInterpolated;

    private enum Mode
    {
        LookAtObject,
        LookAtObjectInverted,
        LookAtObject3d,
    }

    private void Start()
    {
        _targetPosition = target.position;
        _targetPositionInterpolated = _targetPosition;
        StartCoroutine(UpdateTargetPosition());
    }

    private void Update()
    {
        _targetPositionInterpolated += (_targetPosition - _targetPositionInterpolated) * Time.deltaTime / delay;
    }

    public float GetProgress()
    {
        return Vector3.Distance(_targetPosition.normalized, _targetPositionInterpolated.normalized);
    }


    private IEnumerator UpdateTargetPosition()
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            _targetPosition = target.transform.position;
        }
    }

    private void LateUpdate()
    {
        switch (lookAtMode)
        {
            case Mode.LookAtObject:
                transform.LookAt(new Vector3(_targetPositionInterpolated.x, transform.position.y, _targetPositionInterpolated.z));
                break;
            case Mode.LookAtObject3d:
                transform.LookAt(_targetPositionInterpolated);
                break;
            case Mode.LookAtObjectInverted:
                transform.LookAt(new Vector3(_targetPositionInterpolated.x, transform.position.y, _targetPositionInterpolated.z));
                transform.forward = -transform.forward;
                break;
            default:
                transform.LookAt(target.transform);
                break;
        }
    }
}