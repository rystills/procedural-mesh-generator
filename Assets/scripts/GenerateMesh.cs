using UnityEngine;
using System.Collections;

public class GenerateMesh : MonoBehaviour {
	public Material material;
    Vector3[] newVertices;
    int[] newTrianglePoints;
    Vector2[] newUVs;
    Texture2D debugTex;


    void Start() {
        debugTex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        debugTex.SetPixel(0, 0, Color.red);
        debugTex.SetPixel(1, 0, Color.green);
        debugTex.SetPixel(0, 1, Color.blue);
        debugTex.SetPixel(1, 1, Color.yellow);
        debugTex.wrapMode = TextureWrapMode.Repeat;
        debugTex.Apply();
        generatePipe("normal",10);
	}

    //initialize a new procedural mesh (n = number of quads)
    void initNewMesh(int n) {
        newVertices = new Vector3[(n + 1) * (n + 1)]; //one square mesh has 4 vertices, and each subsequent triange adds one new vert (therefore two per additional quad)
        newTrianglePoints = new int[6 * n * n]; //n^2 total quads. each quad is 2 tris. multiply by 3 as each tri is described by 3 points
        newUVs = new Vector2[newVertices.Length]; //number of UVs should match number of vertices for proper mapping
    }

    //construct vertices (n = number of quads)
    void buildVerts(int n, float xChange = .55f, float yChange = .55f, bool orientUp = false, int startOffset = 0) {
        for (int x = 0, listPos = startOffset; x < n + 1; x++) {
            for (int y = 0; y < n + 1; y++, listPos++) {
                newVertices[listPos] = new Vector3(x * xChange, y * yChange * (orientUp ? 1 : 0), y * yChange * (orientUp ? 0 : 1));
            }
        }
    }

    //construct vertices with a random wave on the y axis (n = number of quads)
    void buildVertsWavy(int n) {
        float averageLocalY = 0f;
        for (int x = 0, listPos = 0; x < n + 1; x++) {
            for (int y = 0; y < n + 1; y++, listPos++) {
                if (x == 0 && y == 0) { //first vertex so the average local y is the model's local y
                    averageLocalY = 0;
                }
                else if (x == 0) {
                    averageLocalY = newVertices[listPos - 1].y; //in the first row so only have to check vertex behind you
                }
                else {
                    if (y == 0) { //in the first column but not first row so must check below you and below - infront of you
                        averageLocalY = (newVertices[listPos - (n + 1)].y + newVertices[listPos - (n + 1) + 1].y) / 2;
                    }
                    else if (y == n + 1) { //in the last column but not first row so must check behind you, below behind you, and below you
                        averageLocalY = (newVertices[listPos - 1].y + newVertices[listPos - (n + 1) - 1].y + newVertices[listPos - (n + 1)].y) / 3;
                    }
                    else { //somewhere not on the outside. must check behind, below-behind, below, below-infront
                        averageLocalY = (newVertices[listPos - 1].y + newVertices[listPos - (n + 1) - 1].y + newVertices[listPos - (n + 1)].y + newVertices[listPos - (n + 1) + 1].y) / 4;
                    }
                }
                newVertices[listPos] = new Vector3(x * .55f, averageLocalY + (Random.Range(-0.4f, 0.4f)), y * .55f);
            }
        }
    }

    //break quads down into trianges (n = number of quads) 
    void buildTris(int n, int startOffset = 0) {
        for (int x = 0, listPos = startOffset; x < (n); x += 1) {
            for (int y = 0; y < (n); y += 1, listPos += 6) {
                newTrianglePoints[listPos] = ((n + 1) * x) + (y);
                newTrianglePoints[listPos + 4] = newTrianglePoints[listPos + 1] = ((n + 1) * x) + (y + 1);
                newTrianglePoints[listPos + 3] = newTrianglePoints[listPos + 2] = ((n + 1) * x) + (y + (n + 1));
                newTrianglePoints[listPos + 5] = ((n + 1) * x) + (y + (n + 2));
            }
        }
    }

    //construct UVs, repeating based on some scale factor (for a flat UVW unwrap with a straight-up orientation, we can just map to x,z coords)
    void buildUVs(float factor, bool orientUp = false, int n = 0, int startOffset = 0) {
        n = (n == 0 ? newUVs.Length : n);
        for (int i = startOffset; i < startOffset + n; newUVs[i] = new Vector2(newVertices[i].x / factor, (orientUp ? newVertices[i].y : newVertices[i].z) / factor), i++) ;
    }

    //construct the new mesh, and attach the appropriate components
    void finalizeMesh() {
        Mesh mesh = new Mesh();
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        mesh.vertices = newVertices;
        mesh.triangles = newTrianglePoints;
        mesh.uv = newUVs;
        meshFilter.mesh.RecalculateBounds();
        meshFilter.mesh.RecalculateNormals();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        Renderer renderer = meshRenderer.GetComponent<Renderer>();
        renderer.material.color = Color.blue;
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        if (material) {
            renderer.material = material;
        }
        else {
            renderer.material.mainTexture = debugTex;
        }
    }

    //construct an extruded, closed surface, with shape depending on input mode (n = number of pieces to split the pipe into)
    void generatePipe(string mode, int n) {
        initNewMesh(2*n);

        buildVerts(n, 1, .1f, false);
        buildTris(n);
        buildUVs(n / 2, false,newUVs.Length / 2,0);

        buildVerts(n, 1, .1f, true,n);
        buildTris(n,n);
        buildUVs(n / 2, true, newUVs.Length / 2, newUVs.Length / 2);

        finalizeMesh();
    }

    // construct a flat nxn rectangular mesh (n = number of pieces to split the mesh into)
    void generateMesh(string mode, int n) {
        initNewMesh(n);
        if (mode == "normal") {
            buildVerts(n);
        }
        else if (mode == "wavy") {
            buildVertsWavy(n);
        }
        buildTris(n);
        buildUVs(n/2);
        finalizeMesh();	
	}	
}