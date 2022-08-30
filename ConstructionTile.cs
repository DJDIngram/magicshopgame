using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConstructionTile", menuName = "ScriptableObjects/Construction/ConstructionTile")]
public class ConstructionTile : ScriptableObject
{
    public enum TileType {
        None,
        Floor,
        Wall,
        Door
    }

    public TileType tileType;
    public Vector2Int[] MatrixIndecies;

    public string tileName;
    public float health;
    public bool isCollidable;
    public float walkSpeedModifier;


    public ( Vector2 UV00, Vector2 UV11 ) ChangeUV( int uvIndex ) {
        float matrixTileWidth = 32.0f;
        float matrixTileHeight = 32.0f;
        
        if ( uvIndex > MatrixIndecies.Length && uvIndex > 1) {
            Debug.LogWarning("ConstructionTile.ChangeUV ( uvIndex ) <-- UV INDEX SET IS OUT OF BOUNDS (" + uvIndex + ") RETURNING 1st UV. ");
            return (new Vector2 ((1 / matrixTileWidth) * MatrixIndecies[0].x, (1 / matrixTileHeight) * MatrixIndecies[0].y), new Vector2 ((1 / matrixTileWidth) * (MatrixIndecies[0].x + 1), (1 / matrixTileHeight) * (MatrixIndecies[0].y + 1)));
        } else {
            return (new Vector2 ((1 / matrixTileWidth) * MatrixIndecies[uvIndex].x, (1 / matrixTileHeight) * MatrixIndecies[uvIndex].y), new Vector2 ((1 / matrixTileWidth) * (MatrixIndecies[uvIndex].x + 1), (1 / matrixTileHeight) * (MatrixIndecies[uvIndex].y + 1)));
        }
    }
}
