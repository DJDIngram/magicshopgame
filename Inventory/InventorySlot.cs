using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class InventorySlot
{
    // Each "square" of a chest, or hand of a creature is an inventory slot.
    // This inventory slot contains an item and its amount. Ie you can only carry 1 sword in your hand, but a handful of coins
    [SerializeField] private ItemData item;
    [SerializeField] private int stackSize;

    // Public references for inventoryslot parameters.
    public ItemData Item => item;
    public int StackSize => stackSize;

    public InventorySlot(ItemData itemData, int amount) {
        item = itemData;
        stackSize = amount;
    }

    public InventorySlot() {
        ClearSlot();
    }

    public void ClearSlot() {
        // Reset the inventory slot.
        item = null;
        stackSize = -1;
    }

    public void UpdateInventorySlot(ItemData itemData, int amount) {
        item = itemData;
        stackSize = amount;
    }

    public bool RoomLeftInStack(int amountToAdd, out int amountRemaining) {
        // Used for splitting/combining stacks
        amountRemaining = item.maxStackSize - stackSize;
        return RoomLeftInStack(amountToAdd);
    }
    public bool RoomLeftInStack(int amountToAdd) { 
        if (stackSize + amountToAdd <= item.maxStackSize) return true;
        else return false;
    }

    public void AddToStack(int amount) {
        stackSize += amount;
    }

    public void RemoveFromStack(int amount) {
        stackSize -= amount;
    }

}
