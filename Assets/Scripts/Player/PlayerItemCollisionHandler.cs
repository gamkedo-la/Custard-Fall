using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerItemCollisionHandler : MonoBehaviour
{
    public TextMeshProUGUI textForItemDisplay;

    private void OnTriggerEnter(Collider collision)
    {
        collision.GetComponent<InhaleListener>();
        if (collision.GetComponent<InhaleListener>() != null)
        {
            textForItemDisplay.enabled = true;
            textForItemDisplay.text = collision.GetComponent<InhaleListener>().interactionMessage;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        collision.GetComponent<InhaleListener>();
        if (collision.GetComponent<InhaleListener>() != null)
        {
            textForItemDisplay.enabled = false;
        }
    }
}
