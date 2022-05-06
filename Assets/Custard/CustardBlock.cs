using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustardBlock : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void Start()
    {
    }

    private void Update()
    {
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}