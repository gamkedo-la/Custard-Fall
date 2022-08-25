using System;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    public static EventHandler<EventArgs> onAllCollectiblesInhaled;

    public static CollectibleManager Instance;
    Collectible[] collectiblesInScene;
    int numCollected;
    void Awake() {

		if (Instance == null) {
			Instance = this;
		} else {
			Destroy(this);
            return;
		}

        collectiblesInScene = FindObjectsOfType<Collectible>();
	}

    private void OnEnable() {
        Collectible.onCollectiblePickup += CollectiblePickedUp;
    }

    private void OnDisable() {
        Collectible.onCollectiblePickup -= CollectiblePickedUp;
    }

    private void CollectiblePickedUp(object sender, int e)
    {
        numCollected++;

        if(numCollected == collectiblesInScene.Length){
            onAllCollectiblesInhaled?.Invoke(this, EventArgs.Empty);
            Debug.Log("Congratulations, all collectibles inhaled");
        }
    }
    
    [ContextMenu("Fire Collectible Event")]
    public void DBG_FireCollectibleEvent(){
        onAllCollectiblesInhaled?.Invoke(this, EventArgs.Empty);
    }

}
