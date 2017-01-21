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
        //case 1: there are no verts currently, so generate our first 2 side verts
        if (newVertices.Count == 0) {
            newVertices.Add(new Vector3(xPos, yPos, 0));
            newUVs.Add(new Vector2(xPos, yPos));
            newVertices.Add(new Vector3(xPos + (dir == "x" ? 0 : quadSize), yPos + (dir == "y" ? 0 : quadSize), 0));
            newUVs.Add(new Vector2(xPos + (dir == "x" ? 0 : quadSize), yPos + (dir == "y" ? 0 : quadSize)));
        }
        //case 2: we have our 2 side verts; add two more side verts and generate our tris and UVs to match
        newVertices.Add(new Vector3(xPos + (dir != "x" ? 0 : quadSize), yPos + (dir != "y" ? 0 : quadSize), 0));
        newUVs.Add(new Vector2(xPos + (dir != "x" ? 0 : quadSize), yPos + (dir != "y" ? 0 : quadSize)));
        newVertices.Add(new Vector3(xPos + quadSize, yPos + quadSize, 0));
        newUVs.Add(new Vector2(xPos + quadSize, yPos + quadSize));

        //step 2: generate the necessary tris (because this method adds a single quad, we need two new triangles, or 6 points in our list of tris)
        int botRightIndex = newVertices.Count - 4, topRightIndex = newVertices.Count - 2, botLeftIndex = newVertices.Count - 3, topLeftIndex = newVertices.Count - 1;
        if (dir == "y") {
            topRightIndex = newVertices.Count - 3;
            botLeftIndex = newVertices.Count - 2;
            botRightIndex = newVertices.Count - (prevDir == "x" ? 5 : 4);
        }
        //first new tri
        addTri(botRightIndex, topRightIndex, botLeftIndex);
        //second new tri
        addTri(topRightIndex, topLeftIndex, botLeftIndex);
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