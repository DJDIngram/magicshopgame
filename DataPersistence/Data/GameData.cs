using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class GameData
{
    public List<GridContent> gridContentsList;
    public List<FurnitureContent> furnitureContentsList;
    public Vector3 playerPosition;

    // The values defined in this constructor will be the default values the game starts with when there's no data to load.
    public GameData() {
        this.gridContentsList = new List<GridContent>();
        this.furnitureContentsList = new List<FurnitureContent>();
    }

    // Grid tiles!
    [System.Serializable]
    public class GridContent {
        // For each grid tile, we need:
        public int x, y; // its x-y coordinates.
        public int tId; // The tile that occupies the space

        public GridContent((int x, int y) origin, int tileId) {
            this.x = origin.x;
            this.y = origin.y;
            this.tId = tileId;
        }
    }

    // Furniture!
    [System.Serializable]
    public class FurnitureContent {
       // For each item of furniture, we need:
        public int x, y; // its coordinates
        public int fID; // The ID of the furniture
        public FurnitureObject.Dir fDir;// The furniture's rotation.

        public FurnitureContent((int x, int y) origin, int furnitureID, FurnitureObject.Dir furnitureDir) {
            this.x = origin.x;
            this.y = origin.y;
            this.fID = furnitureID;
            this.fDir = furnitureDir;
        }
    }
}
