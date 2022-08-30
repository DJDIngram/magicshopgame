using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FurnitureObject", menuName = "ScriptableObjects/Construction/FurnitureObject")]
public class FurnitureObject : ScriptableObject
{
    // This Object sets the basis for all furniture in the game!

    // Does it need to go next to a wall, or is it freestanding?
    public enum FurnitureType {
        Attached,
        Freestanding
    }

    // Eunumerator for different directions the object is facing.
    public enum Dir {
        None, // <- The object does not rotate.
        North,
        East,
        South,
        West,
    }

    public GameObject furniturePrefab;
    public FurnitureType furnitureType;
    public Dir dir;
    public int gridWidth; // X AXIS
    public int gridHeight; // Y AXIS
    public string furnitureName;

    // Returns "the next" dir given the old one.
    public static Dir GetNextDir(Dir dir) {
        switch (dir) {
            default:
            case Dir.None: return Dir.None;
            case Dir.North: return Dir.East;
            case Dir.East:  return Dir.South;
            case Dir.South: return Dir.West;
            case Dir.West:  return Dir.North;
        }
    }

    //Returns the rotation about the y axis that the object is rotated, depending on their direction.
    public int GetRotationAngle(Dir dir) {
        switch (dir) {
            default:
            case Dir.None: 
            case Dir.North: return 0;
            case Dir.East:  return 270;
            case Dir.South:  return 180;
            case Dir.West:  return 90;
        }
    }

    //Returns the X-Y Offset when given a direction.
    public (int x, int y) GetRotationOffset(Dir dir) {
        switch (dir) {
            default:
            case Dir.None:
            case Dir.North: return (0,0);
            case Dir.East: return (0, gridWidth);
            case Dir.South: return (gridWidth, gridHeight);
            case Dir.West: return (gridHeight, 0);
        }
    }

    // Takes a grid offset (x, y) and returns a list of grid positions filled by said object. 
    public List<(int x, int y)> GetGridPositionList((int x, int y) offset, Dir dir) {
        List<(int x, int y)> gridPositionList = new List<(int x, int y)>();
        switch (dir) {
            default:
            case Dir.None:
            case Dir.North:
            case Dir.South:
                for (int x = 0; x < gridWidth; x++) {
                    for (int y = 0; y < gridHeight; y++) {
                        //Add x,y to list.
                        // Debug.Log("Adding " + (x + offset.x) + ", " + (y + offset.y));
                        gridPositionList.Add((x + offset.x, y + offset.y));
                    }
                }
                break;
            case Dir.East:
            case Dir.West:
                for (int x = 0; x < gridHeight; x++) {
                    for (int y = 0; y < gridWidth; y++) {
                        //Add x,y to list.
                        // Debug.Log("Adding " + (x + offset.x) + ", " + (y + offset.y));
                        gridPositionList.Add((x + offset.x, y + offset.y));
                    }
                }
                break;
        }
        return gridPositionList;
    } 
}
