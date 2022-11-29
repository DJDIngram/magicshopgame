using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is used on sprites with a custom outline shader.
// It provides handy functionality to change the outline thickness and color.

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOutlineControl : MonoBehaviour
{
    private Material spriteMaterial;
    public ColourPaletteData itemSpritePalette;
    void Awake() {
        spriteMaterial = gameObject.GetComponent<SpriteRenderer>().material;
        ChangeOutlineThickness(0.0f);
    }
    
    // Can use an index from a palette, or just a hardcoded color.
    public void ChangeOutlineColor(int paletteColorIndex) {
        ChangeOutlineColor(itemSpritePalette.palette[paletteColorIndex]);
    }
    public void ChangeOutlineColor(Color color) {
        spriteMaterial.SetColor("_Outline_Color", color);
    }

    public void ChangeOutlineThickness(float thickness) {
        spriteMaterial.SetVector("_Outline_Thickness", new Vector2(thickness, 0));
    }
}
