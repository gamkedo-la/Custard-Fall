using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UpgradableStructureVisual : MonoBehaviour
{
    [SerializeField] [Tooltip("Don't forget to assign!")]
    private UpgradeableStructure upgradeableStructure;

    [SerializeField] private Canvas canvas;

    [SerializeField] private Image slots;

    [SerializeField] private Image filledSlots;

    [SerializeField] private Image previewSlots;
    [SerializeField] private TextMeshProUGUI lvlDisplay;
    [SerializeField] private Image lvlBackground;
    [SerializeField] private Image resourceIcon;
    [FormerlySerializedAs("_cozySettings")] [SerializeField] private RadianceSettings radianceSettings;

    [SerializeField] private bool autoHide = true;
    public bool AutoHide => autoHide;

    private float targetAlpha;
    private bool doFade;
    private Action onFadeComplete;

    private void Start()
    {
        upgradeableStructure.OnPreviewEnter += OnPreviewEnter;
        upgradeableStructure.OnPreviewLeave += OnPreviewLeave;
        upgradeableStructure.OnProgressToLevelUp += OnProgressToLevelUp;
        upgradeableStructure.OnLevelUp += OnLevelUp;
        
        UpdateResourceIcon();
        
        PrepareLvlDisplay(upgradeableStructure.CurrentLevel());
        CenterSlots(upgradeableStructure.RequieredPoints());
        PrepareRequiredSlots(upgradeableStructure.RequieredPoints());
        PreparePreviewPoints(upgradeableStructure.InvestedPoints());

        if (autoHide)
            HideImmediately();
    }

    private void UpdateResourceIcon()
    {
        resourceIcon.sprite = upgradeableStructure?.ExpectedUpgradeMaterial?.Icon;
    }

    private void PrepareLvlDisplay(int currentLevel)
    {
        lvlBackground.color = radianceSettings.GetColorForEffectiveLevel(currentLevel);
        lvlDisplay.SetText("lvl " + currentLevel);
    }

    private void CenterSlots(int requieredPoints)
    {
        RectTransform currentTransform = slots.GetComponent<RectTransform>();
        var tmpPosition = currentTransform.localPosition;
        var centered = new Vector3(1.15f * (1f - (requieredPoints - 1) / 11f), tmpPosition.y, tmpPosition.z);
        currentTransform.localPosition = centered;

        filledSlots.GetComponent<RectTransform>().localPosition = centered;
        previewSlots.GetComponent<RectTransform>().localPosition = centered;
    }

    private void Update()
    {
        if (doFade)
        {
            var materialColor = slots.material.color;
            if (Mathf.Abs(materialColor.a - targetAlpha) <= 0.01)
            {
                materialColor.a = targetAlpha;
                slots.material.color = materialColor;
                onFadeComplete?.Invoke();
                doFade = false;
            }
        }
    }

    public void Hide()
    {
        doFade = true;
        targetAlpha = 0f;
        slots.CrossFadeAlpha(targetAlpha, .3f, false);
        filledSlots.CrossFadeAlpha(targetAlpha, .3f, false);
        previewSlots.CrossFadeAlpha(targetAlpha, .3f, false);
        // XXX something wrong on fade complete
        lvlDisplay.gameObject.SetActive(false);
        lvlBackground.gameObject.SetActive(false);
        onFadeComplete = HideImmediately;
    }

    private void HideImmediately()
    {
        doFade = false;
        targetAlpha = 0f;
        lvlDisplay.gameObject.SetActive(false);
        lvlBackground.gameObject.SetActive(false);
        canvas.gameObject.SetActive(false);
    }

    public void Show()
    {
        doFade = true;
        targetAlpha = 1f;
        slots.CrossFadeAlpha(targetAlpha, .3f, false);
        filledSlots.CrossFadeAlpha(targetAlpha, .3f, false);
        previewSlots.CrossFadeAlpha(targetAlpha, .3f, false);
        onFadeComplete = null;
        lvlDisplay.gameObject.SetActive(true);
        lvlBackground.gameObject.SetActive(true);
        canvas.gameObject.SetActive(true);
    }


    private void OnLevelUp(object sender, UpgradeableStructure.UpgradeArgs e)
    {
        PrepareLvlDisplay(e.level);
        CenterSlots(e.requiredPoints);
        PrepareRequiredSlots(e.requiredPoints);
        PreparePreviewPoints(0);
        if (e.maxedOut)
        {
            // visualize maxed out by hiding any slots
            filledSlots.fillAmount = slots.fillAmount;
            filledSlots.fillAmount = 0;
            slots.fillAmount = 0;
            previewSlots.fillAmount = 0;
        }
        else
        {
            PrepareInvestedSlots(0);
        }

        Show();
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
        PrepareRequiredSlots(e.requiredPoints);
        var investedPoints = e.investedPoints;
        PreparePreviewPoints(investedPoints);
        PrepareInvestedSlots(investedPoints);
    }

    private void PrepareInvestedSlots(int investedPoints)
    {
        filledSlots.fillAmount = investedPoints / 12f;
    }

    private void PreparePreviewPoints(int investedPoints)
    {
        // previewSlots.fillAmount = (investedPoints + 1) / 12f;
        previewSlots.fillAmount = 0;
    }

    private void PrepareRequiredSlots(int requiredPoints)
    {
        slots.fillAmount = requiredPoints / 12f;
    }


    private void OnPreviewLeave(object sender, UpgradeableStructure.UpgradeArgs e)
    {
        if (autoHide)
            Hide();
    }
    
}