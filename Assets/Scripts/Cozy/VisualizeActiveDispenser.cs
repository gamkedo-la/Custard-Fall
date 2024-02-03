using UnityEngine;

[RequireComponent(typeof(RadianceDispenser))]
public class VisualizeActiveDispenser : MonoBehaviour
{
    [SerializeField] private GameObject visualizer;

    private RadianceDispenser _dispenser;

    void Awake()
    {
        _dispenser = gameObject.GetComponent<RadianceDispenser>();
        _dispenser.onActivated += Visualize;
    }

    public void Visualize(object sender, bool active)
    {
        visualizer.gameObject.SetActive(active);
    }
}