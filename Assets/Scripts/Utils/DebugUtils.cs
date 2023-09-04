using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DebugUtils : MonoBehaviour
{
    private static DebugUtils _instance;

    [FormerlySerializedAs("VectorVisual")] [SerializeField] private GameObject vectorVisual;

    [SerializeField] private bool display;

    private static readonly Dictionary<String, Transform> DebugTransforms = new();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public static Transform ProvideTransform(String name)
    {
        if (!DebugTransforms.TryGetValue(name, out var visual))
        {
            visual = Instantiate(_instance.vectorVisual).transform;
            DebugTransforms.Add(name, visual);
        }

        visual.gameObject.SetActive(_instance.display);
        return visual;
    }
}