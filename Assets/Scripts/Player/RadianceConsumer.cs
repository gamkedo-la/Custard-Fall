using System;
using System.Collections;
using UnityEngine;


public class RadianceConsumer : MonoBehaviour
{
    private RadianceReceiver _receiver;
    private Inhaler _inhaler;
    private Coroutine _coroutine;
    [SerializeField] private float rate = 30;

    [SerializeField] private float loss = .1f;


    private void Awake()
    {
        _receiver = gameObject.GetComponent<RadianceReceiver>();
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
        var radianceReceiver = inhaler.gameObject.GetComponentInParent<RadianceReceiver>();
        if (radianceReceiver)
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
            _receiver.DeclineRadiance(loss);
            yield return new WaitForSeconds(1f / rate);
        }
    }
}