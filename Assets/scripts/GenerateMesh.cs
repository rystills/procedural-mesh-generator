using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerateMesh : MonoBehaviour {
	public Material material;
    List<Vector3> newVertices;
    List<int> newTrianglePoints;
    List<Vector2> newUVs;
    Texture2D debugTex;


    void Start() {
        //init mesh lists
        newVertices = new List<Vector3>();
        newTrianglePoints = new List<int>();
        newUVs = new List<Vector2>();
        //build debug texture as a fallback if no material is supplied
        debugTex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        debugTex.SetPixel(0, 0, Color.red);
        debugTex.SetPixel(1, 0, Color.green);
        debugTex.SetPixel(0, 1, Color.blue);
        debugTex.SetPixel(1, 1, Color.yellow);
        debugTex.wrapMode = TextureWrapMode.Repeat;
        debugTex.Apply();
        //generatePipe("normal",1);
        generateMesh("normal", 1,1);
	}

    //construct an extruded, closed surface, with shape depending on input mode (n = number of pieces to split the pipe into)
    /*void generatePipe(string mode, int n) {
        initNewMesh(n,n, 4);

        buildVerts(n,n, 1, .1f, false);
        buildTris(n,n,0,true);
        buildUVs(n, false, newUVs.Length / 2, 0);

        buildVerts(n,n, 1, .1f, true, (n + 1) * (n + 1),0,0,.1f);
        buildTris(n,n, 6 * n * n,true);
        buildUVs(n, true, newUVs.Length / 4, (n + 1) * (n + 1));

        buildVerts(n,n, 1, .1f, true, 2 * ((n + 1) * (n + 1)));
        buildTris(n,n, 2 * (6 * n * n));
        buildUVs(n, true, newUVs.Length / 4, 2 * ((n + 1) * (n + 1)));

        buildVerts(n,n, 1, .1f, false, 3 * ((n + 1) * (n + 1)), 0, .1f, 0);
        buildTris(n,n, 3 * (6 * n * n));
        buildUVs(n, false, newUVs.Length / 4, 3 * ((n + 1) * (n + 1)));

        finalizeMesh();
    }*/

    // construct a flat mxn rectangular mesh (m = length segs, n = width segs)
    void generateMesh(string mode, int m, int n = 0) {
        n = (n == 0 ? m : n);
        //initNewMesh(m,n);
        if (mode == "normal") {
            float xPos = 0;
            float quadSize = 1;
            for (int i = 0; i < m; ++i) {
                float yPos = 0;
                for (int r = 0; r < n; ++r) {
                    propogateQuad(xPos,yPos, quadSize);
                    yPos += quadSize;
                }
                xPos++;
            }
        }
        /*else if (mode == "wavy") {
            buildVertsWavy(n);
        }
        buildTris(n,m);
        buildUVs(Mathf.Max(m,n));*/
        finalizeMesh();
    }

    void propogateQuad(float xPos, float yPos, float quadSize) {
        //step 1: generate the necessary verts and corresponding UVs
        //case 1: there are no verts currently, so generate our first 2 side verts
        if (newVertices.Count == 0) {
            newVertices.Add(new Vector3(xPos, yPos, 0));
            newUVs.Add(new Vector2(xPos, yPos));
            newVertices.Add(new Vector3(xPos, yPos + quadSize, 0));
            newUVs.Add(new Vector2(xPos, yPos + quadSize));
        }
        //case 2: we have our 2 side verts; add two more side verts and generate our tris and UVs to match
        newVertices.Add(new Vector3(xPos + quadSize, yPos, 0));
        newUVs.Add(new Vector2(xPos + quadSize, yPos));
        newVertices.Add(new Vector3(xPos + quadSize, yPos + quadSize, 0));
        newUVs.Add(new Vector2(xPos + quadSize, yPos + quadSize));

        //step 2: generate the necessary tris (because this method adds a single quad, we need two new triangles, or 6 points in our list of tris)
        //first new tri
        newTrianglePoints.Add(newVertices.Count - 4);
        newTrianglePoints.Add(newVertices.Count - 3);
        newTrianglePoints.Add(newVertices.Count - 2);
        //second new tri
        newTrianglePoints.Add(newVertices.Count - 3);
        newTrianglePoints.Add(newVertices.Count - 1);
        newTrianglePoints.Add(newVertices.Count - 2);
    }

    //initialize a new procedural mesh (n = number of quads)
    /*void initNewMesh(int m,int n=0, int factor = 1) {
        n = (n == 0 ? m : n);
        newVertices = new Vector3[factor * ((n + 1) * (m + 1))]; //one square mesh has 4 vertices, and each subsequent triange adds one new vert (therefore two per additional quad)
        newTrianglePoints = new int[factor * (6 * n * m)]; //n^2 total quads. each quad is 2 tris. multiply by 3 as each tri is described by 3 points
        newUVs = new Vector2[newVertices.Length]; //number of UVs should match number of vertices for proper mapping
    }*/

    //construct vertices (n = number of quads)
    /*void buildVerts(int m, int n, float xChange = .55f, float yChange = .55f, bool orientUp = false, int startOffset = 0, float xOffset = 0, float yOffset = 0, float zOffset = 0) {
        for (int x = 0, listPos = startOffset; x < m + 1; x++) {
            for (int y = 0; y < n + 1; y++, listPos++) {
                newVertices[listPos] = new Vector3(xOffset + x * xChange, yOffset + y * yChange * (orientUp ? 1 : 0), zOffset + y * yChange * (orientUp ? 0 : 1));
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
    void buildTris(int m, int n, int startOffset = 0,bool flipNormals = false) {
        for (int x = 0, listPos = startOffset; x < m; x++) {
            for (int y = 0; y < (n); y++, listPos += 6) {
                int offsetX = x, offsetY = 4 * (int)(startOffset / (6 * m * n)) + y;
                newTrianglePoints[listPos + (flipNormals ? 5 : 0)] = ((m + 1) * offsetX) + (offsetY);
                newTrianglePoints[listPos + 4] = newTrianglePoints[listPos + 1] = ((m + 1) * offsetX) + (offsetY + 1);
                newTrianglePoints[listPos + 3] = newTrianglePoints[listPos + 2] = ((m + 1) * offsetX) + (offsetY + (n + 1));
                newTrianglePoints[listPos + (flipNormals ? 0 : 5)] = ((m + 1) * offsetX) + (offsetY + (n + 2));
            }
        }
    }

    //construct UVs, repeating based on some scale factor (for a flat UVW unwrap with a straight-up orientation, we can just map to x,z coords)
    void buildUVs(float factor, bool orientUp = false, int n = 0, int startOffset = 0) {
        n = (n == 0 ? newUVs.Length : n);
        for (int i = startOffset; i < startOffset + n; newUVs[i] = new Vector2(newVertices[i].x / factor, (orientUp ? newVertices[i].y : newVertices[i].z) / factor), i++) ;
    }*/

    //construct the new mesh, and attach the appropriate components
    void finalizeMesh() {
        Mesh mesh = new Mesh();
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTrianglePoints.ToArray();
        mesh.uv = newUVs.ToArray();
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
}