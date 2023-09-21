using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTowards : MonoBehaviour
{
    public float speed = 1;

    [SerializeField] private float magnetStrength = .3f;
    private Transform _magnet;
    private bool _isMoving;


    void Update()
    {
        var modifiedSpeed = Time.deltaTime * speed;
        var projectileTransform = transform;
        var objectPosition = projectileTransform.position;
        objectPosition += projectileTransform.forward * modifiedSpeed;

        if (_isMoving)
        {
            projectileTransform.position +=
                (_magnet.position - projectileTransform.position) * (Time.deltaTime * magnetStrength);
        }
    }

    public void SetMagnet(Transform magnet)
    {
        bool freshAssigment = this._magnet;
        _magnet = magnet;
        _isMoving = magnet != null;
    }
}