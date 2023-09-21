using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMove : MonoBehaviour
{
    public float speed = 1;
    public AnimationCurve spawnSpeed;
    public float spawnAnimationDuration = .3f;
    private float _ageInSeconds = 0;

    [SerializeField] private float magnetStrength = .3f;
    [SerializeField] private float magnetRelease = .5f;
    private Transform _magnet;
    

    void Update()
    {
        var modifiedSpeed = Time.deltaTime * (_ageInSeconds < spawnAnimationDuration
            ? speed * spawnSpeed.Evaluate(_ageInSeconds / spawnAnimationDuration)
            : speed);
        var projectileTransform = transform;
        projectileTransform.position += projectileTransform.forward * modifiedSpeed;

        if (_magnet)
        {
            projectileTransform.position +=
                (_magnet.position - projectileTransform.position) * (Time.deltaTime * magnetStrength);
        }

        _ageInSeconds += Time.deltaTime;
    }

    [ContextMenu("Reset")]
    public void Reset()
    {
        var projectileTransform = transform;
        projectileTransform.position -= projectileTransform.forward * (_ageInSeconds * speed);
        _ageInSeconds = 0;
    }


    public void SetMagnet(Transform magnet)
    {
        bool freshAssigment = this._magnet;
        _magnet = magnet;
        if (freshAssigment)
        {
            StartCoroutine(ResetMagnet());
        }
    }

    private IEnumerator ResetMagnet()
    {
        yield return new WaitForSeconds(magnetRelease);
        _magnet = null;
    }
}