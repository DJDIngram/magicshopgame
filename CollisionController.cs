using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/* 
    The CollisionController takes information from the ConstructionController, and updates its mesh with various
    walls, floors and other tiles.
    
*/

public class CollisionController : MonoBehaviour
{
    // Save a reference to our grid - the ConstructionController, with its array of <Construction>'s.
    private CustomGrid<ConstructionController.Construction> constructionControllerReference;
    // Create a way to link the constructionController to the CollisionController.
    // This is called in the ConstructionController script.
    public void LinkCollisionController(CustomGrid<ConstructionController.Construction> grid) {
        this.constructionControllerReference = grid;
        // Subscribe to event.
        constructionControllerReference.OnGridObjectChanged += ConstructionController_OnGridObjectChanged;
    }

    // Initialize a 2D array of BoxCollider2D's and a compositeCollider to glue them all together!
    private BoxCollider2D[,] boxColliderArray;
    private CompositeCollider2D compCollider;

    private void Awake() {
        // Create ALL Box Colliders
        // Size vector is the grid cellSize
        Vector3 sizeVector = new Vector3(1, 1, 1) * constructionControllerReference.GetCellSize();
        boxColliderArray = new BoxCollider2D[constructionControllerReference.GetWidth(), constructionControllerReference.GetHeight()];

        for (int x = 0; x < constructionControllerReference.GetWidth(); x++)
        {
            for (int y = 0; y < constructionControllerReference.GetHeight(); y++)
            {
                boxColliderArray[x, y] = gameObject.AddComponent<BoxCollider2D>();
                boxColliderArray[x, y].offset = constructionControllerReference.GetWorldPosition(x, y) + (sizeVector * 0.5f);
                boxColliderArray[x, y].size = sizeVector;
                boxColliderArray[x, y].usedByComposite = true;
                boxColliderArray[x, y].enabled = false;
            }
        }

        compCollider = gameObject.GetComponent<CompositeCollider2D>();

        Debug.Log("CollisionController is Awake!");
    }

    // When we recieve the event from the constructionController telling us to update the colliders, do that!
    private void ConstructionController_OnGridObjectChanged( object sender, CustomGrid<ConstructionController.Construction>.OnGridObjectChangedEventArgs e) {
        Debug.Log("CollisionController has recieved the event from Construction Controller - Updating Collisions.");
        if (constructionControllerReference.GetGridObject(e.x, e.y) != null && constructionControllerReference.GetGridObject(e.x, e.y)?.GetConstructionTile() != null && constructionControllerReference.GetGridObject(e.x, e.y).GetConstructionTile().isCollidable) {
            boxColliderArray[e.x, e.y].enabled = true;
        } else { boxColliderArray[e.x, e.y].enabled = false; }
    }
}
