using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerateMesh : MonoBehaviour {
    public Material material;
    List<Vector3> newVertices;
    List<int> newTrianglePoints;
    List<Vector2> newUVs;
    Dictionary<Vector3, int> vertIndices;
    Texture2D debugTex;


    void Start() {
        //init mesh lists
        newVertices = new List<Vector3>();
        newTrianglePoints = new List<int>();
        newUVs = new List<Vector2>();
        vertIndices = new Dictionary<Vector3, int>();
        //build debug texture as a fallback if no material is supplied
        debugTex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        debugTex.SetPixel(0, 0, Color.red);
        debugTex.SetPixel(1, 0, Color.green);
        debugTex.SetPixel(0, 1, Color.blue);
        debugTex.SetPixel(1, 1, Color.yellow);
        debugTex.wrapMode = TextureWrapMode.Repeat;
        debugTex.Apply();
        generateMesh("normal", 2,2);
    }

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
                    propogateQuad(xPos, yPos, r == 0 ? "x" : "y", r == 1 ? "x" : "y", quadSize);
                    yPos += quadSize;
                }
                xPos += quadSize;
            }
        }
        finalizeMesh();
    }

    void propogateQuad(float xPos, float yPos, string dir, string prevDir, float quadSize) {
        //step 1: generate the necessary verts and corresponding UVs
        //there are no verts currently, so generate our first 2 side verts
        if (newVertices.Count == 0) {
            addVert(xPos, yPos, 0);
            addVert(xPos + (dir == "x" ? 0 : quadSize), yPos + (dir == "y" ? 0 : quadSize), 0);            
        }
        //we now have our 2 side verts; add two more side verts
        addVert(xPos + (dir != "x" ? 0 : quadSize), yPos + (dir != "y" ? 0 : quadSize), 0);
        addVert(xPos + quadSize, yPos + quadSize, 0);

        //step 2: generate the necessary tris (because this method adds a single quad, we need two new triangles, or 6 points in our list of tris)
        int topLeftIndex = vertIndices[new Vector3(xPos + quadSize, yPos + quadSize, 0)];
        int botLeftIndex = vertIndices[new Vector3(xPos + quadSize, yPos, 0)];
        int topRightIndex = vertIndices[new Vector3(xPos, yPos + quadSize, 0)];
        int botRightIndex = vertIndices[new Vector3(xPos,yPos, 0)];
        //first new tri
        addTri(botRightIndex, topRightIndex, botLeftIndex);
        //second new tri
        addTri(topRightIndex, topLeftIndex, botLeftIndex);
    }

    //add a new vert with corresponding UVs, and add this vert's position in newVertices to vertIndices
    void addVert(float xPos, float yPos, float zPos) {
        newVertices.Add(new Vector3(xPos, yPos, zPos));
        newUVs.Add(new Vector2(xPos, yPos));
        vertIndices[newVertices[newVertices.Count - 1]] = newVertices.Count - 1;
    }

    //simple helper method to add 3 points to the newTrianglePoints list
    void addTri(int index1, int index2, int index3) {
        newTrianglePoints.Add(index1);
        newTrianglePoints.Add(index2);
        newTrianglePoints.Add(index3);
    }

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