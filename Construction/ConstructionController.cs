using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionController : MonoBehaviour, IDataPersistence
{

    //Create an instance of our grid - filled with type "Constructions"
    private CustomGrid<Construction> constructionController;
    //Instantiates the grid with its centre in the middle of a given position.
    private Vector3 offsetPosition;
    
    //Pass a reference of this grid to the TileController and CollisionController, so we can send updates when a tile needs changing!
    [SerializeField] private TileController tileController;
    [SerializeField] private CollisionController collisionController;
    [SerializeField] private PlayerConstruction playerConstruction;
    [SerializeField] private BuildIndicator buildIndicator;

    // As we often reference constructions and furniture, its easier to reserve the variables now.
    private Construction construction;
    private Furniture furniture;

    //Run this when the game is booted up.
    private void Awake() {

        // Set the grid parameters.
        int gridWidth = 150;
        int gridHeight = 150;
        float cellSize = 5.0f;
        offsetPosition = new Vector3((transform.position.x - (gridWidth / 2) * cellSize), (transform.position.z - (gridHeight / 2) * cellSize), 0f);

        // Instantiate a grid of <Construction>'s with the grid they belong to, their x and y coords.
        constructionController = new CustomGrid<Construction>(offsetPosition, gridWidth, gridHeight, cellSize, (CustomGrid<Construction> g, int x, int y) => new Construction(g,x,y));
        Debug.Log("Construction Controller Grid Initialized! (" + gridWidth + " x " + gridHeight + ") - " + cellSize + ".0f cell size.");

        // Link the various controllers to the build controller so that we can send it grid updates.
        tileController.LinkTileController(constructionController);
        collisionController.LinkCollisionController(constructionController);
        buildIndicator.LinkBuildIndicator(constructionController);
        playerConstruction.LinkPlayerConstruction(constructionController);

    }

    public void LoadData(GameData data) {
        Debug.Log("LoadGame Intialized...");
        // Remove all tiles and furniture.
        Debug.Log("Resetting Current ConstructionController");
        for (int x = 0; x < constructionController.GetWidth(); x++) {
            for (int y = 0; y < constructionController.GetHeight(); y++)
            {
                Construction gridConstruction = constructionController.GetGridObject(x, y);
                // for each x,y: if there's a construction here with either furniture or tiles, get rid of 'em
                if (gridConstruction != null) {
                    if (gridConstruction.GetConstructionTile() != null) {
                        // Remove the construction tile:
                        gridConstruction.RemoveConstructionTile();
                    } 
                    if (gridConstruction.GetFurniture() != null) {
                        //remove the furniture
                        Furniture furnitureToDelete = gridConstruction.GetFurniture();
                        furnitureToDelete.DestroySelf();
                        gridConstruction.RemoveFurniture();
                    }
                }
            }
        }

        Debug.Log("Loading Constructions!");
        // Add back all tiles
        //  NOTE: We want to use ForEach to make sure we don't have to store the tile/furniture in the exact order.
        //      If we're saving and loading once, the player shouldnt notice the time difference in iterating.
        foreach (GameData.GridContent savedConstructionTile in data.gridContentsList)
        {
            Construction construction = constructionController.GetGridObject(savedConstructionTile.x, savedConstructionTile.y);
            if (savedConstructionTile.tId != 0) {
                foreach (ConstructionTile constructionTile in playerConstruction.constructionTiles)
                {
                    if (constructionTile.tileId == savedConstructionTile.tId) {
                        construction.SetConstructionTile(constructionTile);
                        break;
                    }
                }
            }
        }
        foreach (GameData.FurnitureContent savedFurniture in data.furnitureContentsList)
        {
            Construction construction = constructionController.GetGridObject(savedFurniture.x, savedFurniture.y);
            // Go through our list of furnitureobjects until we find the right one.
            foreach (FurnitureObject furnitureObject in playerConstruction.furnitureObjects)
            {
                if (furnitureObject.furnitureId == savedFurniture.fID) {
                    (int offsetX, int offsetY) rotationOffset = furnitureObject.GetRotationOffset(savedFurniture.fDir);
                    Vector3 furnitureObjectWorldPosition = constructionController.GetWorldPosition(savedFurniture.x, savedFurniture.y) +
                        new Vector3(rotationOffset.offsetX, rotationOffset.offsetY, (playerConstruction.furnitureZOffset / constructionController.GetCellSize())) * constructionController.GetCellSize();
                                                                            // This is hacky ^^
                    Furniture furniture = Furniture.Create(furnitureObjectWorldPosition, (savedFurniture.x, savedFurniture.y), savedFurniture.fDir, furnitureObject);
                    // Get the x,y's we want to put the furniture in and set the furnitureobject accordingly.
                    List<(int x, int y)> furnitureGridPositionList = furnitureObject.GetGridPositionList((savedFurniture.x, savedFurniture.y), savedFurniture.fDir);
                    foreach ((int x, int y) in furnitureGridPositionList) {
                        // Get the construction object.
                        Construction gridPositionConstruction = constructionController.GetGridObject(x, y);
                        // Set furniture object in construction object.
                        gridPositionConstruction.SetFurniture(furniture);
                    }
                }
            }
        }
    }

    public void SaveData(GameData data) {
        Debug.Log("Saving Constructions!");
        //  Reset the GameData.gridContentsList and furnitureContentsList- then Repopuate with data;
        data.gridContentsList.Clear();
        data.furnitureContentsList.Clear();
        // Iterate through the grid.
        for (int x = 0; x < constructionController.GetWidth(); x++)
        {
            for (int y = 0; y < constructionController.GetHeight(); y++)
            {
                // For any grid spaces that have a construction on them
                Construction saveConstruction = constructionController.GetGridObject(x, y);
                if (saveConstruction.GetConstructionTile() != null) {
                    // if there's a tile on the space - add a new GridContent entry to gameData.
                    data.gridContentsList.Add(new GameData.GridContent((x, y), saveConstruction.GetConstructionTile().tileId));
                }
                // For any grid spaces that have furniture on them.
                if (saveConstruction.GetFurniture() != null) {
                    // If the current tile's origin = the furniture origin, add it to gameData.
                    Furniture saveFurniture = saveConstruction.GetFurniture();
                    if (saveFurniture.GetOrigin() == (x, y)) {
                        data.furnitureContentsList.Add(new GameData.FurnitureContent(
                            (x, y),
                            saveFurniture.GetFurnitureObject().furnitureId,
                            saveFurniture.GetFurnitureDir()
                            )
                        );
                    }
                }
            }
        }
    }

    // Define the Construction stored in each Grid.
    public class Construction {

        private CustomGrid<Construction> constructionControllerGrid; // A reference to its own grid
        private int x;                                      // its X coord
        private int y;                                      // its Y coord
        private ConstructionTile constructionTile;          // SO with our tile data.
        private Furniture furniture;                        // furniture item currently occupying the space.

        // Instantiator - bare minimum requirements for each cell.
        public Construction(CustomGrid<Construction> constructionControllerGrid, int x, int y) {
            this.constructionControllerGrid = constructionControllerGrid;
            this.x = x;
            this.y = y;
        }

        public (int x, int y) GetConstructionXY() {
            return (this.x, this.y);
        }

        public void SetConstructionTile(ConstructionTile constructionTile) {
            this.constructionTile = constructionTile;
            constructionControllerGrid.TriggerGridObjectChanged(x, y);
        }

        public ConstructionTile GetConstructionTile() {
            return this.constructionTile;
        }

        public void RemoveConstructionTile() {
            constructionTile = null;
            constructionControllerGrid.TriggerGridObjectChanged(x, y);
        }

        public override string ToString() {
            return constructionTile?.tileName + " - " + furniture?.GetFurnitureName();
        }

        public void SetFurniture(Furniture furniture) {
            this.furniture = furniture;
            constructionControllerGrid.TriggerGridObjectChanged(x, y);
        }

        public void RemoveFurniture() {
            furniture = null;
            constructionControllerGrid.TriggerGridObjectChanged(x, y);
        }

        public Furniture GetFurniture() {
            return this.furniture;
        }
    }
}
