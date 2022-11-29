using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Player construction is the class attached to the player that allows them to interact with the grid of constructionobjects/furniture.

public class PlayerConstruction : MonoBehaviour
{
    // Save a reference to our grid - the ConstructionController, with its array of <Construction>'s.
    private CustomGrid<ConstructionController.Construction> constructionController;
    // Create a way to link the constructionController to the PlayerController.
    // This is called in the ConstructionController script.
    public void LinkPlayerConstruction(CustomGrid<ConstructionController.Construction> grid) {
        this.constructionController = grid;
    }

    // Cache the camera, as we make multiple calls to it!
    Camera mainCam;
    //Keep track of the mouse Position before and after clicking
    private Vector2 mouseStartPos;
    private Vector2 prevMousePos;
    private Vector2 mousePos;

    [SerializeField] private BuildIndicator buildIndicator;
    public float furnitureZOffset = 7;

    private enum ConstructionStatus {
        None,
        Cancelled,
        StartedConstruction,
        FinishedConstruction,
        StartedDemolish,
        FinishedDemolish,
    }

    private ConstructionStatus constructionStatus;
    // Set a buildMode bool -> used to toggle construct/deconstruct/tile functionality
    private bool buildMode;
    private Transform furnitureGhost;
    // Create an empty array of bools, showing whether we can build there or not.
    private List<((int x, int y), int buildInd)> buildArray;
    // And the same for furniture...
    private bool placeMode;
    private bool canPlace;

    // As we often reference constructions and furniture, its easier to reserve the variables now.
    private ConstructionController.Construction construction;
    private Furniture furniture;

    //Store a list of all the tile scriptable objects, and keep an index as to the one we've selected.
    public ConstructionTile[] constructionTiles;
    private ConstructionTile selectedConstructionTile;
    private int constructionTilesIndex;

    //Store a list of all the furniture scriptable objects, and keep an index as to the one we've selected.
    public FurnitureObject[] furnitureObjects;
    private FurnitureObject selectedFurnitureObject;
    private int furnitureObjectsIndex;
    private FurnitureObject.Dir dir = FurnitureObject.Dir.North; // a reference to its direction, default is North.
    private List<(int x, int y)> furnitureGridPositionList; // a reference to the grid positions that larger furniure may take up.

    public bool isEnabled; // used by the UI to disable construction/placement.

    void Awake() {
        Debug.Log("Hello from PlayerConstruction");

        // Subscribe to GameStateManager
        GameStateManager.OnGameStateChanged += OnGameStateChanged;

        // Link the main camera.
        mainCam = Camera.main;

        // Set the default tile and furniture by index.
        constructionTilesIndex = 0;
        selectedConstructionTile = constructionTiles[constructionTilesIndex];
        furnitureObjectsIndex = 0;
        selectedFurnitureObject = furnitureObjects[furnitureObjectsIndex];

        // Set buildMode, placeMode and buildArray
        buildMode = false;
        placeMode = false;
        buildArray = new List<((int x, int y), int canBuild)>();

        // Initialize the mouse positions
        mouseStartPos = Vector2.zero;
        prevMousePos = Vector2.zero;
        mousePos = Vector2.zero;
    }

    // Run the build check every 3 frames.
    private int interval = 3;
    // Using LateUpdate on the buildcheck incase anything that happens during update might change its state.
    private void LateUpdate() {
        if (Time.frameCount % interval == 0) {
            BuildCheck();
            PlaceCheck();
        }
    }
        // Logic for adding to buildArray
    private void AddToBuildArray(int x, int y) {
        // If the construction exists
        //  And theres no floor/wall tile there
        //  and we're not trying to put a wall through furniture (this logic looks horrendous...
        //  then Green
        // Else If the construction exists and tile is of the same type as what we're placing
        //  then Gray
        //  Else Red.
        construction = constructionController.GetGridObject(x, y);
        if (construction != null) {
            if (construction.GetConstructionTile() == null &&
                !(constructionTiles[constructionTilesIndex].tileType == ConstructionTile.TileType.Wall && construction.GetFurniture() != null)) {
                buildArray.Add(((x, y), 1));
            } else if ( (construction.GetConstructionTile() != null) &&
            (constructionTiles[constructionTilesIndex].tileType == construction.GetConstructionTile()?.tileType)) {
                buildArray.Add(((x,y), 0));
            } else {
                buildArray.Add(((x,y), 2));
            }
        }
    }

