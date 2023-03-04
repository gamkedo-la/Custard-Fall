using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class UpgradeableStructure : MonoBehaviour, ItemReceiver
{
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int investedPoints = 0;
    [SerializeField] private PlaceableItem expectedUpgradeMaterial;

    [SerializeField] private UpgradeConfig[] upgradeLevels;

    private GameObject previewInstance;
    private PlaceableItemReference placeableItemReference;

    public event EventHandler<UpgradeArgs> OnProgressToLevelUp;
    public event EventHandler<UpgradeArgs> OnLevelUp;
    public event EventHandler<UpgradeArgs> OnPreviewLeave;
    public event EventHandler<UpgradeArgs> OnPreviewEnter;

    private void Awake()
    {
        placeableItemReference = GetComponent<PlaceableItemReference>();
        OnPreviewEnter += PreviewUpgrade;
        OnPreviewLeave += LeavePreviewUpgrade;
        OnLevelUp += HandleUpgrade;
    }

    public bool IsMaxedOut()
    {
        return currentLevel > upgradeLevels.Length;
    }

    public bool CanUpgradeWith(PlaceableItem material)
    {
        return !IsMaxedOut() && expectedUpgradeMaterial == material;
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

        [Tooltip("optional, usually on the last level")]
        public PlaceableItem upgrade;

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

    private void PreviewUpgrade(object sender, UpgradeArgs e)
    {
        if (previewInstance == null)
        {
            var placeableItem = upgradeLevels[currentLevel - 1].upgrade;
            var originalTransform = transform;
            if (placeableItem)
            {
                previewInstance = Instantiate(placeableItem.PlaceablePreview, originalTransform.position, originalTransform.rotation);
            }
            else
            {
                if (placeableItemReference)
                    previewInstance = Instantiate(placeableItemReference.Item().PlaceablePreview, originalTransform.position, originalTransform.rotation);
            }
        }

        previewInstance.SetActive(true);
    }

    private void LeavePreviewUpgrade(object sender, UpgradeArgs e)
    {
        if (previewInstance != null)
        {
            previewInstance.SetActive(false);
        }
    }


    private void HandleUpgrade(object sender, UpgradeArgs e)
    {
        var placeableItem = upgradeLevels[e.level - 2].upgrade;
        GameObject replacement;
        if (placeableItem != null)
        {
            Debug.Log(placeableItem.name);
            var originalTransform = transform;
            replacement = Instantiate(placeableItem.Prototype, originalTransform.position, originalTransform.rotation);
        }
        else
        {
            Debug.Log("no internal upgrade");
            replacement = null;
        }

        if (previewInstance != null)
        {
            previewInstance.SetActive(false);
            Destroy(previewInstance);
            previewInstance = null;
        }

        if (replacement)
        {
            Debug.Log("Destroying "+gameObject.name);
            Destroy(this.gameObject);
        }
    }
}