using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

[DisallowMultipleComponent]
public class FadeInThenOutCanvasGroup : MonoBehaviour
{
    private CanvasGroup _renderer;

    public float fadeInSpeed = 0.4f;
    public float fadeOutSpeed = 0.33f;
    public float midDelay = .75f;

    private bool _fadingOut = false;
    private bool _waiting = false;

    void Awake()
    {
        _renderer = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        _renderer.alpha = 0;
    }


    void Update()
    {
        if (_waiting)
            return;

        if (_fadingOut)
        {
            var alpha = _renderer.alpha - Time.deltaTime / fadeOutSpeed;
            _renderer.alpha = alpha;

            if (alpha <= 0f)
            {
                Destroy(this);
            }
        }
        else
        {
            var alpha = _renderer.alpha + Time.deltaTime / fadeInSpeed;
            _renderer.alpha = alpha;

            if (alpha >= 1.0f)
            {
                _fadingOut = true;
                StartCoroutine(Wait(midDelay));
            }
        }
    }


    private IEnumerator Wait(float waitTime)
    {
        _waiting = true;
        yield return new WaitForSeconds(waitTime);
        _waiting = false;
    }

    public void KeepItUp()
    {
        if (_fadingOut)
            _fadingOut = false;

        if (_waiting)
        {
            StopAllCoroutines();
            _waiting = false;
        }
    }
}