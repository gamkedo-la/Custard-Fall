using System;
using UnityEngine;
using UnityEngine.Serialization;

public class CozinessReceiver : MonoBehaviour
{

    [SerializeField] private float cozyLevelOfSurrounding;
    [SerializeField] private int personalCozyLevel = 0;
    [SerializeField] private float cozinessTillNextLevel = 0;
    private float _cozinessTillNextLevelLinearInternal = 0;
    [SerializeField] private float baseFillDuration = 7;
    [SerializeField] private CozySettings cozySettings;

    public float CozyLevelOfSurrounding => cozyLevelOfSurrounding;
    public int PersonalCozyLevel => personalCozyLevel;
    public float CozinessTillNextLevel => cozinessTillNextLevel;

    private void Start()
    {
    }

    public void OnCozyReceive(float coziness)
    {
        cozyLevelOfSurrounding += coziness;
    }
    
    public void OnCozyLeave(float coziness)
    {
        cozyLevelOfSurrounding -= coziness;
        if (cozyLevelOfSurrounding < 0)
        {
            cozyLevelOfSurrounding = 0;
        }
    }

    private void Update()
    {
        var possibleValue = GetEnvironmentalCoziness();
        var maxPossibleValue = possibleValue;
        if (personalCozyLevel < maxPossibleValue)
        {
            _cozinessTillNextLevelLinearInternal += Time.deltaTime / baseFillDuration;
            cozinessTillNextLevel = cozySettings.EasingFunction.Evaluate(_cozinessTillNextLevelLinearInternal);
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
        return Mathf.Min(Mathf.Floor(cozyLevelOfSurrounding), cozySettings.Levels.Count - 1);
    }
}