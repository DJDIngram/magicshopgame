using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorPalette", menuName = "ScriptableObjects/Misc/ColorPalette")]
public class ColourPaletteData : ScriptableObject
{
    public Color activeColor;
    public Color errorColor;
    public Color highlightColor;

    public List<Color> palette;
}
