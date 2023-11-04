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
    [SerializeField] private float requiredRadianceForLevelUp = 1;
    [SerializeField] private float requiredRadianceForLevelDown = 1;

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

        var oldLevel = radianceLevelOfSurrounding;
        radianceLevelOfSurrounding = radianceLevel;
        onRadianceChangeInZone?.Invoke(radianceLevelOfSurrounding, oldLevel);
    }

    private void Update()
    {
        if (personalRadianceLevel < radianceLevelOfSurrounding)
        {
            var deltaLeveling = Time.deltaTime / baseFillDuration;
            radiance += deltaLeveling;
        } else if (personalRadianceLevel == radianceLevelOfSurrounding && radiance < 0)
        {
            var deltaRefill = Time.deltaTime / baseFillDuration;
            radiance += deltaRefill;
            if (radiance > 0)
                radiance = 0;
        }

        if (_bonusValue >= 0)
        {
            var deltaBonus = Time.deltaTime / baseFillDuration * bonusValueSpeedup;
            _bonusValue -= deltaBonus;
            radiance += deltaBonus;
        }

        if (radiance >= requiredRadianceForLevelUp)
        {
            ChangeLevel(personalRadianceLevel + 1);
        }
    }

    private void ChangeLevel(int radianceLevel)
    {
        if (radianceLevel >= 0 && radianceLevel <= radianceSettings.Levels.Count - 1)
        {
            var oldLevel = personalRadianceLevel;
            personalRadianceLevel = radianceLevel;
            radiance = 0;
            onLevelChange?.Invoke(personalRadianceLevel, oldLevel);
        }
    }

    public void IncreaseRadiance(float bonus)
    {
        // refactor this later
        _bonusValue += bonus;
    }

    public void DeclineRadiance(float amount)
    {
        Debug.Log($"Taking damage {amount}");
        if (personalRadianceLevel == 0 && amount >= radiance - 0.001f)
        {
            radiance = 0;
            return;
        }

        radiance -= amount;
        if (radiance < -requiredRadianceForLevelDown + 0.001f)
        {
            radiance = -requiredRadianceForLevelDown;
        }
        else if (Math.Abs(radiance) <= 0.001f)
        {
            radiance = 0;
        }

        if (radiance <= -requiredRadianceForLevelDown)
        {
            ChangeLevel(personalRadianceLevel - 1);
        }
    }
}