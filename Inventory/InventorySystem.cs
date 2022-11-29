using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

// An inventory system is a collection of inventory slots, and a number of functions to manipulate these slots.

[System.Serializable]
public class InventorySystem
{
    [SerializeField] private List<InventorySlot> inventorySlots;
    
    public List<InventorySlot> InventorySlots => inventorySlots;
    public int InventorySize => InventorySlots.Count;

    public UnityAction<InventorySlot> OnInventorySlotChanged;

    public InventorySystem(int size) {
        //Creates an Inventory Slot list of a specified size.
        inventorySlots = new List<InventorySlot>(size);

        for (int i = 0; i < size; i++) {
            inventorySlots.Add(new InventorySlot());
        }

        Debug.Log("Added " + size + " slots. -> " + InventorySlots.Count);
    }

    public bool ContainsItem(ItemData itemToAdd, out List<InventorySlot> invSlotList) {
        // Create a list of inventory slots where the item in the slot is of the same type we want to add to.
        invSlotList = InventorySlots.Where(i => i.Item == itemToAdd).ToList();
        // If the inventory system contains any slots that have the item we'd like to add
        // return true, otherwise there is none of this new item anywhere in the inventory-
        //  we wont have made a list, so list will be null, so return false.
        return invSlotList == null ? false : true;

    }

    public bool HasFreeSlot(out InventorySlot freeSlot) {
        // Checks if we have a free InventorySlot; and if so, gets it and returns true;
        freeSlot = InventorySlots.FirstOrDefault(i => i.Item == null);
        return freeSlot == null ? false : true;
    }

    public bool AddToInventory(ItemData itemToAdd, int amountToAdd) {
        if (ContainsItem(itemToAdd, out List<InventorySlot> invSlotList)) {   // Check whether item exists in inventory
            // For each of the inventory slots that contain the item we'd like to add,
            //      if there's room in the stack, add to it!
            foreach (InventorySlot invSlot in invSlotList)
            {
                if (invSlot.RoomLeftInStack(amountToAdd)) {
                    // 
                    invSlot.AddToStack(amountToAdd);
                    // If there are listeners to the event, invoke the function and pass through invSlot;
                    OnInventorySlotChanged?.Invoke(invSlot);
                    return true;
                }
            }
        }

        if (HasFreeSlot(out InventorySlot freeSlot)) { // Gets the first available slot
            freeSlot.UpdateInventorySlot(itemToAdd, amountToAdd);
            OnInventorySlotChanged?.Invoke(freeSlot);
            return true;
        }
        return false;
    }
}