    private void BuildCheck () {
        // Can we build here?
        if (buildMode) {
            prevMousePos = mousePos;
            mousePos =  mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            if (constructionStatus == ConstructionStatus.StartedConstruction || constructionStatus == ConstructionStatus.StartedDemolish) {
                // We're in the process of clicking and dragging here.
                // If the current mouse grid position changes, we need to update the buildArray.
                if (constructionController.GetXY(prevMousePos) != constructionController.GetXY(mousePos)) {
                    // Refresh Array.
                    buildArray.Clear();
                    ( int fromX, int fromY ) = constructionController.GetXY(mouseStartPos);
                    ( int toX, int toY ) = constructionController.GetXY(mousePos);

                    // In order for us to be able to click and drag from anywhere, 
                    //  make sure we reset the start and end X/Y to the lowest value, and vice versa.
                    int startX = fromX < toX ? fromX : toX;
                    int startY = fromY < toY ? fromY : toY;
                    int finishX = fromX > toX ? fromX : toX;
                    int finishY = fromY > toY ? fromY : toY;
                    // Debug.Log(startX + " " + startY);
                    // Debug.Log(finishX + " " + finishY);
                    // Iterate through our selection, and update the buildArray
                    for (int x = startX; x <= finishX; x++)
                    {
                        for (int y = startY; y <= finishY; y++)
                        {
                            
                            // if we're constructing, work out the build indicator. If we arent, demolish everything (red indicator)
                            if (constructionStatus == ConstructionStatus.StartedConstruction) {
                                AddToBuildArray(x, y);
                            } else {
                                if (construction != null) {
                                    buildArray.Add(((x, y), 2));
                                }
                            }
                        }
                    }
                    // Debug.Log("Array Refreshed");
                    buildIndicator.UpdateBuildIndicatorMatrix(buildArray);
                }
            } else if (constructionStatus == ConstructionStatus.None || constructionStatus == ConstructionStatus.Cancelled) {
                // If ConstructionStatus = None, but we can build, then we're using the grid reference as an outline
                buildArray.Clear();
                // Get the construction on the grid
                (int x, int y) = constructionController.GetXY(mousePos);
                AddToBuildArray(x, y);
                buildIndicator.UpdateBuildIndicatorMatrix(buildArray);
            }
        }
    }

    private void PlaceCheck (bool force = false) {
        // Can we place here?
        if (placeMode) {
            prevMousePos = mousePos;
            mousePos =  mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            // If the mouse position has moved, we can indicate if the player can place the furniture here.
            // And if they cannot, which grid spaces are causing this?
            if (force || (constructionController.GetXY(prevMousePos) != constructionController.GetXY(mousePos))) {
                // Update Build Indicator Matrix
                buildArray.Clear();
                (int x, int y) origin = constructionController.GetXY(mousePos);
                furnitureGridPositionList = selectedFurnitureObject.GetGridPositionList((origin.x, origin.y), dir);
                
                foreach ((int x, int y) gridPosition in furnitureGridPositionList)
                {
                    // Get the construction object.
                    // Debug.Log("Checking " + gridPosition.Item1 + ", " + gridPosition.Item2);
                    construction = constructionController.GetGridObject(gridPosition.Item1, gridPosition.Item2);
                    if (construction != null) {
                        if (construction.GetFurniture() != null || construction.GetConstructionTile()?.tileType == ConstructionTile.TileType.Wall) {
                            // Either out of bounds or already furniture here, or a wall here.
                            buildArray.Add(((gridPosition.x, gridPosition.y), 2)); // Cant Place here
                        } // Otherwise, canPlace is already false and we dont need to do anything...
                        else if (construction.GetFurniture() is null) {
                            // Within the grid and no tile here yet!
                            buildArray.Add(((gridPosition.x, gridPosition.y), 1)); // Can Place Here.
                        }
                    }
                }

                // Debug.Log("Array Refreshed");
                buildIndicator.UpdateBuildIndicatorMatrix(buildArray);
                // if all of the furniture grid spaces are in the grid, show the furniture ghost, else set inactive.
                furnitureGhost.gameObject.SetActive(buildArray.Count == selectedFurnitureObject.gridWidth * selectedFurnitureObject.gridHeight);
                if (furnitureGhost.gameObject.activeSelf) // if it IS active, move it to the right space.
                {
                    (int offsetX, int offsetY) rotationOffset = selectedFurnitureObject.GetRotationOffset(dir);
                    Vector3 furnitureObjectWorldPosition = constructionController.GetWorldPosition(origin.x, origin.y) +
                        new Vector3(rotationOffset.offsetX, rotationOffset.offsetY, (furnitureZOffset / constructionController.GetCellSize())) * constructionController.GetCellSize();
                                                                            // This is hacky ^^
                    furnitureGhost.transform.position = furnitureObjectWorldPosition;
                    furnitureGhost.transform.rotation = Quaternion.Euler(0, 0, selectedFurnitureObject.GetRotationAngle(dir));
                }
            }
        }
    }

