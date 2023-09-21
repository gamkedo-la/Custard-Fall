using System;
using UnityEngine;


// TODO rewrite as ScriptableObject
public class InhalersTracker : MonoBehaviour
{
    public static InhalersTracker Instance { get; private set; }

    public delegate void OnInhaleStart(Inhaler inhaler);

    public OnInhaleStart onInhaleStart;

    public delegate void OnInhaleEnd(Inhaler inhaler);

    public OnInhaleEnd onInhaleEnd;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
}