using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ItemInteraction : MonoBehaviour
{
    // This class covers player item interaction.
    // Current features:
    //  - Item highlighting and swapping active highlighted items when hovered.
    //  - Item pickup with "interact" (currently mapped to F)
    // - 

    // #TODO - do I instead, keep a List<Item> and do a GetComponent<Item>(); 
        // for each item every time it enters the pickup radius..
    private InventoryHolder playerInventoryHolder;
    [SerializeField] private int selectedSlotIndex;
    [SerializeField] private List<GameObject> itemsInPickupRadius;
    private GameObject lastSelectedObject;
    private int selectedPickupItemIndex;
    private Vector3 previousPosition;

    void Awake() {
        selectedPickupItemIndex = 0;
        lastSelectedObject = null;
        playerInventoryHolder = gameObject.GetComponent<InventoryHolder>();
        if (playerInventoryHolder.InventorySystem.InventorySize < 0) {
            Debug.LogWarning("No Inventory Slot? Please Check!");
        } else {
            selectedSlotIndex = 0;
        }
    }

    public UnityAction<int> OnActiveInventorySlotChanged;

    #region Item Pickup
    private void OnTriggerEnter2D (Collider2D collider) {
        if (collider.gameObject.CompareTag("Item")){
            itemsInPickupRadius.Add(collider.gameObject);
            ResetIndex();
        }
    }

    private void OnTriggerExit2D (Collider2D collider) {
        if (collider.gameObject.CompareTag("Item")){
            collider.gameObject.GetComponent<Item>().RemoveOutline();
            itemsInPickupRadius.Remove(collider.gameObject);
            ResetIndex();
        }
    }

    public void HandleItemPickup(InputAction.CallbackContext context) {
        if (context.performed && GameStateManager.instance.gameState == GameState.Playing) {
            PickUpItem();
        }
    }

    public void PickUpItem() {
        // If we have an item selected
        if (itemsInPickupRadius.Count != 0 && itemsInPickupRadius[selectedPickupItemIndex] != null) {
            GameObject itemToPickup = itemsInPickupRadius[selectedPickupItemIndex];
            ItemData itemDataToAdd = itemToPickup.GetComponent<Item>().itemData;
            if (playerInventoryHolder.InventorySystem.AddToInventory(itemDataToAdd, 1)) {
                // if we were able to pickup the item, delete it
                // Remove from itemsinpickupradius
                itemsInPickupRadius.Remove(itemToPickup);
                ResetIndex();
                // Delete the gameobject.
                Destroy(itemToPickup);
            }
            else {
                Debug.Log("Did not pickup item");
            }
        }
        else {
            Debug.Log("No Items in Range!");
        }
    }

    public void HandleIncrementIndexAction(InputAction.CallbackContext context) {
        // When the player has multiple items at their feet, they can press a movement key
            //  to shift the index over one, depending on the item positions and direction
            //      pressed, I.E move up, selects the next highest item.
        if (context.performed && GameStateManager.instance.gameState == GameState.Playing) {
            IncrementIndex(context.ReadValue<Vector2>());
        }
    }

    private void IncrementIndex(Vector2 movementVector) {
        // Increment the index in the direction of the movement vector.
        if (itemsInPickupRadius.Count < 2) {
            // Do nothing with the index.
        } else if (movementVector.x == 1 || movementVector.y == 1) {
            // Increment Index
            if (selectedPickupItemIndex < itemsInPickupRadius.Count - 1) {
                // We can increment the index;
                selectedPickupItemIndex++;
            } else {
                selectedPickupItemIndex = 0;
            }
            ChangeSelectedItem(selectedPickupItemIndex);
        } else if (movementVector.x == -1 || movementVector.y == -1 ) {
            // Decrement index
            if (selectedPickupItemIndex == 0) {
                // 0 -> loop round -> max;
                selectedPickupItemIndex = itemsInPickupRadius.Count - 1;
            } else {
                selectedPickupItemIndex--;
            }
            ChangeSelectedItem(selectedPickupItemIndex);
        } else { } // Do Nothing.
    }

    private void ResetIndex() {
        // if the currently selected item is still in the list, choose that one.
        for (int i = 0; i < itemsInPickupRadius.Count; i++)
        {
            if (itemsInPickupRadius[i] == lastSelectedObject) {
                // Update the selectedPickupItemIndex and return from the function
                selectedPickupItemIndex = i;
                return;
            }
        }
        // Otherwise:
        // Reset the index when an item enters or leaves the itemsInPickupRadius to avoid Out Of Bounds errors.
        if (selectedPickupItemIndex < 0) {
            // If the index is at -1 and the list goes from 0 -> 1, set the index to the min.
            selectedPickupItemIndex = 0;
        } else if (selectedPickupItemIndex >= 0 || selectedPickupItemIndex <= itemsInPickupRadius.Count) {
            // If the index is at max and the list goes from max -> max-1, set the index to the new max.
            selectedPickupItemIndex = itemsInPickupRadius.Count - 1;
        }
        ChangeSelectedItem(selectedPickupItemIndex);


    }

    // Sets the selected item to have an outline, and all others to remove theirs.
    private void ChangeSelectedItem(int itemIndex) {
        // Send an action to the item to highlight it.
        if (itemsInPickupRadius.Count > 0) {
            foreach (GameObject item in itemsInPickupRadius)
            {
                if (item == itemsInPickupRadius[itemIndex]) {
                    Item itemObject = item.GetComponent<Item>();
                    itemObject.SetOutlineColorActive();
                    itemObject.ShowOutline();
                    lastSelectedObject = item;
                } else {
                    item.GetComponent<Item>().RemoveOutline();
                }
            }
        }
    }

    #endregion

    #region Inventory Interaction
        // Includes changing selected inventoryslot.
        public void HandleInventorySlotChangeActive(InputAction.CallbackContext context) {
            if (GameStateManager.instance.gameState == GameState.Playing) {
                float scrollDirection = context.ReadValue<Vector2>().normalized.y;
                if (scrollDirection != 0) IncrementActiveInventorySlot(context.ReadValue<Vector2>().normalized.y, playerInventoryHolder);
            }
        }
        
        private void RecalculateActiveInventorySlot(InventoryHolder inventoryHolder) { IncrementActiveInventorySlot(0, inventoryHolder); }
        private void IncrementActiveInventorySlot(float direction, InventoryHolder inventoryHolder)
        {
            if (direction > 0)
            {
                // Increment Index
                // if number of inventory slots = current index, index is 0, else increment by 1
                if (selectedSlotIndex == inventoryHolder.InventorySystem.InventorySize - 1) {
                    selectedSlotIndex = 0;
                } else {
                    selectedSlotIndex++;
                }
                OnActiveInventorySlotChanged?.Invoke(selectedSlotIndex);
            } else if (direction < 0) {
                // Decrement Index
                // if number of inventory slots = 0, index is number of inventory slots, else decrement by 1
                if (selectedSlotIndex == 0 ) {
                    selectedSlotIndex = inventoryHolder.InventorySystem.InventorySize - 1;
                } else {
                    selectedSlotIndex--;
                }
                OnActiveInventorySlotChanged?.Invoke(selectedSlotIndex);
            } else {
                // if 0 - recalculate index
                //  in the case the number of inventory slots changes and our new index is out of bounds.
                if (selectedSlotIndex - 1 >= inventoryHolder.InventorySystem.InventorySize) {
                    selectedSlotIndex = inventoryHolder.InventorySystem.InventorySize - 1;
                }
                OnActiveInventorySlotChanged?.Invoke(selectedSlotIndex);
            };
        }

    #endregion
}
