using System;
using UnityEngine;

public class CozinessReceiver : MonoBehaviour
{

    [SerializeField] private float cozyLevel;
    [SerializeField] private float feelingCozy = 0;
    [SerializeField] private float baseFillDuration = 15f;
    

    public float CozyLevel => cozyLevel;
    public float DeelingCozy => feelingCozy;

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
        Debug.Log(maxValue);
        if (feelingCozy < maxValue)
        {
            feelingCozy += maxValue/baseFillDuration * Time.deltaTime;
        }
        else
        {
            feelingCozy = maxValue;
        }
    }
}