using System;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(ProjectileMove))]
public class MoveOnRangedInhaling : MonoBehaviour
{
    [SerializeField] private float range;
    private ProjectileMove _movementLogic;

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
        _movementLogic = gameObject.GetComponent<ProjectileMove>();
        _movementLogic.enabled = false;
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
        _movementLogic.gameObject.SetActive(true);
    }

    private void StopMoving()
    {
        _isMoving = false;
        _movementLogic.SetMagnet(null);
        _movementLogic.enabled = true;
    }

    private bool InRange(Inhaler inhaler, Transform magnet)
    {
        return Vector3.Distance(inhaler.transform.position, magnet.position) < range;
    }
}