using System;
using UnityEngine;

[RequireComponent(typeof(CozyDispenser), typeof(UpgradeableStructure))]
public class UpgradableCozyDispenser : MonoBehaviour
{
    private CozyDispenser _cozyDispenser;
    private Inhalable _inhalable;

    private void Awake()
    {
        var upgradableCozyDispenser = GetComponent<UpgradeableStructure>();
        upgradableCozyDispenser.OnLevelUp += OnLevelUp;

        _cozyDispenser = GetComponent<CozyDispenser>();
        _inhalable = GetComponent<Inhalable>();
    }

    private void OnLevelUp(object sender, UpgradeableStructure.UpgradeArgs e)
    {
        _cozyDispenser.Coziness += 1;

        // also update the comment
        if (_inhalable)
        {
            _inhalable.interactionMessage = e.comment;
        }
    }
}