using System;
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

    [SerializeField] private VisualMode visualMode;
    [SerializeField] private TweenFade fader;

    private float _lastValue;

    private void Awake()
    {
        slider.maxValue = 1;
    }

    private void Start()
    {
        _displayedRadianceLevel = radianceReceiver.PersonalRadianceLevel;
        UpdateSlider(radianceReceiver.Radiance);
    }

    private void Update()
    {
        var progression = radianceReceiver.Radiance;
        _isProgressionMode = progression >= 0;
        _displayedRadianceLevel = radianceReceiver.PersonalRadianceLevel;

        var originalPreviousValue = _lastValue;
        UpdateSlider(progression);
        UpdateVisuals();

        var isIdlePrevious = Math.Abs(originalPreviousValue - _lastValue) < .001f;
        var isIdle = Math.Abs(_lastValue - slider.value) < .001f;
        if (isIdle != isIdlePrevious && visualMode != VisualMode.Visible)
        {
            fader.SetFadeIn(((!isIdle || progression<0 || radianceReceiver.PersonalRadianceLevel == 0) && visualMode == VisualMode.ShowWhenActive) ||
                           ((isIdle && progression>=0 && radianceReceiver.PersonalRadianceLevel != 0) && visualMode == VisualMode.ShowWhenIdle));
            fader.enabled = true;
        }
    }

    private void UpdateSlider(float progression)
    {
        // we reuse the single sliders to display decline (colors are swapped in this case)
        var fillAmount = progression >= 0 ? progression : 1f + progression;
        _lastValue = slider.value;
        slider.value = fillAmount;
    }

    private void UpdateVisuals()
    {
        RadianceLevel radianceInfo = radianceSettings.Levels[_displayedRadianceLevel];
        displayText.text = radianceInfo.DisplayName;

        if (_isProgressionMode)
        {
            fill.color = radianceInfo.ProgressColor;
            backdrop.color = radianceInfo.Color;
        }
        else
        {
            fill.color = radianceInfo.Color;
            backdrop.color = radianceInfo.DeclineColor;
        }
    }

    private enum VisualMode
    {
        Visible,
        ShowWhenIdle,
        ShowWhenActive
    }
}