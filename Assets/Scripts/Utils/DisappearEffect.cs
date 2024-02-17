using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisappearEffect : MonoBehaviour
{
    private float _targetAlpha;
    private TextMeshProUGUI _textMesh;
    private Color _color;


    void Start()
    {
        _textMesh = GetComponent<TextMeshProUGUI>();
        _color = _textMesh.color;
    }

    public void Activate()
    {
        _targetAlpha = 0;
        _textMesh.CrossFadeAlpha(_targetAlpha, .22f, true);
    }

    public void Reset()
    {
        _targetAlpha = 1;
        _textMesh.color = _color;
        _textMesh.CrossFadeAlpha(_color.a,0,true);
    }
}
