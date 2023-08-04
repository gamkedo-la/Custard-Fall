using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

[DisallowMultipleComponent]
public class FadeInCanvasGroup : MonoBehaviour
{
    private CanvasGroup _renderer;

    public float fadeSpeed = 0.4f;

    // Start is called before the first frame update
    void Awake()
    {
        _renderer = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        _renderer.alpha = 0;
    }


    // Update is called once per frame
    void Update()
    {
        var alpha = _renderer.alpha + Time.deltaTime / fadeSpeed;
        _renderer.alpha = alpha;

        if (alpha >= 1.0f)
        {
            Destroy(this);
        }
    }
}