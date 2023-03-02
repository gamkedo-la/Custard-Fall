using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CustardFall/CombinatorProfileSO")]
public class CombinatorProfileSO : ScriptableObject
{

    [SerializeField] private LayerMask layerMask;
    

    public ItemReceiver CanCombine(PlaceableItem activeItem, Vector3 position)
    {
            Debug.Log("Want to use activeItem "+activeItem.name);
        if (Physics.Raycast(position + 5*Vector3.up, Vector3.down, out RaycastHit hit, 10f, layerMask.value))
        {
            Debug.Log("At least found something");
            var itemReceiver = hit.transform.GetComponent<ItemReceiver>();
            if (itemReceiver != null && itemReceiver.CanReceiveItem(activeItem))
                return itemReceiver;
        }
        return null;
    }
    
}