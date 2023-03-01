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
    public event EventHandler<UpgradeArgs> OnValidFocus;
    public event EventHandler<UpgradeArgs> OnLevelUp;

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
            var nextUpgrade = upgradeLevels[currentLevel - 1];

            OnProgressToLevelUp?.Invoke(this,
                new UpgradeArgs(currentLevel, nextUpgrade.comment, nextUpgrade.requiredPoints, investedPoints, false));

            if (investedPoints >= nextUpgrade.RequiredPoints)
            {
                currentLevel++;
                bool maxedOut = IsMaxedOut();
                if (!maxedOut)
                {
                    investedPoints = 0;
                    nextUpgrade = upgradeLevels[currentLevel - 1];
                }

                OnLevelUp?.Invoke(this,
                    new UpgradeArgs(currentLevel, nextUpgrade.comment, nextUpgrade.requiredPoints, investedPoints,
                        maxedOut));
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
    }

    public bool CanReceiveItem(PlaceableItem material)
    {
        return CanUpgradeWith(material);
    }

    public bool PreviewReceiveItem(PlaceableItem material)
    {
        Debug.Log("Preview upgrade");
        var canUpgradeWith = CanUpgradeWith(material);
        // if(canUpgradeWith)
        //     OnValidFocus?.Invoke(this, );
        return canUpgradeWith;
    }

    public bool ReceiveItem(PlaceableItem material)
    {
        return UpgradeWith(material);

    }

    public void OnPreviewLeave()
    {
        Debug.Log("Leaving upgrade preview.");
    }
}