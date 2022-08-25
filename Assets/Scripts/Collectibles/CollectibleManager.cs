using System;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    public static EventHandler<EventArgs> onAllCollectiblesInhaled;

    public static CollectibleManager Instance;
    Collectible[] collectiblesInScene;
    int numCollected;

    public int NumCollectibles { get {
        return collectiblesInScene.Length;
    } }

    public int NumCollected { get => numCollected; set => numCollected = value; }

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
        NumCollected++;

        if(NumCollected == collectiblesInScene.Length){
            onAllCollectiblesInhaled?.Invoke(this, EventArgs.Empty);
            Debug.Log("Congratulations, all collectibles inhaled");
        }
    }
    
    [ContextMenu("Fire Collectible Event")]
    public void DBG_FireCollectibleEvent(){
        onAllCollectiblesInhaled?.Invoke(this, EventArgs.Empty);
    }

}
