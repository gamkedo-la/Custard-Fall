using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CraftReceiver))]
public class Crafter : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private CraftReceiver craftReceiver;

    public void CraftItem(CraftableItem item)
    {
        Debug.Log("Crafting item");
        var cost = CalculateCost(item, out var craftingRequirements);
        
        if (craftingRequirements.Count == 0)
        {
            foreach (var costForSlot in cost)
            {
                var inventorySlot = costForSlot.Key;
                inventory.AddOrSubResourceAmount(inventorySlot.Resource, -costForSlot.Value);
                craftReceiver.TakePlaceableItem(item.Item);
            }
        }
        else
        {
            // TODO feedback about failure, though should not have been 
            Debug.Log("Could not craft");
        }
    }

    public bool CanCraftItem(CraftableItem item)
    {
        return CalculateCost(item, out _).Count > 0;
    }

    private Dictionary<Inventory.InventorySlot, int> CalculateCost(CraftableItem item,
        out List<CraftableItem.CraftingRequirement> craftingRequirements)
    {
        Dictionary<Inventory.InventorySlot, int> cost = new Dictionary<Inventory.InventorySlot, int>();
        craftingRequirements = new List<CraftableItem.CraftingRequirement>(item.Requirements);
        foreach (var inventorySlot in inventory.GetResourceList())
        {
            foreach (var requirement in craftingRequirements)
            {
                if (requirement.ResourceName == inventorySlot.Resource.Name &&
                    inventorySlot.Amount >= requirement.Amount)
                {
                    cost.Add(inventorySlot, requirement.Amount);
                    craftingRequirements.Remove(requirement);
                    if (craftingRequirements.Count == 0)
                    {
                        return cost;
                    }
                }
            }
        }

        return cost;
    }
}