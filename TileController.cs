using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/* 
    The TileController takes information from the ConstructionController, and updates its mesh with various
    walls, floors and other tiles.
    
*/

public class TileController : MonoBehaviour
{
    // Save a reference to our grid - the ConstructionController, with its array of <Construction>'s.
    private CustomGrid<ConstructionController.Construction> constructionControllerReference;
    // Create a way to link the constructionController to the TileController.
    // This is called in the ConstructionController script.
    public void LinkTileController(CustomGrid<ConstructionController.Construction> grid) {
        this.constructionControllerReference = grid;
        // Subscribe to event.
        constructionControllerReference.OnGridObjectChanged += ConstructionController_OnGridObjectChanged;
    }

    private bool updateMesh;
    private Mesh mesh;

    private void Awake () {
        //Create a new mesh
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    // When we recieve the event from the constructionController telling us to update the mesh, do that!
    private void ConstructionController_OnGridObjectChanged( object sender, CustomGrid<ConstructionController.Construction>.OnGridObjectChangedEventArgs e) {
        Debug.Log("TileController has recieved the event from Construction Controller - Updating Mesh.");
        updateMesh = true;
    }

    private void LateUpdate() {
        if (updateMesh) {
            updateMesh = false;
            UpdateTiles();
        }
    }
    
    private void UpdateTiles () {
        // Create all the verticies, uvs and triangles needed to populate the entire map.
        (Vector3[] vertices, Vector2[] uvs, int[] triangles) = CreateEmptyMeshArrays(constructionControllerReference.GetWidth() * constructionControllerReference.GetHeight());

        for (int x = 0; x < constructionControllerReference.GetWidth(); x++) {
            for (int y = 0; y < constructionControllerReference.GetHeight(); y++) {
                // Cycle through the quads.
                int quadIndex = x * constructionControllerReference.GetHeight() + y;
                Vector3 quadSize = new Vector3(1, 1) * constructionControllerReference.GetCellSize();
                
                // Get the construction and its sprite Enum on each grid.
                ConstructionController.Construction construction = constructionControllerReference.GetGridObject(x, y);
                ConstructionTile constructionTile = construction.GetConstructionTile();

                Vector2 UV00, UV11; 
                // Debug.Log(constructionTile.tileName);
                if (constructionTile is null) {
                    // This is really hacky! Essentially all verticies are still being created, but dont occupy any space.
                    UV00 = Vector2.zero;
                    UV11 = Vector2.zero;
                    quadSize = Vector3.zero;
                } else {
                    if (constructionTile.MatrixIndecies.Length > 1) {
                        (  UV00, UV11 ) = constructionTile.ChangeUV(GetUVVariant(construction));
                    } else {
                        (  UV00, UV11 ) = constructionTile.ChangeUV(0);
                    }
                }
                // Note I'm adding transform.position to allow for offsetting the mesh by the z-amount i specify in the inspector.
                AddToMeshArrays(vertices, uvs, triangles, quadIndex, (constructionControllerReference.GetWorldPosition(x, y) + transform.position) + quadSize * 0.5f, 0f, quadSize, UV00, UV11);
            
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
    }

    // For tiles with multiple texture variants (ie. walls), look at the surrounding tiles and decide which variant should be rendered.
    // output the matrixIndex
    public int GetUVVariant(ConstructionController.Construction construction) {
        //0 - North, 1 - East, 2 - South, 3 - West
        ConstructionTile[] adjacentConstructionTiles = new ConstructionTile[4];
        (int x, int y) = construction.GetConstructionXY();

        // Populate the array with the correct construction tiles on the NESW array.
        adjacentConstructionTiles[0] = constructionControllerReference.GetGridObject(x, y + 1)?.GetConstructionTile();
        adjacentConstructionTiles[1] = constructionControllerReference.GetGridObject(x + 1, y)?.GetConstructionTile();
        adjacentConstructionTiles[2] = constructionControllerReference.GetGridObject(x, y - 1)?.GetConstructionTile();
        adjacentConstructionTiles[3] = constructionControllerReference.GetGridObject(x - 1, y)?.GetConstructionTile();

        bool[] connectedNeighbour = new bool[4];
        for (int i = 0; i < adjacentConstructionTiles.Length; i++)
        {
            if (adjacentConstructionTiles[i] && (adjacentConstructionTiles[i].tileType == ConstructionTile.TileType.Wall || adjacentConstructionTiles[i].tileType == ConstructionTile.TileType.Door)) {
                connectedNeighbour[i] = true;
            } else {
                connectedNeighbour[i] = false;
            }
        }

        // This is nuts...
        switch (( connectedNeighbour[0], connectedNeighbour[1], connectedNeighbour[2], connectedNeighbour[3] )) {
            case (false, true, false, false):
                return 1;
            case (false, true, false, true):
                return 2;
            case (false, true, true, true):
                return 3;
            case (false, false, true, true):
                return 4;
            case (true, true, true, true):
                return 5;
            case (false, false, false, true):
                return 6;
            case (true, false, true, false):
                return 7;
            case (true, true, false, true):
                return 8;
            case (true, false, false, true):
                return 9;
            case (true, true, true, false):
                return 10;
            case (true, false, true, true):
                return 11;
            case (true, false, false, false):
                return 12;
            case (false, true, true, false):
                return 13;
            case (true, true, false, false):
                return 14;
            case (false, false, true, false):
                return 15;
            default:
                return 0;
        }
    }

    // *** MESH FUNCTIONS *** 

    // Calculate all the verticies, uvs and triangles needed to populate the grid with quads. Create them!
    public static (Vector3[] vertices, Vector2[] uvs, int[] triangles) CreateEmptyMeshArrays (int quadCount) {
        return (new Vector3[4 * quadCount], new Vector2[4 * quadCount], new int[6 * quadCount]);
    }
    
    // 
    public static void AddToMeshArrays(Vector3[] vertices, Vector2[] uvs, int[] triangles, int index, Vector3 pos, float rot, Vector3 baseSize, Vector2 uv00, Vector2 uv11) {
        // Calculate vertices depending on array index.
        int vIndex = index * 4;

        int vIndex0 = vIndex;       //EG vertex for quad 12 (12,0) would be 12 13, 14 and 15.
        int vIndex1 = vIndex + 1;
        int vIndex2 = vIndex + 2;
        int vIndex3 = vIndex + 3;
        
        baseSize *= .5f;

        //Locate vertices
        bool skewed = baseSize.x != baseSize.y;
        if (skewed) {
            vertices[vIndex0] = pos + GetQuaternionEuler(rot) * new Vector3(-baseSize.x, baseSize.y);
            vertices[vIndex1] = pos + GetQuaternionEuler(rot) * new Vector3(-baseSize.x, -baseSize.y);
            vertices[vIndex2] = pos + GetQuaternionEuler(rot) * new Vector3(baseSize.x, -baseSize.y);
            vertices[vIndex3] = pos + GetQuaternionEuler(rot) * baseSize;
        } else { 
            vertices[vIndex0] = pos + GetQuaternionEuler(rot - 270) * baseSize;
            vertices[vIndex1] = pos + GetQuaternionEuler(rot - 180) * baseSize;
            vertices[vIndex2] = pos + GetQuaternionEuler(rot - 90) * baseSize;
            vertices[vIndex3] = pos + GetQuaternionEuler(rot - 0) * baseSize;
        }

        //Relocate UV's
        uvs[vIndex0] = new Vector2(uv00.x, uv11.y);
        uvs[vIndex1] = new Vector2(uv00.x, uv00.y);
        uvs[vIndex2] = new Vector2(uv11.x, uv00.y);
        uvs[vIndex3] = new Vector2(uv11.x, uv11.y);

        //Create triangles
		int tIndex = index*6;
		
		triangles[tIndex+0] = vIndex0;
		triangles[tIndex+1] = vIndex3;
		triangles[tIndex+2] = vIndex1;
		
		triangles[tIndex+3] = vIndex1;
		triangles[tIndex+4] = vIndex3;
		triangles[tIndex+5] = vIndex2;
            
		// mesh.vertices = vertices;
		// mesh.triangles = triangles;
		// mesh.uv = uvs;

        // return mesh;
    }

    private static Quaternion[] cachedQuaternionEulerArr;
    private static void CacheQuaternionEuler() {
        if (cachedQuaternionEulerArr != null) return;
        cachedQuaternionEulerArr = new Quaternion[360];
        for (int i=0; i<360; i++) {
            cachedQuaternionEulerArr[i] = Quaternion.Euler(0,0,i);
        }
    }
    private static Quaternion GetQuaternionEuler(float rotFloat) {
        int rot = Mathf.RoundToInt(rotFloat);
        rot = rot % 360;
        if (rot < 0) rot += 360;
        //if (rot >= 360) rot -= 360;
        if (cachedQuaternionEulerArr == null) CacheQuaternionEuler();
        return cachedQuaternionEulerArr[rot];
    }
}
