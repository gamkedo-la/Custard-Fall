using UnityEngine;

public class InhaleVisualizer : MonoBehaviour
{
    [SerializeField] private EffectPromise inhaleEffect;

    void Start()
    {
        Inhaler inhaler = gameObject.GetComponent<Inhaler>();
        inhaler.onResourceInhaled += OnInhaled;
    }

    private void OnInhaled(Resource resource, int amount)
    {
        if (resource.Name == "radiance-orb" || resource.Name == "health-orb")
        {
            inhaleEffect.PlayEffect(() =>
            {
                Debug.Log("inhale effect ended");
            });
        }
    }
}