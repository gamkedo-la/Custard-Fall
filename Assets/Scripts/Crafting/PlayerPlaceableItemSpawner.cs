using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerPlaceableItemSpawner : CraftReceiver
{

    [SerializeField]
    private Player player;
    
    public override void TakePlaceableItem(PlaceableItem item)
    {
        player.EnterPlaceMode(item);
    }
}