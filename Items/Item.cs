using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Item is the functionality of different shop items.

public class Item : MonoBehaviour
{
    public ItemData itemData;
    private SpriteRenderer itemSpriteRenderer;
    private SpriteOutlineControl outlineController;

    void Awake() {
        itemSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        outlineController = gameObject.GetComponent<SpriteOutlineControl>();
        itemSpriteRenderer.sprite = itemData.itemSprite;
    }
    
    #region Outline Functionality

    public void RemoveOutline() {
        SetOutlineThickness(0);
    }
    public void ShowOutline() {
        SetOutlineThickness(itemData.itemOutlineThickness);
    }
    public void SetOutlineThickness(float outlineThickness) {
        outlineController.ChangeOutlineThickness(outlineThickness);
    }
    public void SetOutlineColorActive() {
        outlineController.ChangeOutlineColor(0);
    }
    public void SetOutlineColorError() {
        outlineController.ChangeOutlineColor(1);
    }
    public void SetOutlineColorHighlight() {
        outlineController.ChangeOutlineColor(2);
    }

    #endregion

    

}
