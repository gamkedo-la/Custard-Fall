using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(menuName = "CustardFall/CraftableItem")]
public class CraftableItem : ScriptableObject
{
    [SerializeField] private PlaceableItem item;


    [SerializeField] private List<CraftingRequirement> requirements;

    public PlaceableItem Item => item;

    [Serializable]
    public class CraftingRequirement
    {
        [SerializeField]
        private string resourceName;

        [SerializeField]
        private int amount;
    }
}