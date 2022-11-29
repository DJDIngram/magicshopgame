using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class InventorySlotUI 
{
    private VisualElement itemIcon;
    private Label l_itemAmount;
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private string itemCount;
    private InventorySlot assignedInventorySlot;
    public Button b_slotButton { get; private set; }

    public InventorySlot AssignedInventorySlot => assignedInventorySlot;
    public InventoryDisplay ParentDisplay { get; private set; }

    public InventorySlotUI(Button button, InventoryDisplay parentDisplay) {
        itemIcon = button.Q("ItemIcon");
        l_itemAmount = button.Q<Label>("ItemAmount");
        // When we create the slot, clear its properties.
        ClearSlot();
        // Then register a callback for when its clicked.
        b_slotButton = button;
        b_slotButton.RegisterCallback<ClickEvent>(OnUISlotClick);
        ParentDisplay = parentDisplay;
    }

    public void Init(InventorySlot slot) {
        assignedInventorySlot = slot;
        UpdateUISlot(slot);
    }

    public void UpdateUISlot(InventorySlot slot) {
        // When we initialize the slot with an item, change the sprite and the tintcolor
        if (slot.Item != null) {
            itemSprite = slot.Item.itemSprite;
            itemIcon.style.backgroundImage = new StyleBackground(itemSprite);
            
            // if there's only one item in the stack, dont display the number.
            if (slot.StackSize > 1 ) {
                itemCount = slot.StackSize.ToString();
            } else {
                itemCount = "";
            }

            l_itemAmount.text = itemCount;
            
        } else {
            ClearSlot();
        }

    }

    public void UpdateUISlot() {
        // essentially, refresh the slot.
        if (assignedInventorySlot != null) UpdateUISlot(assignedInventorySlot);
    }

    public void ClearSlot() {
        itemSprite = null;
        itemIcon.style.backgroundImage = new StyleBackground(itemSprite);
        itemCount = "";
    }

    public void OnUISlotClick(ClickEvent evt) {
        ParentDisplay?.SlotClicked(this);
        Debug.Log("Clicked!");
    }

}
