using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMove : MonoBehaviour
{
    public float speed = 1;
    public AnimationCurve spawnSpeed;
    public float spawnAnimationDuration = .3f;
    [SerializeField] private float maxLifespan = 5f;
    private float _ageInSeconds = 0;

    [SerializeField] private float magnetStrength = .3f;
    private Transform _magnet;

    private void Start()
    {
        StartCoroutine(KillMe());
    }

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

    private IEnumerator KillMe()
    {
        yield return new WaitForSeconds(maxLifespan);
        Debug.Log("killing old projectile");
        Destroy(gameObject);
    }

    public void SetMagnet(Transform magnet)
    {
        if (this._magnet != null)
        {
            this._magnet = magnet;
            StartCoroutine(ResetMagnet());
        }
    }

    private IEnumerator ResetMagnet()
    {
        yield return new WaitForSeconds(.3f);
        this._magnet = null;
    }
}