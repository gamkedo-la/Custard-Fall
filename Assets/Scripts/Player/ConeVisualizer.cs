using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeVisualizer : MonoBehaviour
{
    [SerializeField] private Inhaler inhaler;
    [SerializeField] private Material small;
    [SerializeField] private Material normal;
    [SerializeField] private Renderer renderer;

    private void Start()
    {
        inhaler.onConeSizeChange += VisualizeCone;
    }

    private void VisualizeCone(Inhaler.ConeSize newsize, Inhaler.ConeSize oldsize)
    {
        if (newsize == Inhaler.ConeSize.SMALL)
        {
            gameObject.transform.localScale -= new Vector3(0.4f, 0.6f ,0.6f);
            renderer.material = small;
        }
        else
        {
            gameObject.transform.localScale += new Vector3(0.4f, 0.6f ,0.6f);
            renderer.material = normal;
        }
    }
}
