using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerInventoryDisplay : InventoryDisplay
{
    private VisualElement root;
    // Reference the players inventory holder and ui slots.
    [SerializeField] private InventoryHolder inventoryHolder;
    [SerializeField] private List<InventorySlotUI> slots;

    // Subscribe to itemInteraction to change the highlighted inventory slot
    [SerializeField] private ItemInteraction itemInteraction;

    protected override void Start() {
        base.Start();
        if (inventoryHolder != null) {
            inventorySystem = inventoryHolder.InventorySystem;
            inventorySystem.OnInventorySlotChanged += UpdateSlot;
        }
        else {
            Debug.LogWarning("No inventory assigned to " + (this.gameObject));
        }
        // Get the two inventory slot buttons.
        // Pass the inventory slot buttons to the slots list.
        root = GetComponent<UIDocument>().rootVisualElement;
        GetAllInventorySlotButtons().ForEach((Button b_slotButton) => {
            slots.Add(new InventorySlotUI(b_slotButton, this));
        });

        AssignSlot(inventorySystem);

        // Subscribe to events where the player changes their active inventory slot.
        itemInteraction.OnActiveInventorySlotChanged += ChangeActiveInventorySlotDisplay;
        
    }

    void OnDestroy() {
        // Unsubscribe from events.
        itemInteraction.OnActiveInventorySlotChanged -= ChangeActiveInventorySlotDisplay;
    }

    public override void AssignSlot(InventorySystem invToDisplay)
    {
        slotDictionary = new Dictionary<InventorySlotUI, InventorySlot>();

        if (slots.Count != inventorySystem.InventorySize) {
            Debug.LogWarning ("Inventory Slots and UI slots out of sync on " + this.gameObject + "!!!");
        }

        for (int i = 0; i < inventorySystem.InventorySize; i++)
        {
            slotDictionary.Add(slots[i], inventorySystem.InventorySlots[i]);
            slots[i].Init(InventorySystem.InventorySlots[i]);
        }
    }

    private UQueryBuilder<Button> GetAllInventorySlotButtons() {
        return root.Query<Button>(className: "inventory-slot");
    }

    private void ChangeActiveInventorySlotDisplay(int activeInventorySlotIndex) {
        for (int i = 0; i < slots.Count; i++)
        {
            if (i == activeInventorySlotIndex) {
                // if i is the activeinventoryindex and doesnt have the class, add it.
                if (!slots[i].b_slotButton.ClassListContains("inventory-slot-selected")) {
                    slots[i].b_slotButton.AddToClassList("inventory-slot-selected");
                }
            } else {
                // otherwise remove the class if it has it and its not the active slot
                if (slots[i].b_slotButton.ClassListContains("inventory-slot-selected")) {
                    slots[i].b_slotButton.RemoveFromClassList("inventory-slot-selected");
                }
            }
        }
    }

}
