using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

 // Creating the X,Z axis grid system underpinning the games build feature. Thanks Code Monkey <3

public class CustomGrid<TGridObject>
{
    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs {
        public int x;
        public int y;
    }

    // Initialize the origin of the grid, its width and height, and the size of each Cell.
    private Vector3 originPosition;
    private int width;
    private int height;
    private float cellSize;
    // Initialize the grid array, a 2D multidimensional array of TGridObjects.
    private TGridObject[,] gridArray;

    // Instantiator Method (called when a new CustomGrid is made).
    /*
        Because the grid is a 2D array of Objects, a func is called to give each grid
        object its "blank" value. This includes a reference to itself (a grid of
        objects), its x and z positions as well as the Object stored in it.
    */
    public CustomGrid(Vector3 originPosition, int width, int height, float cellSize, Func<CustomGrid<TGridObject>, int, int, TGridObject> createGridObject) {
        this.originPosition = originPosition;
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;

        // Initialize the array.
        gridArray = new TGridObject[width, height];

        // Cycle through the array.
        // GetLength returns the length of a certain dimension of the array.
        //      Iterate through the gridArray(x) component, essentially getting each row.
        for (int x = 0; x < gridArray.GetLength(0); x++) {
            for (int y = 0; y < gridArray.GetLength(1); y++) {
                // For each x,y value
                //      Instantiate a grid object.
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }

        // DEBUG MODE --> Draw Gizmo lines and text showing grid boundaries and ToString() text
        bool showDebug = true;
        if (showDebug) {
            TextMesh[,] debugTextArray = new TextMesh[width, height];

            for (int x = 0; x < gridArray.GetLength(0); x++) {
                for (int y = 0; y < gridArray.GetLength(1); y++) {
                    debugTextArray[x, y] = UtilsClass.CreateWorldText(gridArray[x, y]?.ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * .5f, 30, Color.white, TextAnchor.MiddleCenter);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                }
            }
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);

            // The debug text array will only update when the OnGridObjectChanged event fires.
            OnGridObjectChanged += (object sender, OnGridObjectChangedEventArgs eventArgs) => {
                debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();
            };
        }
    }

    // Just incase these are needed
    public float GetCellSize() { return cellSize; }
    public int GetHeight() { return height; }
    public int GetWidth() { return width; }

    // Get the world position of a cell in the grid.
    public Vector3 GetWorldPosition (int x, int y) {
        return new Vector3(x, y) * cellSize + originPosition;
    }

    // Get the X,Y coordinate of the grid, using the worldposition as an input
    // Returns a tuple of the closest x,y coord.
    public (int x, int y) GetXY (Vector3 worldPosition) {
        return(Mathf.FloorToInt((worldPosition - originPosition).x / cellSize), Mathf.FloorToInt((worldPosition - originPosition).y / cellSize));
    }

    // Returns the closest grid origin given a vector3
    public Vector3 ClosestGridOrigin (Vector3 worldPosition) {
        (int x, int y) = GetXY(worldPosition);
        return GetWorldPosition(x, y);
    }

    // Set value of array to TGridObject
    // First check that the integer x,z values given to it are within the boundaries of the grid.
    public void SetGridObject (int x, int y, TGridObject value) {
        if(x >= 0 && y >= 0 && x < width && y < height) {
            gridArray[x,y] = value;
            if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs{ x = x, y = y});
        }
    }

    // Set value of grid when given the worldPositon.
    public void SetGridObject (Vector3 worldPosition, TGridObject value) {
        (int x, int y) gridPosition = GetXY(worldPosition);
        SetGridObject(gridPosition.Item1, gridPosition.Item2, value);
    }

    // This can be called by the grid when using custom objects in each cell, to let the grid know that one of the objects data has changed.
    public void TriggerGridObjectChanged(int x, int y) {
        if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs{ x = x, y = y});
    }

    // Get the value at the specified grid.
    public TGridObject GetGridObject(int x, int y) {
        if (x >= 0 && y >= 0 && x < width && y < height){
            return gridArray[x,y];
        } else {
            // if out of bounds, return default of generic type
            return default(TGridObject);
        }
    }

    // Same function, using vector3 input
    public TGridObject GetGridObject(Vector3 worldPosition) {
        (int x, int y) gridPosition = GetXY(worldPosition);
        return GetGridObject(gridPosition.Item1, gridPosition.Item2);
    }

}
