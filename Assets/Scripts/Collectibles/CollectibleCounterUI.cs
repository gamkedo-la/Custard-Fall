using UnityEngine;
using TMPro;
using System;

public class CollectibleCounterUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI counterText;
    [SerializeField] private GameObject questTag;
    [SerializeField] private GameObject questIcon;

    void OnEnable()
    {
        Collectible.onCollectiblePickup += CollectiblePickedUp;

        CollectiblePickedUp(this, 0);
    }

    private void OnDisable()
    {
        Collectible.onCollectiblePickup -= CollectiblePickedUp;
    }

    private void CollectiblePickedUp(object sender, int e)
    {
        if (CollectibleManager.Instance == null)
        {
            Debug.Log("Collectibles in scene but no Collectible Manager");
            return;
        }

        if (CollectibleManager.Instance.NumCollected == 0)
        {
            counterText.text = "";
            Hide();
        }
        else
        {
            counterText.text =
                $"{CollectibleManager.Instance.NumCollected} / {CollectibleManager.Instance.NumCollectibles} reclaimed";
            Show();
        }
    }

    private void Show()
    {
        questTag.SetActive(true);
        questIcon.SetActive(true);
    }

    private void Hide()
    {
        questTag.SetActive(false);
        questIcon.SetActive(false);
    }
}