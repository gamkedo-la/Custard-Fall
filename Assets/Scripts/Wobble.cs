using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Wobble : MonoBehaviour
{
    public float wobbleStrength = 0f;
    private Vector3 startShakePosition;
    private float shakeScale = 0.2f;
    private float decayValue = .85f;
    private bool increasingWobble = false;

    // Start is called before the first frame update
    void Start()
    {
        startShakePosition = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = startShakePosition + Random.insideUnitSphere * (wobbleStrength * shakeScale);
        if (increasingWobble)
        {
            increasingWobble = false;
        }
        else
        {
            wobbleStrength *= decayValue;
        }
    }

    public bool IsAtMaxWobble()
    {
        return wobbleStrength >= decayValue;
    }

    public void DoWobble()
    {
        increasingWobble = true;
        wobbleStrength += .1f;
        wobbleStrength = Mathf.Min(wobbleStrength, 1f);
    }
}