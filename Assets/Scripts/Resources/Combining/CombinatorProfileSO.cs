using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CustardFall/CombinatorProfileSO")]
public class CombinatorProfileSO : ScriptableObject
{
    [SerializeField] private LayerMask layerMask;


    public ItemReceiver CanCombineRaycast(PlaceableItem activeItem, Vector3 position)
    {
        Debug.Log("Want to use activeItem " + activeItem.name);
        if (Physics.Raycast(position + 5 * Vector3.up, Vector3.down, out RaycastHit hit, 10f, layerMask.value))
        {
            var itemReceiver = hit.transform.GetComponent<ItemReceiver>();
            Debug.Log("No ItemReceiver found");
            if (itemReceiver != null && itemReceiver.CanReceiveItem(activeItem))
                return itemReceiver;
        }

        Debug.Log("Cannot combine");
        return null;
    }

    public ItemReceiver CanCombine(PlaceableItem activeItem, Transform transform)
    {
        Debug.Log("Want to use activeItem " + activeItem.name);

        var itemReceiver = transform.GetComponent<ItemReceiver>();
        Debug.Log("No ItemReceiver found");
        if (itemReceiver != null && itemReceiver.CanReceiveItem(activeItem))
            return itemReceiver;
        Debug.Log("Cannot combine");
        return null;
    }
}