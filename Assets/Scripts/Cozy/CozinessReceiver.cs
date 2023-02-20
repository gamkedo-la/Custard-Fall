using System;
using UnityEngine;

public class CozinessReceiver : MonoBehaviour
{

    [SerializeField] private float cozyLevel;
    [SerializeField] private float feelingCozy = 0;
    [SerializeField] private float baseFillDuration = 30f;
    [SerializeField] private CozySettings cozySettings;
    
    private float _feelingCozy = 0;
    

    public float CozyLevel => cozyLevel;
    public float FeelingCozy => feelingCozy;

    public void OnCozyReceive(float coziness)
    {
        cozyLevel += coziness;
    }
    
    public void OnCozyLeave(float coziness)
    {
        cozyLevel -= coziness;
        if (cozyLevel < 0)
        {
            cozyLevel = 0;
        }
    }

    private void Update()
    {
        var maxValue = Mathf.Floor(cozyLevel);
        if (_feelingCozy < maxValue)
        {
            _feelingCozy += maxValue/baseFillDuration * Time.deltaTime;
            feelingCozy = cozySettings.EasingFunction.Evaluate(_feelingCozy);
        }
        else
        {
            _feelingCozy = maxValue;
        }
    }
}