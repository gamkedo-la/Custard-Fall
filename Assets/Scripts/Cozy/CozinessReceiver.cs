using UnityEngine;

public class CozinessReceiver : MonoBehaviour
{

    [SerializeField] private float cozyLevel;

    public float CozyLevel => cozyLevel;

    public void OnCozyReceive(CozyDispenser dispenser)
    {
        cozyLevel += dispenser.Coziness;
    }
    
    public void OnCozyLeave(CozyDispenser dispenser)
    {
        cozyLevel -= dispenser.Coziness;
        if (cozyLevel < 0)
        {
            cozyLevel = 0;
        }
    }
}