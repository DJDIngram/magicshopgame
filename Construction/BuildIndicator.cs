using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
    The BuildIndicator takes information from the ConstructionController, and updates 
    its mesh with different build indicators.
*/

public class BuildIndicator : MonoBehaviour
{
    // Save a reference to our grid - the ConstructionController, with its array of <Construction>'s.
    private CustomGrid<ConstructionController.Construction> constructionControllerReference;
    // Create a way to link the constructionController to the BuildIndicator.
    // This is called in the ConstructionController script.
    public void LinkBuildIndicator(CustomGrid<ConstructionController.Construction> grid) {
        this.constructionControllerReference = grid;
    }

    private bool updateMesh;
    private Mesh mesh;
    private Vector3[] vertices; 
    private Vector2[] uvs;
    private int[] triangles; 

    private void Awake () {
        //Create a new mesh
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.bounds = new Bounds (Vector3.zero, Vector3.one * 500f);
    }
    
    public void UpdateBuildIndicatorMatrix (List<((int x, int y), int canBuild)> buildArray) {

        (vertices, uvs, triangles) = CreateEmptyMeshArrays(constructionControllerReference.GetWidth() * constructionControllerReference.GetHeight());

        foreach (((int x, int y), int canBuild) buildInd in buildArray)
        {
            if (buildInd.Item1.x >= constructionControllerReference.GetWidth() || buildInd.Item1.y >= constructionControllerReference.GetHeight())
            {
                continue;
            }          
            // Cycle through the buildArray, populating the mesh..
            int quadIndex = buildInd.Item1.x * constructionControllerReference.GetHeight() + buildInd.Item1.y;
            Vector3 quadSize = new Vector3(1, 1) * constructionControllerReference.GetCellSize();
            
            // Get the construction and its sprite Enum on each grid.
            Vector2 UV00, UV11; 
            // Debug.Log(constructionTile.tileName);
            switch (buildInd.Item2) {
                default:
                case 0:
                    UV00 = new Vector2(0, 0);
                    UV11 = new Vector2(0.333f, 1);
                    break;
                case 1:
                    UV00 = new Vector2(0.333f, 0);
                    UV11 = new Vector2(0.666f, 1);
                    break;
                case 2:
                    UV00 = new Vector2(0.666f, 0);
                    UV11 = new Vector2(1, 1);
                    break;
            }
            // Debug.Log(buildInd.Item1.x + " " + buildInd.Item1.y);
            AddToMeshArrays(vertices, uvs, triangles, quadIndex, constructionControllerReference.GetWorldPosition(buildInd.Item1.x, buildInd.Item1.y) + quadSize * 0.5f, 0f, quadSize, UV00, UV11);
        }
        mesh.bounds = new Bounds (Vector3.zero, Vector3.one * 5000f);
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
    }

    public void SetSingleIndicator() {
        vertices = new Vector3[4];
        uvs = new Vector2[4];
        triangles = new int[6];

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

        // Do we want to set default UV's??
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(0, 1);
        uvs[2] = new Vector2(0.333f, 1);
        uvs[3] = new Vector2(0.333f, 0);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
    }

    public void PositionBuildIndicator( Vector3 pos ) {
        float cellSize = constructionControllerReference.GetCellSize();
        vertices[0] = new Vector3(  0,    0)            + pos;
        vertices[1] = new Vector3(  0,  cellSize)       + pos;
        vertices[2] = new Vector3(cellSize,  cellSize)  + pos;
        vertices[3] = new Vector3(cellSize,  0)         + pos;
        
        mesh.vertices = vertices;
    }

    public void SetBuildState(int buildState) {
        // Change the mesh
        if (buildState == 0) {
            // Debug.Log("Grey");
            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(0, 1);
            uvs[2] = new Vector2(0.3333f, 1);
            uvs[3] = new Vector2(0.3333f, 0);
        } else if (buildState ==  1) {
            // Debug.Log("Can Build Here");
            uvs[0] = new Vector2(0.3333f, 0);
            uvs[1] = new Vector2(0.3333f, 1);
            uvs[2] = new Vector2(0.6666f, 1);
            uvs[3] = new Vector2(0.6666f, 0);
        } else {
            // Debug.Log("Cannot Build Here");
            uvs[0] = new Vector2(0.6666f, 0);
            uvs[1] = new Vector2(0.6666f, 1);
            uvs[2] = new Vector2(1, 1);
            uvs[3] = new Vector2(1, 0);
        }
        mesh.uv = uvs;
    }

    public void Inactive() {
        for (int i = 0; i < vertices.Length - 1; i++)
        {
            vertices[i] = Vector3.zero;
        }
        mesh.vertices = vertices;
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
