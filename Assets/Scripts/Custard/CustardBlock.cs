using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustardBlock : MonoBehaviour
{
    private Renderer _renderer;
    
    private void Awake()
    {
        gameObject.SetActive(false);
        _renderer = gameObject.GetComponent<Renderer>();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void ChangeMaterial(Material material)
    {
        _renderer.material = material;
    }

    private void OnTriggerEnter(Collider other)
    {
        // TODO: change sound to be underwater
        Debug.Log("INSIDE CUSTARD!");
    }

    private void OnTriggerExit(Collider other)
    {
        // TODO: change sound to be above water
        Debug.Log("OUTSIDE CUSTARD!");
    }
}