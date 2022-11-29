using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName= "ItemData", menuName="ScriptableObjects/Inventory/Items")]
public class ItemData : ScriptableObject
{
    // This contains the data for an item. Pretty self-explanatory.
    public int itemId;
    public string itemName;
    public int maxStackSize;
    public Sprite itemSprite;
    public float itemOutlineThickness;
}
