using System;
using UnityEngine;

[RequireComponent(typeof(RadianceDispenser), typeof(UpgradeableStructure))]
public class UpgradableRadianceDispenser : MonoBehaviour
{
    private RadianceDispenser _radianceDispenser;
    private Inhalable _inhalable;

    private void Awake()
    {
        var upgradableRadianceDispenser = GetComponent<UpgradeableStructure>();
        upgradableRadianceDispenser.OnLevelUp += OnLevelUp;

        _radianceDispenser = GetComponent<RadianceDispenser>();
        _inhalable = GetComponent<Inhalable>();
    }

    private void OnLevelUp(object sender, UpgradeableStructure.UpgradeArgs e)
    {
        _radianceDispenser.Radiance = e.level;

        // also update the comment
        if (_inhalable)
        {
            _inhalable.interactionMessage = e.comment;
        }
    }
}