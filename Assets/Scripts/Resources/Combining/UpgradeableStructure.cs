using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class UpgradeableStructure : MonoBehaviour, ItemReceiver
{
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int investedPoints = 0;
    [SerializeField] private PlaceableItem validUpgrade;

    [SerializeField] private UpgradeConfig[] upgradeLevels;

    public event EventHandler<UpgradeArgs> OnProgressToLevelUp;
    public event EventHandler<UpgradeArgs> OnLevelUp;
    public event EventHandler<UpgradeArgs> OnPreviewLeave;
    public event EventHandler<UpgradeArgs> OnPreviewEnter;

    public bool IsMaxedOut()
    {
        return currentLevel > upgradeLevels.Length;
    }

    public bool CanUpgradeWith(PlaceableItem material)
    {
        return !IsMaxedOut() && validUpgrade == material;
    }

    public bool UpgradeWith(PlaceableItem material)
    {
        if (CanUpgradeWith(material))
        {
            investedPoints++;
            var obtainedUpgrade = upgradeLevels[currentLevel - 1];

            OnProgressToLevelUp?.Invoke(this,
                new UpgradeArgs(currentLevel, obtainedUpgrade.Comment, obtainedUpgrade.RequiredPoints, investedPoints,
                    false));

            if (investedPoints >= obtainedUpgrade.RequiredPoints)
            {
                currentLevel++;
                if (IsMaxedOut())
                {
                    OnLevelUp?.Invoke(this,
                        new UpgradeArgs(currentLevel, obtainedUpgrade.Comment, obtainedUpgrade.RequiredPoints,
                            investedPoints,
                            true));
                }
                else
                {
                    investedPoints = 0;
                    UpgradeConfig nextUpgrade = upgradeLevels[currentLevel - 1];
                    OnLevelUp?.Invoke(this,
                        new UpgradeArgs(currentLevel, obtainedUpgrade.Comment, nextUpgrade.RequiredPoints,
                            investedPoints,
                            false));
                }
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    [Serializable]
    public class UpgradeConfig
    {
        public string comment;
        public int requiredPoints;

        public string Comment => comment;
        public int RequiredPoints => requiredPoints;

        public PlaceableItem upgrade;
    }

    public class UpgradeArgs : EventArgs
    {
        public int level;
        public string comment;
        public int requiredPoints;
        public int investedPoints;
        public bool maxedOut;

        public UpgradeArgs(int level, string comment, int requiredPoints, int investedPoints, bool maxedOut)
        {
            this.level = level;
            this.comment = comment;
            this.requiredPoints = requiredPoints;
            this.investedPoints = investedPoints;
            this.maxedOut = maxedOut;
        }

        private UpgradeArgs()
        {
        }

        public static UpgradeArgs Empty()
        {
            return new UpgradeArgs();
        }
    }

    public bool CanReceiveItem(PlaceableItem material)
    {
        return CanUpgradeWith(material);
    }

    public bool PreviewReceiveItem(PlaceableItem material)
    {
        var canUpgradeWith = CanUpgradeWith(material);
        if (canUpgradeWith)
        {
            var upgradeConfig = upgradeLevels[currentLevel - 1];
            OnPreviewEnter?.Invoke(this,
                new UpgradeArgs(currentLevel, upgradeConfig.comment, upgradeConfig.requiredPoints, investedPoints,
                    IsMaxedOut()));
        }

        return canUpgradeWith;
    }

    public bool ReceiveItem(PlaceableItem material)
    {
        return UpgradeWith(material);
    }

    public void LeavePreview()
    {
        OnPreviewLeave?.Invoke(this, UpgradeArgs.Empty());
    }
}