    public void Debuggit(InputAction.CallbackContext context) {
        if (!(buildMode || placeMode) && context.started && GameStateManager.instance.gameState == GameState.Playing) {
            mousePos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            construction = constructionController.GetGridObject(mousePos);
            Debug.Log(construction?.ToString());
        }
    }

    // Set Construction
    public void Construct(InputAction.CallbackContext context) {
        if (constructionStatus == ConstructionStatus.StartedDemolish) {
            // STOP CONSTRUCTION
            Debug.Log("CANCEL DEMOLISH");
            CancelAction();
        } else if (buildMode && !isEnabled && context.started) {
            // We've JUST initialized the click. Get the initial reference for our mousePos,
            //  and clear the can/cannotBuild arrays. Set the constructionStatus so that
            //  buildCheck can start populating them.
            mouseStartPos =  mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            // Debug.Log("StartedConstruction" + mouseStartPos);
            constructionStatus = ConstructionStatus.StartedConstruction;
            // Clear the buildArray
            buildArray.Clear();
            // Add first tile to buildArray
            (int x, int y) = constructionController.GetXY(mouseStartPos);
            AddToBuildArray(x, y);
        } else if (buildMode && context.performed) { 
            // Debug.Log("Performed");
        } else if (buildMode && context.canceled) {
            // If the construction action hasnt been cancelled.
            if (constructionStatus == ConstructionStatus.StartedConstruction) {
                // Debug.Log("Finshed" + mousePos);
                // We've finished the click. Now we need to run buildcheck one more time with
                // the last values of can/cannot build before confirming and setting our tiles.
                constructionStatus = ConstructionStatus.FinishedConstruction;
                BuildCheck();
                // Iterate over our toBuild list and update tiles accordingly!
                Debug.Log("Constructing...");
                foreach (((int x, int y), int canBuild) tileXY in buildArray)
                {
                    if (tileXY.canBuild == 1) {
                        construction = constructionController.GetGridObject(tileXY.Item1.x, tileXY.Item1.y);
                        // construction.SetConstructionTile(constructionTiles[constructionTilesIndex]);
                        construction.SetConstructionTile(selectedConstructionTile);
                    }
                }
            }
            constructionStatus = ConstructionStatus.None;
            mouseStartPos = Vector2.zero;
            mousePos = Vector2.zero;
        }
    }

    //Place Furniture
    public void Place(InputAction.CallbackContext context) {
        if (placeMode && !isEnabled && context.started) {
            // Cycle through buildArray, and if no values are false then we can place!
            bool canPlace = false;
            // For I x J furniture, if we place near the edge of the grid, we want to make sure the buildarray covers all spaces that will be occupied by the furniture.
            //      Otherwise we may have a 1x2 desk where 1 grid
            if (buildArray.Count == selectedFurnitureObject.gridWidth * selectedFurnitureObject.gridHeight)
            {
                foreach (((int x, int y), int canBuild) furnitureSpace in buildArray)
                {
                    if (furnitureSpace.canBuild == 2) {
                        canPlace = false;
                        break;
                    } else {
                        canPlace = true;
                    }
                }
            }
            if (canPlace) {
                // If nothing is here, we can place!
                (int x, int y) = constructionController.GetXY(mousePos);
                (int offsetX, int offsetY) rotationOffset = selectedFurnitureObject.GetRotationOffset(dir);
                Vector3 furnitureObjectWorldPosition = constructionController.GetWorldPosition(x, y) +
                    new Vector3(rotationOffset.offsetX, rotationOffset.offsetY, (furnitureZOffset / constructionController.GetCellSize())) * constructionController.GetCellSize();
                                                                        // This is hacky ^^
                Furniture furniture = Furniture.Create(furnitureObjectWorldPosition, (x, y), dir, selectedFurnitureObject);

                Debug.Log("Placing...");
                // Get the x,y's we want to put the furniture in and set the furnitureobject accordingly.
                foreach ((int x, int y) gridPosition in furnitureGridPositionList) {
                    // Get the construction object.
                    construction = constructionController.GetGridObject(gridPosition.Item1, gridPosition.Item2);
                    // Set furniture object in construction object.
                    construction.SetFurniture(furniture);
                    // Debug.Log("Adding " + gridPosition.Item1 + ", " + gridPosition.Item2 + " to furniture grid positionlist");
                }
            }
        }
    }

