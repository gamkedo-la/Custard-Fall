using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(menuName = "CustardFall/CraftableItem")]
public class CraftableItem : ScriptableObject
{
    [SerializeField] private PlaceableItem item;


    [SerializeField] private List<CraftingRequirement> requirements;
    

    [Serializable]
    public class CraftingRequirement
    {
        [SerializeField]
        private string resourceName;

        [SerializeField]
        private int amount;
        
        public string ResourceName => resourceName;
        public int Amount => amount;
        
    }

    public PlaceableItem Item => item;
    public List<CraftingRequirement> Requirements => requirements;
}