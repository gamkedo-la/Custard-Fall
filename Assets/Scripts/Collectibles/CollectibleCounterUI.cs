using UnityEngine;
using TMPro;
using System;

public class CollectibleCounterUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI counterText;

    void OnEnable()
    {
        Collectible.onCollectiblePickup += CollectiblePickedUp;

        CollectiblePickedUp(this, 0);
    }

    private void OnDisable() {
        Collectible.onCollectiblePickup -= CollectiblePickedUp;
    }

    private void CollectiblePickedUp(object sender, int e)
    {
        if(CollectibleManager.Instance == null){
            Debug.Log("Collectibles in scene but no Collectible Manager");
            return;
        }

        counterText.text = $"{CollectibleManager.Instance.NumCollected} / {CollectibleManager.Instance.NumCollectibles} Collected";
    }

}
