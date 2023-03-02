using System;
using UnityEngine;
using UnityEngine.UI;

public class UpgradableStructureVisual : MonoBehaviour
{
    [SerializeField] [Tooltip("Don't forget to assign!")]
    private UpgradeableStructure upgradeableStructure;

    [SerializeField] private Canvas canvas;

    [SerializeField] private Image slots;

    [SerializeField] private Image filledSlots;

    [SerializeField] private Image previewSlots;

    private void Start()
    {
        upgradeableStructure.OnPreviewEnter += OnPreviewEnter;
        upgradeableStructure.OnPreviewLeave += OnPreviewLeave;
        upgradeableStructure.OnProgressToLevelUp += OnProgressToLevelUp;
        upgradeableStructure.OnLevelUp += OnLevelUp;

        Hide();
    }

    private void Hide()
    {
        canvas.gameObject.SetActive(false);
    }

    private void Show()
    {
        canvas.gameObject.SetActive(true);
    }


    private void OnLevelUp(object sender, UpgradeableStructure.UpgradeArgs e)
    {
        slots.fillAmount = e.requiredPoints / 12f;
        previewSlots.fillAmount = 0;
        if (e.maxedOut)
        {
            filledSlots.fillAmount = slots.fillAmount;
        }
        else
        {
            filledSlots.fillAmount = 0;
        }
    }

    private void OnProgressToLevelUp(object sender, UpgradeableStructure.UpgradeArgs e)
    {
        slots.fillAmount = e.requiredPoints / 12f;
        previewSlots.fillAmount = 0;
        filledSlots.fillAmount = e.investedPoints / 12f;
    }

    private void OnPreviewEnter(object sender, UpgradeableStructure.UpgradeArgs e)
    {
        Show();
        slots.fillAmount = e.requiredPoints / 12f;
        previewSlots.fillAmount = (e.investedPoints + 1) / 12f;
        filledSlots.fillAmount = e.investedPoints / 12f;
    }


    private void OnPreviewLeave(object sender, UpgradeableStructure.UpgradeArgs e)
    {
        Hide();
    }
}