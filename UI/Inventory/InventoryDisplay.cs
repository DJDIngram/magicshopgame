using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// A collection of inventoryslotUI's.
// This is an ABSTRACT CLASS. https://www.w3schools.com/cs/cs_abstract.php

public abstract class InventoryDisplay : MonoBehaviour
{
    // [SerializeField] MouseItemData mouseItemData;
    protected InventorySystem inventorySystem;
    protected Dictionary<InventorySlotUI, InventorySlot> slotDictionary; // Maps a UI square to its inventory slot.

    //Getters: Publicly accessible.
    public InventorySystem InventorySystem => inventorySystem;
    public Dictionary<InventorySlotUI, InventorySlot> SlotDictionary => slotDictionary;

    protected virtual void Start() 
    { 
        
    }

    public abstract void AssignSlot(InventorySystem invToDisplay);

    protected virtual void UpdateSlot(InventorySlot updatedSlot) {
        // Not abstract, this is the same no matter what.

        // Look at each slot to find the slot we want to update.
        foreach (var slot in SlotDictionary) {
            if (slot.Value == updatedSlot) {
                // This is the slot who's UI we want to update
                //... so update it ...
                slot.Key.UpdateUISlot(updatedSlot);
            }
        }
    }

    public void SlotClicked(InventorySlotUI clickedSlot) {
        Debug.Log("Slot Clicked!");
    }
}