    // Demolish Construction
    public void Demolish(InputAction.CallbackContext context) {
        if (constructionStatus == ConstructionStatus.StartedConstruction) {
            // We wnat to use right-click to stop constructing too.
            // So if we're still constructing, stop this!
            Debug.Log("CANCEL CONSTRUCTION");
            CancelAction(); // Will set ConstructionStatus to Cancelled.
        } else if (buildMode && !isEnabled && context.started) {
            mouseStartPos =  mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            constructionStatus = ConstructionStatus.StartedDemolish;
            // Clear the buildArray
            buildArray.Clear();
            // Add first tile to buildArray
            (int x, int y) = constructionController.GetXY(mouseStartPos);
            AddToBuildArray(x, y);
        } else if (buildMode && context.performed) { 
            // Debug.Log("Performed");
        } else if (buildMode && context.canceled) {
            // If the construction action hasnt been cancelled.
            if (constructionStatus == ConstructionStatus.StartedDemolish) {
                // We've finished the click. Now we need to run buildcheck one more time with
                // the last values of can/cannot build before confirming and setting our tiles.
                constructionStatus = ConstructionStatus.FinishedDemolish;
                BuildCheck();
                // Iterate over our toBuild list and update tiles accordingly!
                Debug.Log("Demolishing...");
                foreach (((int x, int y), int canBuild) tileXY in buildArray)
                {
                    construction = constructionController.GetGridObject(tileXY.Item1.x, tileXY.Item1.y);
                    if (construction != null && construction?.GetConstructionTile() != null) {
                        construction.RemoveConstructionTile();
                    }
                }
            }
            constructionStatus = ConstructionStatus.None;
            mouseStartPos = Vector2.zero;
            mousePos = Vector2.zero;
        }
    }

    private void CancelAction() {
        if (constructionStatus == ConstructionStatus.StartedConstruction || constructionStatus == ConstructionStatus.StartedDemolish) {
            buildArray.Clear();
            constructionStatus = ConstructionStatus.Cancelled;
        }
    }

