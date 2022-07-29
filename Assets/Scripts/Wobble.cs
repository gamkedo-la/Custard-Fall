using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Wobble : MonoBehaviour
{
    public float wobbleStrength = 0f;
    private Vector3 startShakePosition;
    private float shakeScale = 0.3f;
    private float decayValue = .95f; 

    // Start is called before the first frame update
    void Start()
    {
        startShakePosition = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
            transform.position = startShakePosition + Random.insideUnitSphere * (wobbleStrength * shakeScale);
            wobbleStrength *= decayValue;
    }
}