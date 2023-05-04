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
            var upgradableStructureVisual = collision.GetComponentInChildren<UpgradableStructureVisual>();
            if (upgradableStructureVisual != null)
            {
                upgradableStructureVisual.Show();
            }

            _disappearEffect.Reset();
            _timeSinceActivated = Time.time;
            StartCoroutine(DisappearLater(upgradableStructureVisual));
        }
    }

    private IEnumerator DisappearLater(UpgradableStructureVisual upgradableStructureVisual)
    {
        var sinceActivated = _timeSinceActivated;
        yield return new WaitForSecondsRealtime(3f);

        // target did not switch meanwhile
        if (Math.Abs(sinceActivated - _timeSinceActivated) < .1f)
        {
            if (upgradableStructureVisual != null)
            {
                upgradableStructureVisual.Hide();
            }

            Hide();
        }
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
            var upgradableStructureVisual = collision.GetComponentInChildren<UpgradableStructureVisual>();
            if (upgradableStructureVisual != null)
            {
                upgradableStructureVisual.Hide();
            }
            
            Hide();
        }
    }
}