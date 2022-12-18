using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerItemCollisionHandler : MonoBehaviour
{
    public TextMeshProUGUI textForItemDisplay;
    private DisappearEffect _disappearEffect;
    private float _timeSinceActivated;

    private void Awake()
    {
        _disappearEffect = textForItemDisplay.GetComponent<DisappearEffect>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        collision.GetComponent<Inhalable>();
        if (collision.GetComponent<Inhalable>() != null)
        {
            textForItemDisplay.enabled = true;
            textForItemDisplay.text = collision.GetComponent<Inhalable>().interactionMessage;
            _disappearEffect.Reset();
            _timeSinceActivated = Time.time;
            StartCoroutine(DisappearLater());
        }
    }

    private IEnumerator DisappearLater()
    {
        var sinceActivated = _timeSinceActivated;
        yield return new WaitForSecondsRealtime(3f);

        // target did not switch meanwhile
        if (Math.Abs(sinceActivated - _timeSinceActivated) < .1f)
            Hide();
    }

    private void Hide()
    {
        _disappearEffect.Activate();
    }

    private void OnTriggerExit(Collider collision)
    {
        collision.GetComponent<Inhalable>();
        if (collision.GetComponent<Inhalable>() != null)
        {
            Hide();
        }
    }
}