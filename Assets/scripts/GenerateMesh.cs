using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        //generateMesh("normal", 3,5);
        generateBox(4, 5, 3);
    }

    //construct a closed box, with each side is mxn quads (m = x segments, n = y segmenet); adapted from generateMesh
    void generateBox(int numSides, int m, int n) {
        //give n a default value of m if it is not specified
        n = (n == 0 ? m : n);
        float quadSize = 1;
        string[] axes = new string[2];

        //generate front
        axes[0] = "x"; axes[1] = "y";
        float xPos = 0;
        //outer loop: iterate over the x axis
        for (int i = 0; i < m; ++i) {
            float yPos = 0;
            //inner loop: create quads while iterating over the y axis
            for (int j = 0; j < n; ++j) {
                propogateQuad(xPos, yPos, 0, axes, quadSize);
                yPos += quadSize;
            }
            xPos += quadSize;
        }

        //generate back
        axes[0] = "x"; axes[1] = "y";
        xPos = 0;
        //outer loop: iterate over the x axis
        for (int i = 0; i < m; ++i) {
            float yPos = 0;
            //inner loop: create quads while iterating over the y axis
            for (int j = 0; j < n; ++j) {
                propogateQuad(xPos, yPos, quadSize * m, axes, quadSize, true);
                yPos += quadSize;
            }
            xPos += quadSize;
        }

        //generate left
        axes[0] = "z"; axes[1] = "y";
        float zPos = 0;
        //outer loop: iterate over the x axis
        for (int i = 0; i < m; ++i) {
            float yPos = 0;
            //inner loop: create quads while iterating over the y axis
            for (int j = 0; j < n; ++j) {
                propogateQuad(0, yPos, zPos, axes, quadSize, true);
                yPos += quadSize;
            }
            zPos += quadSize;
        }

        //generate right
        axes[0] = "z"; axes[1] = "y";
        zPos = 0;
        //outer loop: iterate over the x axis
        for (int i = 0; i < m; ++i) {
            float yPos = 0;
            //inner loop: create quads while iterating over the y axis
            for (int j = 0; j < n; ++j) {
                propogateQuad(quadSize * m, yPos, zPos, axes, quadSize, false);
                yPos += quadSize;
            }
            zPos += quadSize;
        }

        //generate top
        axes[0] = "z"; axes[1] = "x";
        zPos = 0;
        //outer loop: iterate over the x axis
        for (int i = 0; i < m; ++i) {
            xPos = 0;
            //inner loop: create quads while iterating over the y axis
            for (int j = 0; j < m; ++j) {
                propogateQuad(xPos, 0, zPos, axes, quadSize, false);
                xPos += quadSize;
            }
            zPos += quadSize;
        }

        //generate bottom
        axes[0] = "z"; axes[1] = "x";
        zPos = 0;
        //outer loop: iterate over the x axis
        for (int i = 0; i < m; ++i) {
            xPos = 0;
            //inner loop: create quads while iterating over the y axis
            for (int j = 0; j < m; ++j) {
                propogateQuad(xPos, quadSize * n, zPos, axes, quadSize, true);
                xPos += quadSize;
            }
            zPos += quadSize;
        }
        finalizeMesh();
    }

    // construct a flat mxn rectangular mesh (m = x segments, n = y segments)
    void generateMesh(string mode, int m, int n = 0) {
        //give n a default value of m if it is not specified
        n = (n == 0 ? m : n);
        if (mode == "normal") {
            float quadSize = 1;
            float xPos = 0;
            string[] axes = new string[2];
            axes[0] = "x"; axes[1] = "y";
            //outer loop: iterate over the x axis
            for (int i = 0; i < m; ++i) {
                float yPos = 0;
                //inner loop: create quads while iterating over the y axis
                for (int j = 0; j < n; ++j) {
                    propogateQuad(xPos, yPos, 0, axes, quadSize);
                    yPos += quadSize;
                }
                xPos += quadSize;
            }
        }
        finalizeMesh();
    }

    //create an additional quad from xPos,yPos of size quadSize going in direction dir ('x', 'y', or 'z' for now)
    void propogateQuad(float xPos, float yPos, float zPos, string[] axes, float quadSize, bool flip = false) {
        //step 1: generate the necessary verts, and corresponding UVs
        //generate 2 verts for first side
        addVert(xPos, yPos, zPos, axes);
        addVert(xPos + (axes[0] == "x" ? 0 : axes.Contains("x") ? quadSize : 0), yPos + (axes[0] == "y" ? 0 : axes.Contains("y") ? quadSize : 0), zPos + (axes[0] == "z" ? 0 : axes.Contains("z") ? quadSize : 0), axes);            
        //generate 2 verts for second sdie
        addVert(xPos + (axes[0] == "x" ? quadSize : 0), yPos + (axes[0] == "y" ? quadSize : 0), zPos + (axes[0] == "z" ? quadSize : 0), axes);
        addVert(xPos + (axes.Contains("x") ? quadSize : 0), yPos + (axes.Contains("y") ? quadSize : 0), zPos + (axes.Contains("z") ? quadSize : 0), axes);

        //step 2: generate the necessary tris (because this method adds a single quad, we need two new triangles, or 6 points in our list of tris)
        int topLeftIndex = vertIndices[new Vector3(xPos + (axes.Contains("x") ? quadSize : 0), yPos + (axes.Contains("y") ? quadSize : 0), zPos + (axes.Contains("z") ? quadSize : 0))];
        int botLeftIndex = vertIndices[new Vector3(xPos + (axes[0] == "x" ? quadSize : 0), yPos + (axes[0] == "y" ? quadSize : 0), zPos + (axes[0] == "z" ? quadSize : 0))];
        int topRightIndex = vertIndices[new Vector3(xPos + (axes[0] == "x" ? 0 : axes.Contains("x") ? quadSize : 0), yPos + (axes[0] == "y" ? 0 : axes.Contains("y") ? quadSize : 0), zPos + (axes[0] == "z" ? 0 : axes.Contains("z") ? quadSize : 0))];
        int botRightIndex = vertIndices[new Vector3(xPos,yPos, zPos)];
        //first new tri
        addTri(botRightIndex, topRightIndex, botLeftIndex,flip);
        //second new tri
        addTri(topRightIndex, topLeftIndex, botLeftIndex, flip);
    }

    //add a new vert with corresponding UVs if xPos,yPos does not already contain one, and add this vert's position in newVertices to vertIndices
    void addVert(float xPos, float yPos, float zPos, string[] axes) {
        //make sure there is not already a vertex at xPos,yPos 
        if (vertIndices.ContainsKey(new Vector3(xPos, yPos, zPos))) {
            return;
        }
        newVertices.Add(new Vector3(xPos, yPos, zPos));
        newUVs.Add(new Vector2(axes[0] == "x" ? xPos : axes[0] == "y" ? yPos : zPos, axes[1] == "x" ? xPos : axes[1] == "y" ? yPos : zPos));
        vertIndices[newVertices[newVertices.Count - 1]] = newVertices.Count - 1;
    }

    //simple helper method to add 3 points to the newTrianglePoints list
    void addTri(int index1, int index2, int index3,bool flip = false) {
        newTrianglePoints.Add(flip? index3 : index1);
        newTrianglePoints.Add(index2);
        newTrianglePoints.Add(flip ? index1 : index3);
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