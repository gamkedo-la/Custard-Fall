using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RadianceReceiver : MonoBehaviour
{
    [SerializeField] private float radiance = 0;
    [SerializeField] private int radianceLevelOfSurrounding;
    [SerializeField] private int personalRadianceLevel = 0;
    [SerializeField] private float baseFillDuration = 3;
    [SerializeField] private RadianceSettings radianceSettings;
    [SerializeField] private float requiredRadianceForLevel = 1;

    public int RadianceLevelOfSurrounding => radianceLevelOfSurrounding;
    public int PersonalRadianceLevel => personalRadianceLevel;
    public float Radiance => radiance;


    public delegate void OnRadianceChangeInZone(int newLevel, int oldLevel);

    public OnRadianceChangeInZone onRadianceChangeInZone;

    public delegate void OnLevelChange(int newLevel, int previousLevel);

    public OnLevelChange onLevelChange;

    private float _bonusValue = 0;
    [SerializeField] private float bonusValueSpeedup = .25f;

    public void UpdateRadianceZoneLevel(int radianceLevel)
    {
        if (radianceLevel == radianceLevelOfSurrounding)
            return;

        onRadianceChangeInZone?.Invoke(radianceLevel, radianceLevelOfSurrounding);
        radianceLevelOfSurrounding = radianceLevel;
    }

    private void Update()
    {
        if (personalRadianceLevel < radianceLevelOfSurrounding)
        {
            var delta = Time.deltaTime / baseFillDuration;
            radiance += delta;

            if (_bonusValue >= 0)
            {
                var valueSpeedup = delta * bonusValueSpeedup;
                _bonusValue -= valueSpeedup;
                radiance += valueSpeedup;
            }

            if (radiance >= GetRequiredRadianceForLevel(personalRadianceLevel + 1))
            {
                radiance = 0;
                onLevelChange?.Invoke(personalRadianceLevel + 1, personalRadianceLevel);
                personalRadianceLevel++;
            }
        }
        else if (_bonusValue >= 0)
        {
            var delta = Time.deltaTime / baseFillDuration;
            _bonusValue -= delta;
            radiance += delta;
        }
    }

    private float GetRequiredRadianceForLevel(int targetLevel)
    {
        return requiredRadianceForLevel;
    }

    public void IncreaseRadiance(float bonus)
    {
        // refactor this later
        _bonusValue += bonus;
    }

    public void TakeDamage(float fraction)
    {
        float threshold = .01f;
        if (radiance > fraction + threshold)
        {
            radiance -= fraction;
        }
        else if (radiance > threshold)
        {
            radiance = 0;
        }
        else
        {
            personalRadianceLevel--;
            radiance = 1f - fraction;
        }
    }
}