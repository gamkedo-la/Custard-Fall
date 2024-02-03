using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RadianceReceiver))]
public class RadianceZoneChangeVisualizer : MonoBehaviour
{
    
    [SerializeField] private GameObject visualizer;

    private RadianceReceiver _receiver;
    
    void Awake()
    {
        _receiver = gameObject.GetComponent<RadianceReceiver>();
        _receiver.onRadianceChangeInZone += Visualize;
    }

    public void Visualize(int newLevel, int oldLevel)
    {
        visualizer.gameObject.SetActive(newLevel != 0);
    }
   
}
