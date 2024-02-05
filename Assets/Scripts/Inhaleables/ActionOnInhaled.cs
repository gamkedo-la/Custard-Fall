using UnityEngine;

public class ActionOnInhaled : MonoBehaviour
{
    private Inhalable _inhalable;
    [SerializeField] private float radianceBonus = 0;
    [SerializeField] private int healthBonus = 0;

    private void Awake()
    {
        _inhalable = gameObject.GetComponent<Inhalable>();
        _inhalable.onInhaled += IncreaseRadiance;
    }

    private void IncreaseRadiance(Inhaler inhaler, Resource resource, int amount)
    {
        if (radianceBonus != 0f)
        {
            var radianceReceiver = inhaler.gameObject.GetComponentInParent<RadianceReceiver>();
            if (radianceReceiver != null)
            {
                radianceReceiver.IncreaseRadiance(radianceBonus);
            }
        }

        if (healthBonus != 0)
        {
            var player = inhaler.gameObject.GetComponentInParent<Player>();
            if (player != null)
            {
                player.TakeDamage(-healthBonus, DamageImplication.Health);
            }
        }
    }
}