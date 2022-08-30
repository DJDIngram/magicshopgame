using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furniture : MonoBehaviour
{
    // Like *this* desk is one instance of a *generic desk*, 
    //      so too is the *furniture* class to *a piece of furniture*.

    // Need references to the FurnitureObject the furniture belongs to
    private FurnitureObject furnitureObject;
    private FurnitureObject.Dir dir;
    private FurnitureObject.FurnitureType furnitureType;
    private SpriteRenderer spriteRenderer;
    private string furnitureName;
    private (int x, int y) origin;

    // Put the function to Instantiate a furnatureObject here, rather than in the parent class
    public static Furniture Create(Vector3 worldPosition, (int x, int y) origin, FurnitureObject.Dir dir, FurnitureObject furnitureObject) {
        GameObject furniturePrefab = Instantiate(furnitureObject.furniturePrefab,
            worldPosition,
            Quaternion.Euler(0, 0, furnitureObject.GetRotationAngle(dir)));
        
        Furniture furniture  = furniturePrefab.GetComponent<Furniture>();

        furniture.furnitureObject = furnitureObject;
        furniture.origin = origin;
        furniture.dir = dir;
        furniture.furnitureName = furnitureObject.furnitureName;
        return furniture;
    }

    // Use the same function of its parent buildable object to get all the spaces this building is currently occupying.
    public List<(int x, int y)> GetGridPositionList() {
        return furnitureObject.GetGridPositionList(origin, dir);
    }

    public string GetFurnitureName() {
        return furnitureName;
    }

    public void ChangeSpriteColor(Color color) {
        spriteRenderer = this.gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;
    }

    public void Reposition(Vector3 position, FurnitureObject.Dir dir) {
        this.transform.position = position;
        this.transform.rotation = Quaternion.Euler(0, 0, furnitureObject.GetRotationAngle(dir));
    }
    // Destroy itself.
    public void DestroySelf() {
        Destroy(gameObject);
    }

}
