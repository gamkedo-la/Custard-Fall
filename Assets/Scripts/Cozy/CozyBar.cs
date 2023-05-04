using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CozyBar : MonoBehaviour
{
    [SerializeField] private CozinessReceiver cozinessReceiver;

    [SerializeField] private Slider slider;

    [SerializeField] private Image fill;
    [SerializeField] private Image backdrop;
    [SerializeField] private TextMeshProUGUI displayText;

    [SerializeField] private CozySettings cozySettings;

    private int _previousCozyLevel = 0;

    private void Awake()
    {
        slider.maxValue = 1;
    }

    private void Start()
    {
        _previousCozyLevel = cozinessReceiver.PersonalCozyLevel;
        ChangeDisplayedLevel(_previousCozyLevel);
    }


    private void Update()
    {
        if (cozinessReceiver.PersonalCozyLevel != _previousCozyLevel)
        {
            ChangeDisplayedLevel(cozinessReceiver.PersonalCozyLevel);
        }
        else
        {
            slider.value = Mathf.Lerp(slider.value, cozinessReceiver.CozinessTillNextLevel, Time.deltaTime *5f);
        }
    }

    private void ChangeDisplayedLevel(int newCozyLevelValue)
    {
        CozyLevel newCozyLevel;
        if (newCozyLevelValue <= 0)
        {
            newCozyLevel = cozySettings.Levels[0];
            backdrop.color =  cozySettings.Levels[0].Color;
        }else if (newCozyLevelValue >= cozySettings.Levels.Count)
        {
            newCozyLevel = cozySettings.Levels[^1];
            backdrop.color =  cozySettings.Levels[^1].Color;
        }
        else
        {
            newCozyLevel = cozySettings.Levels[newCozyLevelValue];
            backdrop.color = cozySettings.Levels[newCozyLevelValue].Color;
        }
        displayText.text = newCozyLevel.DisplayName;
        fill.color = newCozyLevel.ProgressColor;

        if (_previousCozyLevel <= newCozyLevelValue)
        {
            slider.value = 0;
        }
        else
        {
            slider.value = 1;
        }
        _previousCozyLevel = newCozyLevelValue;
    }
}