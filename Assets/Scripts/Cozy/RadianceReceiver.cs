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

    [SerializeField] private float refillRate = 0.05f;
    [SerializeField] private float refillTillLevel = 1;
    [SerializeField] private float refillStartDelay = 1.33f;
    [SerializeField] private bool refillIfDeclining = true;
    
    public int RadianceLevelOfSurrounding => radianceLevelOfSurrounding;
    public int PersonalRadianceLevel => personalRadianceLevel;
    public float Radiance => radiance;

    private bool _fillingUp = false;

    public delegate void OnRadianceChangeInZone(int newLevel, int oldLevel);

    public OnRadianceChangeInZone onRadianceChangeInZone;

    public delegate void OnLevelChange(int newLevel, int previousLevel);

    public OnLevelChange onLevelChange;

    private float _bonusValue = 0;
    [SerializeField] private float bonusValueSpeedup = .25f;
    private Coroutine _delayedRefill;

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
        var radianceBefore = radiance;
        float deltaRadiance = 0;
        if (personalRadianceLevel < radianceLevelOfSurrounding)
        {
            var deltaLeveling = Time.deltaTime / baseFillDuration;
            deltaRadiance += deltaLeveling;
        }
        else if (personalRadianceLevel == radianceLevelOfSurrounding && radiance < 0)
        {
            deltaRadiance += Time.deltaTime / baseFillDuration;
        }

        if (_bonusValue >= 0)
        {
            var deltaBonus = Time.deltaTime / baseFillDuration * bonusValueSpeedup;
            _bonusValue -= deltaBonus;
            deltaRadiance += deltaBonus;
        }

        if (deltaRadiance != 0)
        {
            StopDelayedRefill();
        }
        else switch (_fillingUp)
        {
            case false when refillRate > 0 &&
                            (personalRadianceLevel < refillTillLevel || refillIfDeclining && radiance < 0):
                StartDelayedRefill();
                break;
            case true when refillIfDeclining && radiance >= 0 && personalRadianceLevel >= refillTillLevel:
                StopDelayedRefill();
                break;
        }

        radiance += deltaRadiance;

        if (radianceBefore < 0 && radiance is > 0 and < .05f)
        {
            // filled up negative radiance
            radiance = 0;
        }

        if (radiance >= requiredRadianceForLevelUp)
        {
            ChangeLevel(personalRadianceLevel + 1);
        }
    }

    private void StartDelayedRefill()
    {
        _fillingUp = true;
        _delayedRefill = StartCoroutine(DelayedRefill(refillStartDelay));
    }

    private IEnumerator DelayedRefill(float delay)
    {
        yield return new WaitForSeconds(delay);
        while (_fillingUp)
        {
            var radianceBefore = radiance;
            radiance += refillRate * Time.deltaTime;
            if (radianceBefore < 0 && radiance > 0)
            {
                radiance = 0;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private void StopDelayedRefill()
    {
        if (_fillingUp && _delayedRefill != null)
        {
            StopCoroutine(_delayedRefill);
        }

        _fillingUp = false;
    }

    private void ChangeLevel(int radianceLevel)
    {
        if (radianceLevel >= 0 && radianceLevel <= radianceSettings.Levels.Count - 1)
        {
            var oldLevel = personalRadianceLevel;
            personalRadianceLevel = radianceLevel;
            radiance = 0;
            StopDelayedRefill();
            onLevelChange?.Invoke(personalRadianceLevel, oldLevel);
        }
    }

    public void IncreaseRadiance(float bonus)
    {
        _bonusValue += bonus;
    }

    public void DeclineRadiance(float amount)
    {
        StopDelayedRefill();
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