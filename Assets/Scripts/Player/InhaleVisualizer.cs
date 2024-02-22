using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InhaleVisualizer : MonoBehaviour
{
    [SerializeField] private GameObject inhaleEffect;
    void Start()
    {
        Inhaler inhaler = gameObject.GetComponent<Inhaler>();
        inhaler.onResourceInhaled += OnInhaled;
    }

    private void OnInhaled(Resource resource, int amount)
    {
        inhaleEffect.SetActive(true);
    }
}
