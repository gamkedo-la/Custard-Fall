﻿using System;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MoveTowards))]
public class MoveOnRangedInhaling : MonoBehaviour
{
    [SerializeField] private float range;
    private MoveTowards _movementLogic;

    private Inhaler _activeInhaler;
    private bool _isMoving;
    private bool _checkInhalers;

    private void Start()
    {
        InhalersTracker.Instance.onInhaleStart += OnInhaleStart;
        InhalersTracker.Instance.onInhaleEnd += OnInhaleEnd;
    }

    private void Awake()
    {
        _movementLogic = gameObject.GetComponent<MoveTowards>();
    }

    private void FixedUpdate()
    {
        if (_checkInhalers)
        {
            if (InRange(_activeInhaler, this.transform))
            {
                if (!_isMoving)
                {
                    StartMoving(_activeInhaler.transform);
                }
            }
            else if (_isMoving)
            {
                StopMoving();
            }
        }
        else if (_isMoving)
        {
            StopMoving();
        }
    }

    private void OnInhaleEnd(Inhaler inhaler)
    {
        _checkInhalers = false;
        _activeInhaler = null;
    }

    private void OnInhaleStart(Inhaler inhaler)
    {
        _checkInhalers = true;
        _activeInhaler = inhaler;
    }

    private void StartMoving(Transform magnet)
    {
        _isMoving = true;
        _movementLogic.SetMagnet(magnet);
    }

    private void StopMoving()
    {
        _isMoving = false;
        _movementLogic.SetMagnet(null);
    }

    private bool InRange(Inhaler inhaler, Transform magnet)
    {
        return Vector3.Distance(inhaler.transform.position, magnet.position) < range;
    }
}