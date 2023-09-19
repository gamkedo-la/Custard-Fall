using System;
using UnityEngine;
using UnityEngine.Serialization;

public class CozinessReceiver : MonoBehaviour
{
    [SerializeField] private float cozyLevelOfSurrounding;
    [SerializeField] private int personalCozyLevel = 0;
    [SerializeField] private float cozinessTillNextLevel = 0;
    private float _cozinessTillNextLevelLinearInternal = 0;
    [SerializeField] private float baseFillDuration = 3;
    [SerializeField] private CozySettings cozySettings;

    public float CozyLevelOfSurrounding => cozyLevelOfSurrounding;
    public int PersonalCozyLevel => personalCozyLevel;
    public float CozinessTillNextLevel => cozinessTillNextLevel;


    public delegate void OnCozyEnter(float amount);

    public OnCozyEnter onCozyEnter;

    public delegate void OnCozyLeave(float amount);

    public OnCozyLeave onCozyLeave;

    private float _bonusValue = 0;

    public void OnCozyReceive(float coziness)
    {
        onCozyEnter?.Invoke(coziness);
    }

    public void OnCozyLost(float coziness)
    {
        onCozyLeave?.Invoke(coziness);
    }

    private void Update()
    {
        var possibleValue = GetEnvironmentalCoziness();
        var maxPossibleValue = (possibleValue == 0 ? personalCozyLevel : possibleValue) + _bonusValue;
        if (personalCozyLevel < maxPossibleValue)
        {
            var delta = Time.deltaTime / baseFillDuration;
            _cozinessTillNextLevelLinearInternal += delta;


            cozinessTillNextLevel = cozySettings.EasingFunction.Evaluate(_cozinessTillNextLevelLinearInternal);
            if (_bonusValue > 0)
            {
                _bonusValue -= cozySettings.EasingFunction.Evaluate(_cozinessTillNextLevelLinearInternal + delta) - cozinessTillNextLevel;
            }
            else
            {
                _bonusValue = 0;
            }
            if (cozinessTillNextLevel >= 1)
            {
                _cozinessTillNextLevelLinearInternal = 0;
                cozinessTillNextLevel = 0;
                personalCozyLevel++;
            }
        }
    }

    private float GetEnvironmentalCoziness()
    {
        return Mathf.Min(Mathf.Floor(CozinessManager.Instance.GetTotalCoziness(this)), cozySettings.Levels.Count - 1);
    }

    public void IncreaseRadiance(float bonus)
    {
        // refactor this later
        _bonusValue += bonus;
        onCozyEnter?.Invoke(bonus);
    }

    public void TakeDamage(float fraction)
    {
        float threshold = .01f;
        if (cozinessTillNextLevel > fraction + threshold)
        {
            cozinessTillNextLevel -= fraction;
        }
        else if (cozinessTillNextLevel > threshold)
        {
            cozinessTillNextLevel = 0;
        }
        else
        {
            personalCozyLevel--;
            cozinessTillNextLevel = 1f - fraction;
        }
    }
}