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
        generateMesh("normal", 3,5);
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
    void propogateQuad(float xPos, float yPos, float zPos, string[] axes, float quadSize) {
        //step 1: generate the necessary verts, and corresponding UVs
        //generate 2 verts for first side
        addVert(xPos, yPos, zPos);
        addVert(xPos + (axes[0] == "x" ? 0 : axes.Contains("x") ? quadSize : 0), yPos + (axes[0] == "y" ? 0 : axes.Contains("y") ? quadSize : 0), zPos + (axes[0] == "z" ? 0 : axes.Contains("z") ? quadSize : 0));            
        //generate 2 verts for second sdie
        addVert(xPos + (axes[0] == "x" ? quadSize : 0), yPos + (axes[0] == "y" ? quadSize : 0), zPos + (axes[0] == "z" ? quadSize : 0));
        addVert(xPos + (axes.Contains("x") ? quadSize : 0), yPos + (axes.Contains("y") ? quadSize : 0), zPos + (axes.Contains("z") ? quadSize : 0));

        //step 2: generate the necessary tris (because this method adds a single quad, we need two new triangles, or 6 points in our list of tris)
        int topLeftIndex = vertIndices[new Vector3(xPos + quadSize, yPos + quadSize, zPos)];
        int botLeftIndex = vertIndices[new Vector3(xPos + quadSize, yPos, zPos)];
        int topRightIndex = vertIndices[new Vector3(xPos, yPos + quadSize, zPos)];
        int botRightIndex = vertIndices[new Vector3(xPos,yPos, zPos)];
        //first new tri
        addTri(botRightIndex, topRightIndex, botLeftIndex);
        //second new tri
        addTri(topRightIndex, topLeftIndex, botLeftIndex);
    }

    //add a new vert with corresponding UVs if xPos,yPos does not already contain one, and add this vert's position in newVertices to vertIndices
    void addVert(float xPos, float yPos, float zPos) {
        //make sure there is not already a vertex at xPos,yPos 
        if (vertIndices.ContainsKey(new Vector3(xPos, yPos, zPos))) {
            return;
        }
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