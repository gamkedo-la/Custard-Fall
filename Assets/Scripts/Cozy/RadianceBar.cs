using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RadianceBar : MonoBehaviour
{
    [SerializeField] private RadianceReceiver radianceReceiver;

    [SerializeField] private Slider slider;

    [SerializeField] private Image fill;
    [SerializeField] private Image backdrop;
    [SerializeField] private TextMeshProUGUI displayText;

    [SerializeField] private RadianceSettings radianceSettings;

    private int _displayedRadianceLevel = 0;
    private bool _isProgressionMode = true;

    private void Awake()
    {
        slider.maxValue = 1;
    }

    private void Start()
    {
        _displayedRadianceLevel = radianceReceiver.PersonalRadianceLevel;
        UpdateLevelDisplay(_displayedRadianceLevel, radianceReceiver.Radiance);

        radianceReceiver.onLevelChange +=
            (newLevel, oldLevel) => OnLevelChange(newLevel);
    }

    private void OnLevelChange(int newLevel)
    {
        _displayedRadianceLevel = newLevel;
        UpdateLevelDisplay(newLevel, _displayedRadianceLevel);
    }


    private void Update()
    {
        if (radianceReceiver.PersonalRadianceLevel != _displayedRadianceLevel)
        {
            UpdateLevelDisplay(radianceReceiver.PersonalRadianceLevel, radianceReceiver.Radiance);
        }
        else
        {
            slider.value = radianceReceiver.Radiance >= 0 ? radianceReceiver.Radiance : 1f - radianceReceiver.Radiance;
        }
    }

    private void UpdateLevelDisplay(int newRadianceLevelValue, float radianceReceiverRadianceTillNextLevel)
    {
        int index;
        int previousIndex;
        int successorIndex;
        if (newRadianceLevelValue <= 0)
        {
            index = previousIndex = 0;
            successorIndex = 1;
        }
        else if (newRadianceLevelValue >= radianceSettings.Levels.Count)
        {
            previousIndex = radianceSettings.Levels.Count - 2;
            index = successorIndex = radianceSettings.Levels.Count - 1;
        }
        else
        {
            previousIndex = newRadianceLevelValue - 1;
            index = newRadianceLevelValue;
            successorIndex = newRadianceLevelValue + 1;
        }

        RadianceLevel newRadianceLevel = radianceSettings.Levels[index];

        displayText.text = newRadianceLevel.DisplayName;


        if (_displayedRadianceLevel <= newRadianceLevelValue)
        {
            slider.value = 0;

            if (radianceReceiverRadianceTillNextLevel < 0 && _isProgressionMode)
            {
                _isProgressionMode = false;
                fill.color = newRadianceLevel.Color;
                backdrop.color = radianceSettings.Levels[previousIndex].BackColor;
            }
            else if (radianceReceiverRadianceTillNextLevel >= 0)
            {
                _isProgressionMode = true;
                fill.color = radianceSettings.Levels[successorIndex].ProgressColor;
                backdrop.color = newRadianceLevel.BackColor;
            }
        }
        else
        {
            slider.value = 1;

            if (radianceReceiverRadianceTillNextLevel < 0 && _isProgressionMode)
            {
                _isProgressionMode = false;
                fill.color = newRadianceLevel.Color;
                backdrop.color = radianceSettings.Levels[previousIndex].BackColor;
            }
            else if (radianceReceiverRadianceTillNextLevel >= 0 && !_isProgressionMode)
            {
                _isProgressionMode = true;
                fill.color = radianceSettings.Levels[successorIndex].ProgressColor;
                backdrop.color = newRadianceLevel.Color;
            }
        }

        _displayedRadianceLevel = newRadianceLevelValue;
    }
}