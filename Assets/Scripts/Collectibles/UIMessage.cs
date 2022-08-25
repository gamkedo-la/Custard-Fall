using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class UIMessage : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI congratsMsg;
    [SerializeField] float messageFadeInDuration, messageShowDuration, messageFadeOutDuration;

    private void OnEnable() {
        CollectibleManager.onAllCollectiblesInhaled += AllCollectiblesInhaled;
    }

    private void OnDisable() {
        CollectibleManager.onAllCollectiblesInhaled -= AllCollectiblesInhaled;
    }

    private void AllCollectiblesInhaled(object sender, EventArgs e)
    {
        StartCoroutine(MessageShow());
        
    }

    IEnumerator MessageShow(){
        congratsMsg.enabled = true;
        float currentTime = 0;

        while((currentTime / messageFadeInDuration) < 1){
            currentTime += Time.deltaTime;
            congratsMsg.alpha = (currentTime / messageFadeInDuration);
            yield return null;

        }

        congratsMsg.alpha = 1;

        currentTime = 0;

        while(currentTime  < messageShowDuration){
            currentTime += Time.deltaTime;
            yield return null;
        }

        currentTime = 0;

        while((currentTime / messageFadeOutDuration) < 1){
            currentTime += Time.deltaTime;
            congratsMsg.alpha = 1 - (currentTime / messageFadeOutDuration);
            yield return null;

        }

        congratsMsg.alpha = 0f;
        congratsMsg.enabled = false;

    }
}
