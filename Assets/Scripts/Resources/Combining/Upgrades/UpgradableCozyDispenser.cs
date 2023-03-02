using System;
using UnityEngine;

[RequireComponent(typeof(CozyDispenser), typeof(UpgradeableStructure))]
    public class UpgradableCozyDispenser : MonoBehaviour
    {

        private void Start()
        {
            var upgradableCozyDispenser = GetComponent<UpgradeableStructure>();
            upgradableCozyDispenser.OnLevelUp += OnLevelUp;
        }

        private void OnLevelUp(object sender, UpgradeableStructure.UpgradeArgs e)
        {
            var cozyDispenser = GetComponent<CozyDispenser>();
            cozyDispenser.Coziness += 1;
            var inhalable = GetComponent<Inhalable>();
            if (inhalable)
            {
                inhalable.interactionMessage = e.comment;
            }
        }
    }
