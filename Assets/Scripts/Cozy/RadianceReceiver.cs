using System;
using UnityEngine;
using UnityEngine.Serialization;

public class RadianceReceiver : MonoBehaviour
{
    [FormerlySerializedAs("cozyLevelOfSurrounding")] [SerializeField] private float radianceLevelOfSurrounding;
    [FormerlySerializedAs("personalCozyLevel")] [SerializeField] private int personalRadianceLevel = 0;
    [FormerlySerializedAs("cozinessTillNextLevel")] [SerializeField] private float radianceTillNextLevel = 0;
    private float _radianceTillNextLevelLinearInternal = 0;
    [SerializeField] private float baseFillDuration = 3;
    [FormerlySerializedAs("cozySettings")] [SerializeField] private RadianceSettings radianceSettings;

    public float RadianceLevelOfSurrounding => radianceLevelOfSurrounding;
    public int PersonalRadianceLevel => personalRadianceLevel;
    public float RadianceTillNextLevel => radianceTillNextLevel;


    public delegate void OnRadianceEnter(float amount);

    public OnRadianceEnter onRadianceEnter;

    public delegate void OnRadianceLeave(float amount);

    public OnRadianceLeave onRadianceLeave;

    private float _bonusValue = 0;

    public void OnRadianceReceive(float radiance)
    {
        onRadianceEnter?.Invoke(radiance);
    }

    public void OnRadianceLost(float radiance)
    {
        onRadianceLeave?.Invoke(radiance);
    }

    private void Update()
    {
        var possibleValue = GetEnvironmentalRadiance();
        var maxPossibleValue = (possibleValue == 0 ? personalRadianceLevel : possibleValue) + _bonusValue;
        if (personalRadianceLevel < maxPossibleValue)
        {
            var delta = Time.deltaTime / baseFillDuration;
            _radianceTillNextLevelLinearInternal += delta;


            radianceTillNextLevel = radianceSettings.EasingFunction.Evaluate(_radianceTillNextLevelLinearInternal);
            if (_bonusValue > 0)
            {
                _bonusValue -= radianceSettings.EasingFunction.Evaluate(_radianceTillNextLevelLinearInternal + delta) - radianceTillNextLevel;
            }
            else
            {
                _bonusValue = 0;
            }
            if (radianceTillNextLevel >= 1)
            {
                _radianceTillNextLevelLinearInternal = 0;
                radianceTillNextLevel = 0;
                personalRadianceLevel++;
            }
        }
    }

    private float GetEnvironmentalRadiance()
    {
        return Mathf.Min(Mathf.Floor(RadianceManager.Instance.GetTotalRadiance(this)), radianceSettings.Levels.Count - 1);
    }

    public void IncreaseRadiance(float bonus)
    {
        // refactor this later
        _bonusValue += bonus;
        onRadianceEnter?.Invoke(bonus);
    }

    public void TakeDamage(float fraction)
    {
        float threshold = .01f;
        if (radianceTillNextLevel > fraction + threshold)
        {
            radianceTillNextLevel -= fraction;
        }
        else if (radianceTillNextLevel > threshold)
        {
            radianceTillNextLevel = 0;
        }
        else
        {
            personalRadianceLevel--;
            radianceTillNextLevel = 1f - fraction;
        }
    }
}