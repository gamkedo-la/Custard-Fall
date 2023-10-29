using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RadianceBar : MonoBehaviour
{
    [FormerlySerializedAs("cozinessReceiver")] [SerializeField] private RadianceReceiver radianceReceiver;

    [SerializeField] private Slider slider;

    [SerializeField] private Image fill;
    [SerializeField] private Image backdrop;
    [SerializeField] private TextMeshProUGUI displayText;

    [FormerlySerializedAs("cozySettings")] [SerializeField] private RadianceSettings radianceSettings;

    private int _previousRadianceLevel = 0;

    private void Awake()
    {
        slider.maxValue = 1;
    }

    private void Start()
    {
        _previousRadianceLevel = radianceReceiver.PersonalRadianceLevel;
        ChangeDisplayedLevel(_previousRadianceLevel);
    }


    private void Update()
    {
        if (radianceReceiver.PersonalRadianceLevel != _previousRadianceLevel)
        {
            ChangeDisplayedLevel(radianceReceiver.PersonalRadianceLevel);
        }
        else
        {
            slider.value = Mathf.Lerp(slider.value, radianceReceiver.RadianceTillNextLevel, Time.deltaTime *5f);
        }
    }

    private void ChangeDisplayedLevel(int newRadianceLevelValue)
    {
        RadianceLevel newRadianceLevel;
        if (newRadianceLevelValue <= 0)
        {
            newRadianceLevel = radianceSettings.Levels[0];
            backdrop.color =  radianceSettings.Levels[0].Color;
        }else if (newRadianceLevelValue >= radianceSettings.Levels.Count)
        {
            newRadianceLevel = radianceSettings.Levels[^1];
            backdrop.color =  radianceSettings.Levels[^1].Color;
        }
        else
        {
            newRadianceLevel = radianceSettings.Levels[newRadianceLevelValue];
            backdrop.color = radianceSettings.Levels[newRadianceLevelValue].Color;
        }
        displayText.text = newRadianceLevel.DisplayName;
        fill.color = newRadianceLevel.ProgressColor;

        if (_previousRadianceLevel <= newRadianceLevelValue)
        {
            slider.value = 0;
        }
        else
        {
            slider.value = 1;
        }
        _previousRadianceLevel = newRadianceLevelValue;
    }
}