    // Remove furniture
    public void Remove(InputAction.CallbackContext context) {
        if (placeMode && !isEnabled && context.started) {
            construction = constructionController.GetGridObject(mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
            // If something is here, we can remove it!
            // Find the furniture item
            furniture = construction?.GetFurniture();
            if (furniture != null) {
            //  then find all other spaces occupied by that same furniure
                foreach ((int x, int y) furnitureTile in furniture.GetGridPositionList())
                {
                    // then remove that furniture object on each space
                    constructionController.GetGridObject(furnitureTile.Item1, furnitureTile.Item2).RemoveFurniture();
                }
                // Then destroy furnuiture.
                furniture.DestroySelf();
                // Then do a place-check
                PlaceCheck(force:true);
            }
        }
    }

    // Change Construction --> loop back round if trying to swap tile/furniture out of bounds
    //      I.E N[0] goes back to N[max] and N[max] goes forward to N[0]
    //  29.10.22 - REMOVED FUNCTIONALITY
    public void SwapConstruction(InputAction.CallbackContext context) {
        if (buildMode && context.performed) {
            constructionTilesIndex = System.Array.IndexOf(constructionTiles, selectedConstructionTile);
            float direction = context.ReadValue<float>();
            // Debug.Log("direction -> " + direction);
            if (direction < 0) {
                // Debug.Log("Swapping Tiles --> Previous...");
                constructionTilesIndex = constructionTilesIndex == 0 ? constructionTiles.Length - 1 : constructionTilesIndex -= 1;
                Debug.Log(constructionTilesIndex);
            } else if (direction > 0) {
                // Debug.Log("Swapping Tiles --> Next...");
                constructionTilesIndex = constructionTilesIndex == constructionTiles.Length - 1 ? 0 : constructionTilesIndex += 1;
                Debug.Log(constructionTilesIndex);
            }
            SetConstructionTile(constructionTiles[constructionTilesIndex]);
        } else if (placeMode && context.performed) {
            furnitureObjectsIndex = System.Array.IndexOf(furnitureObjects, selectedFurnitureObject);
            float direction = context.ReadValue<float>();
            // Debug.Log("direction -> " + direction);
            if (direction < 0) {
                // Debug.Log("Swapping Furniture --> Previous...");
                furnitureObjectsIndex = furnitureObjectsIndex == 0 ? furnitureObjects.Length - 1 : furnitureObjectsIndex -= 1;
                Debug.Log(furnitureObjectsIndex);
            } else if (direction > 0) {
                // Debug.Log("Swapping Tiles --> Next...");
                furnitureObjectsIndex = furnitureObjectsIndex == furnitureObjects.Length - 1 ? 0 : furnitureObjectsIndex += 1;
                Debug.Log(furnitureObjectsIndex);
            }
            SetFurnitureObject(furnitureObjects[furnitureObjectsIndex]);
        }
    }

    public void SetConstructionTile(ConstructionTile constructionTile) {
        // Choosing a construction from the UI will toggle build mode to ON
        if (!buildMode) { ToggleBuildMode(); };
        selectedConstructionTile = constructionTile;
    }

    public void SetFurnitureObject(FurnitureObject furnitureObject) {
        // Choosing furniture from the UI will toggle place mode to ON
        if (!placeMode) { TogglePlaceMode(); };
        selectedFurnitureObject = furnitureObject;
        // Need to make sure we reset the direction to the new furniture object default
        // when we change, to avoid rotating those that dont rotate...
        dir = selectedFurnitureObject.dir;
        // Reinstantiate the furnitureGhost.
        Destroy(furnitureGhost.gameObject);
        (int offsetX, int offsetY) rotationOffset = selectedFurnitureObject.GetRotationOffset(dir);
        Vector3 furnitureObjectWorldPosition = constructionController.ClosestGridOrigin(mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue())) +
            new Vector3(rotationOffset.offsetX, rotationOffset.offsetY, (furnitureZOffset / constructionController.GetCellSize())) * constructionController.GetCellSize();
                                                                    // This is hacky ^^
        furnitureGhost = Instantiate(selectedFurnitureObject.furniturePrefab.transform.GetChild(0), furnitureObjectWorldPosition, Quaternion.Euler(0, 0, selectedFurnitureObject.GetRotationAngle(dir)));
        furnitureGhost.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.4f);
        PlaceCheck(force:true);
    }

    public void SetBuildMode(bool buildModeState) { if (buildMode != buildModeState) { ToggleBuildMode(); }; }
    public void SetPlaceMode(bool placeModeState) { if (placeMode != placeModeState) { TogglePlaceMode(); }; }

    private void ToggleBuildMode() {
        buildMode = buildMode == true ? false : true;
        if (buildMode == false) {
            buildIndicator.Inactive();
        } else if (placeMode == true && buildMode == true) {
            // if entering buildmode when placemode is already on - stop placemode.
            placeMode = false;
            Debug.Log("PLACE MODE --> " + placeMode);
            Destroy(furnitureGhost.gameObject);
        }
        Debug.Log("BUILD MODE --> " + buildMode);
    }

    private void TogglePlaceMode() {
        placeMode = placeMode == true ? false : true;
        if (placeMode == false) {
            Destroy(furnitureGhost.gameObject);
            buildIndicator.Inactive();
        } else {
            if (buildMode) {
                // if entering placemode when buildmode is already on - stop buildmode.
                buildMode = false;
                Debug.Log("BUILD MODE --> " + buildMode);
            }
            (int offsetX, int offsetY) rotationOffset = selectedFurnitureObject.GetRotationOffset(dir);
            Vector3 furnitureObjectWorldPosition = constructionController.ClosestGridOrigin(mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue())) +
                new Vector3(rotationOffset.offsetX, rotationOffset.offsetY, (furnitureZOffset / constructionController.GetCellSize())) * constructionController.GetCellSize();
                                                                    // This is hacky ^^
            furnitureGhost = Instantiate(selectedFurnitureObject.furniturePrefab.transform.GetChild(0), furnitureObjectWorldPosition, Quaternion.Euler(0, 0, selectedFurnitureObject.GetRotationAngle(dir)));
            furnitureGhost.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.4f);
            PlaceCheck(force:true);
        }
        Debug.Log("PLACE MODE --> " + placeMode);
    }

    // Used for rotation of furniture objects.
    public void ToggleRotation(InputAction.CallbackContext context) {
        if (context.performed) {
            dir = FurnitureObject.GetNextDir(dir);
            Debug.Log("New Dir --> " + dir);
            PlaceCheck(force:true);
        }
    }

    private void OnGameStateChanged(GameState gameState) {
        if (gameState == GameState.Paused) {
            if (placeMode) {
                TogglePlaceMode();
            } else if (buildMode) {
                CancelAction();
                ToggleBuildMode();
            }
        }
    }

    private void OnDestroy() {
        // Subscribe to GameStateManager
        GameStateManager.OnGameStateChanged -= OnGameStateChanged;
    }
}
