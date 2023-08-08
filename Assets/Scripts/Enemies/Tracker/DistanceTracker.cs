using System;
using System.Collections;
using UnityEngine;


public class DistanceTracker : MonoBehaviour
{
    [SerializeField] private float maxDistance = 8f;
    [SerializeField] private float checkIntervalEnter = .4f;
    [SerializeField] private float checkIntervalExit = .3f;

    public delegate void OnTargetEnter();

    public OnTargetEnter onTargetEnter;

    public delegate void OnTargetExit();

    public OnTargetExit onTargetExit;


    private GameObject _target;
    private bool _inRange = false;

    private void Awake()
    {
        _target = FindObjectOfType<Player>().gameObject;
    }

    private void Start()
    {
        StartCoroutine(MaybeTriggerDistanceEvent());
    }

    private IEnumerator MaybeTriggerDistanceEvent()
    {
        while (true)
        {
            yield return new WaitForSeconds(_inRange ? checkIntervalExit : checkIntervalEnter);
            var inRange = Vector3.Distance(transform.position, _target.transform.position) < maxDistance;
            
            if (!_inRange && inRange)
            {
                onTargetEnter?.Invoke();
            }
            else if (_inRange && !inRange)
            {
                onTargetExit?.Invoke();
            }

            _inRange = inRange;
        }
    }
}