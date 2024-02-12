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

    private GameObject _previewInstance;
    private PlaceableItemReference _placeableItemReference;

    public event EventHandler<UpgradeArgs> OnProgressToLevelUp;
    public event EventHandler<UpgradeArgs> OnLevelUp;
    public event EventHandler<UpgradeArgs> OnPreviewLeave;
    public event EventHandler<UpgradeArgs> OnPreviewEnter;

    public PlaceableItem ExpectedUpgradeMaterial
    {
        get
        {
            if (currentLevel > 0 && currentLevel < upgradeLevels.Length)
            {
                var upgradeMaterial = upgradeLevels[currentLevel].upgradeMaterial;
                return upgradeMaterial == null ? expectedUpgradeMaterial : upgradeMaterial;
            }
            else
            {
                return expectedUpgradeMaterial;
            }
        }
    }

    private void Awake()
    {
        _placeableItemReference = GetComponent<PlaceableItemReference>();
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
        return !IsMaxedOut() && ExpectedUpgradeMaterial == material;
    }

    public int CurrentLevel()
    {
        return currentLevel;
    }

    public void SetCurrentLevelSilently(int level)
    {
        currentLevel = level;
        investedPoints = 0;
    }

    public int RequieredPoints()
    {
        if (currentLevel < upgradeLevels.Length)
            return upgradeLevels[currentLevel - 1].requiredPoints;
        else
            return 0;
    }

    public int InvestedPoints()
    {
        return investedPoints;
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
                if (obtainedUpgrade.JumpToLevel >= currentLevel)
                {
                    currentLevel = obtainedUpgrade.jumpToLevel;
                }
                else
                {
                    currentLevel++;
                }

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
        public int requiredPoints = 1;
        public int jumpToLevel;

        [Tooltip("optional, turn current object into the upgrade")]
        public PlaceableItem upgrade;

        [Tooltip("optional, override required upgrade material")]
        public PlaceableItem upgradeMaterial;

        public string Comment => comment;
        public int RequiredPoints => requiredPoints;
        public int JumpToLevel => jumpToLevel;
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
        else
        {
            Debug.Log("Cannot upgrade with "+material.name);
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
        Debug.Log("# preview " + _previewInstance == null);
        if (_previewInstance == null)
        {
            var originalTransform = transform;
            var placeableItem = upgradeLevels[currentLevel - 1].upgrade;
            if (placeableItem)
            {
                _previewInstance = Instantiate(placeableItem.PlaceablePreview, originalTransform.position,
                    originalTransform.rotation);
            }
            else
            {
                if (_placeableItemReference)
                    _previewInstance = Instantiate(_placeableItemReference.Item().PlaceablePreview,
                        originalTransform.position, originalTransform.rotation);
            }
        }

        _previewInstance.SetActive(true);
    }

    private void LeavePreviewUpgrade(object sender, UpgradeArgs e)
    {
        if (_previewInstance != null)
        {
            _previewInstance?.SetActive(false);
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

        if (_previewInstance != null)
        {
            _previewInstance.SetActive(false);
            Destroy(_previewInstance);
            _previewInstance = null;
        }

        if (replacement)
        {
            Debug.Log("Destroying " + gameObject.name);
            Destroy(this.gameObject);
        }
    }
}