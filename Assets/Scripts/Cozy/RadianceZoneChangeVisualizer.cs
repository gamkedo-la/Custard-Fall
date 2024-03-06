using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(RadianceReceiver))]
public class RadianceZoneChangeVisualizer : MonoBehaviour
{
    [FormerlySerializedAs("visualizer")] [SerializeField] private List<GameObject> visualizers;

    private RadianceReceiver _receiver;

    private void Awake()
    {
        _receiver = gameObject.GetComponent<RadianceReceiver>();
        _receiver.onRadianceChangeInZone += Visualize;
    }

    private void Visualize(int newLevel, int oldLevel)
    {
        foreach (var visual in visualizers)
        {
            visual.gameObject.SetActive(newLevel != 0);
        }
    }
}