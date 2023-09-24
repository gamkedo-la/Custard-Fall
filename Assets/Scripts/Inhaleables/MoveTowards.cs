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
        if (_isMoving)
        {
            var transformTmp = transform;
            var positionTmp = transformTmp.position;
            positionTmp +=
                (_magnet.position - positionTmp) * (magnetStrength * speed * Time.deltaTime);
            transformTmp.position = positionTmp;
        }
    }

    public void SetMagnet(Transform magnet)
    {
        _magnet = magnet;
        _isMoving = magnet != null;
    }
}