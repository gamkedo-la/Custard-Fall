using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CustardFall/CombinatorProfileSO")]
public class CombinatorProfileSO : ScriptableObject
{
    private Dictionary<PlaceableItem, HashSet<PlaceableItem>> possibleCombinations = new();

    [SerializeField] private Combination[] combinationMatrix;
    [SerializeField] private LayerMask layerMask;

    private void Awake()
    {
        foreach (var combo in combinationMatrix)
        {
            var possibilities = possibleCombinations.GetValueOrDefault(combo.activeItem, new HashSet<PlaceableItem>());
            possibilities.Add(combo.focusedItem);
            possibleCombinations.TryAdd(combo.activeItem, possibilities);
        }
    }


    public ItemReceiver CanCombine(PlaceableItem activeItem, Vector3 position)
    {
        if (Physics.Raycast(position + Vector3.up, Vector3.down, out RaycastHit hit, 3f, layerMask.value))
        {
            var itemReceiver = hit.transform.GetComponent<ItemReceiver>();
            if (itemReceiver != null && itemReceiver.CanReceiveItem(activeItem))
                return itemReceiver;
        }

        return null;
    }


    [Serializable]
    public class Combination
    {
        public PlaceableItem activeItem;
        public PlaceableItem focusedItem;
    }
}