using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Wobble : MonoBehaviour
{
    public float wobbleStrength = 0.0f; // range is 0.0 to 1.0
    public bool canMove = false;
    public Transform parent;
    public float shakeScale = 0.15f; // change to adjust severity for shake
    private float
        decayValue =
            0.85f; // percentage of wobbleStrength kept every FixedUpdate; suggested range .7 to .99 (change to adjust transition to stop shaking)
    private float _wobbleIncreaseRate = 0.05f;
    private bool _increasingWobble = false; // one fixed frame advantage: prevents decay fighting against build up
    private Vector3 _startShakePosition;

    // Start is called before the first frame update
    void Start()
    {
        _startShakePosition = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (canMove)
        {
            _startShakePosition = parent.position;
        }

        var diff = Random.insideUnitSphere * (wobbleStrength * shakeScale);
        transform.position = _startShakePosition + diff;
        if (_increasingWobble)
        {
            _increasingWobble = false;
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
        _increasingWobble = true;
        wobbleStrength += _wobbleIncreaseRate;
        wobbleStrength = Mathf.Min(wobbleStrength, 1f);
    }
}