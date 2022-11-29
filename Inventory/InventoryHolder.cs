using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// An Inventory holder is in essence an inventory.
// For cases where we might need 2 inventory systems (such as a hotbar/backpack) in one holder, this can be done here!

[System.Serializable]
public class InventoryHolder : MonoBehaviour
{
    [SerializeField] private int inventorySize;
    // all inventorysystems will inherit from InventorySystem.
    // Some things may have 2 inventory systems (like a players hands/backpack)
    [SerializeField] protected InventorySystem inventorySystem;
    
    // Public getter
    public InventorySystem InventorySystem => inventorySystem;

    public static UnityAction<InventorySystem> OnDynamicInventoryDisplayRequested;

    private void Awake() {
        inventorySystem = new InventorySystem(inventorySize);
    }
}
