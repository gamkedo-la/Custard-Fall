using System;
using System.Collections;
using UnityEngine;


public class RadianceConsumer : MonoBehaviour
{
    private CozinessReceiver _receiver;
    private Inhaler _inhaler;
    private Coroutine _coroutine;
    [SerializeField] private float rate = 30;

    [SerializeField] private float loss = .1f;


    private void Awake()
    {
        _receiver = gameObject.GetComponent<CozinessReceiver>();
        _inhaler = gameObject.GetComponentInChildren<Inhaler>();
    }

    private void Start()
    {
        _inhaler.onInhaleStart += StartConsuming;
        _inhaler.onInhaleEnd += StopConsuming;
    }

    private void StopConsuming(Inhaler inhaler)
    {
        StopCoroutine(_coroutine);
    }

    private void StartConsuming(Inhaler inhaler)
    {
        _inhaler = inhaler;
        var cozinessReceiver = inhaler.gameObject.GetComponentInParent<CozinessReceiver>();
        if (cozinessReceiver)
        {
            if (_coroutine != null)
                StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(ConsumeRadiance());
        }
    }

    private IEnumerator ConsumeRadiance()
    {
        while (true)
        {
            _receiver.TakeDamage(loss);
            yield return new WaitForSeconds(1f / rate);
        }
    }
}