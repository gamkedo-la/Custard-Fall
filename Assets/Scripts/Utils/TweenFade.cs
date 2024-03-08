using System;
using UnityEngine;

public class TweenFade : MonoBehaviour
{
    [SerializeField] private GameObject visual;
    [SerializeField] private float fadeOutDelay = .4f;
    [SerializeField] private float fadeInDelay = .3f;

    private bool _fadeIn = true;
    private float _waitTime = 0;

    public void SetFadeIn(bool doFadeIn)
    {
        _fadeIn = doFadeIn;
        _waitTime = doFadeIn ? fadeInDelay : fadeOutDelay;
    }

    private void Update()
    {
        if(_waitTime <= 0)
        {
            visual.SetActive(_fadeIn);
            enabled = false;
        }
        else
        {
            _waitTime -= Time.deltaTime;
        }
    }
}