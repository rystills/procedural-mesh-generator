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
        generateMesh("standard",10);
	}

    //initialize a new procedural mesh (n = number of quads)
    void initNewMesh(int n) {
        newVertices = new Vector3[(n + 1) * (n + 1)]; //one square mesh has 4 vertices, and each subsequent triange adds one new vert (therefore two per additional quad)
        newTrianglePoints = new int[6 * n * n]; //n^2 total quads. each quad is 2 tris. multiply by 3 as each tri is described by 3 points
        newUVs = new Vector2[newVertices.Length]; //number of UVs should match number of vertices for proper mapping
    }

    //construct vertices (n = number of quads)
    void buildVerts(int n) {
        for (int x = 0, listPos = 0; x < n + 1; x++) {
            for (int y = 0; y < n + 1; y++, listPos++) {
                newVertices.SetValue(new Vector3(x * .55f, 0, y * .55f), listPos);
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
                newVertices.SetValue(new Vector3(x * .55f, averageLocalY + (Random.Range(-0.4f, 0.4f)), y * .55f), listPos);
            }
        }
    }

    //construct trianges (n = number of quads) 
    void buildTris(int n) {
        for (int x = 0, listPos = 0; x < (n); x += 1) {
            for (int y = 0; y < (n); y += 1, listPos += 6) {
                newTrianglePoints.SetValue(((n + 1) * x) + (y), listPos);
                newTrianglePoints.SetValue(((n + 1) * x) + (y + 1), listPos + 1);
                newTrianglePoints.SetValue(((n + 1) * x) + (y + (n + 1)), listPos + 2);
                newTrianglePoints.SetValue(((n + 1) * x) + (y + (n + 1)), listPos + 3);
                newTrianglePoints.SetValue(((n + 1) * x) + (y + 1), listPos + 4);
                newTrianglePoints.SetValue(((n + 1) * x) + (y + (n + 2)), listPos + 5);
            }
        }
    }

    //construct UVs
    void buildUVs() {
        for (int i = 0; i < newUVs.Length; newUVs[i] = new Vector2(newVertices[i].x / (5), newVertices[i].z / (5)), i++) ;
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

    // construct a flat nxn rectangular mesh (n = number of pieces to split the mesh into)
    void generateMesh(string mode, int n) {
        initNewMesh(n);
        if (mode == "standard") {
            buildVerts(n);
        }
        else if (mode == "wavy") {
            buildVertsWavy(n);
        }
        buildTris(n);
        buildUVs();
        finalizeMesh();	
	}	
}