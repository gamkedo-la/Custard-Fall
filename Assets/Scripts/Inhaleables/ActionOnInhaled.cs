using UnityEngine;

public class ActionOnInhaled : MonoBehaviour
{

    private Inhalable _inhalable;
    [SerializeField]
    private float radianceBonus;
    
    private void Awake()
    {
        _inhalable = gameObject.GetComponent<Inhalable>();
        _inhalable.onInhaled += IncreaseRadiance;
    }

    private void IncreaseRadiance(Inhaler inhaler, Resource resource, int amount)
    {
        var radianceReceiver = inhaler.gameObject.GetComponentInParent<RadianceReceiver>();
        if (radianceReceiver != null)
        {
            radianceReceiver.IncreaseRadiance(radianceBonus);
        }
    }
    
}