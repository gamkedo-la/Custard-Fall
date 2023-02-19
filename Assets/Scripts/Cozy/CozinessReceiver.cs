using UnityEngine;

public class CozinessReceiver : MonoBehaviour
{

    [SerializeField] private float cozyLevel;

    public float CozyLevel => cozyLevel;

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
}