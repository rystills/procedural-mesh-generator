//LEGACY SCRIPT! generate a mesh by specifying movements along the 3 axes 
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GenerateMeshAxes : MonoBehaviour {
	public Material material;
	List<Vector3> newVertices;
	List<Vector3> newNormals;
	List<int> newTrianglePoints;
	List<Vector2> newUVs;
	Dictionary<Vector3, Dictionary<string[], int>> vertIndicesAxes;
	Dictionary<Vector3, Dictionary<Quaternion, List<int>>> connectedVertIDs;
	Texture2D debugTex;
	const float smoothnessFloatTolerance = .5f; //tolerance applied to all direction comparisons to compensate for floating point imprecision
	const float normalAverageMaxDifference = 45; //normals of overlapping vertices will not be averaged if their starting normals are larger than this value

	void Start() {
		//init mesh lists
		newVertices = new List<Vector3>();
		newNormals = new List<Vector3>();
		newTrianglePoints = new List<int>();
		newUVs = new List<Vector2>();
		vertIndicesAxes = new Dictionary<Vector3, Dictionary<string[], int>>();
		connectedVertIDs = new Dictionary<Vector3, Dictionary<Quaternion, List<int>>>();
		//build debug texture as a fallback if no material is supplied
		debugTex = new Texture2D(2, 2);
		debugTex.SetPixel(0, 0, Color.red);
		debugTex.SetPixel(1, 0, Color.magenta);
		debugTex.SetPixel(0, 1, Color.blue);
		debugTex.SetPixel(1, 1, Color.cyan);
		debugTex.wrapMode = TextureWrapMode.Repeat;
		debugTex.Apply();
		
		generateBoxAxes(3, 5, 7);
		
		finalizeMesh();
	}

	//construct a closed box, with length, width, height segments; adapted from generateMesh
	void generateBoxAxes(int length, int width, int height) {
		float quadSize = 1;
		string[] allAxes = { "x", "y", "z" };
		int[] allDims = { length, width, height };
		//generate sides in groups of 2; front and back, then left and right, then finally top and bottom
		for (int k = 0; k < 3; ++k) {
			string[] axes = { allAxes[k], allAxes[(k + 1) % allAxes.Length] };
			for (int l = 0; l < 2; ++l) {
				float[] position = { 0, 0, 0 };
				//if l is 0, generate first side (no offset); otherwise, generate opposte side (quadSize offset)
				position[(k + 2) % 3] = l == 0 ? 0 : allDims[(k + 2) % allDims.Length] * quadSize;
				//outer loop: iterate over the primary axis
				for (int i = 0; i < allDims[k]; ++i) {
					position[(k + 1) % 3] = 0;
					//inner loop: create quads while iterating over the secondary axis
					for (int j = 0; j < allDims[(k + 1) % allDims.Length]; ++j) {
						propagateQuadAxes(position, axes, quadSize, l != 0);
						position[(k + 1) % 3] += quadSize;
					}
					position[k] += quadSize;
				}
			}
		}
		finalizeMesh(true);
	}

	//create an additional quad from position[] of size quadSize on axes
	void propagateQuadAxes(float xPos, float yPos, float zPos, string[] axes, float quadSize, bool flip = false) {
		//step 1: generate the necessary verts, and corresponding UVs
		//generate 2 verts for first side
		addVertAxes(xPos, yPos, zPos, axes);
		addVertAxes(xPos + (axes[0] == "x" ? 0 : axes.Contains("x") ? quadSize : 0), yPos + (axes[0] == "y" ? 0 : axes.Contains("y") ? quadSize : 0), zPos + (axes[0] == "z" ? 0 : axes.Contains("z") ? quadSize : 0), axes);
		//generate 2 verts for second sdie
		addVertAxes(xPos + (axes[0] == "x" ? quadSize : 0), yPos + (axes[0] == "y" ? quadSize : 0), zPos + (axes[0] == "z" ? quadSize : 0), axes);
		addVertAxes(xPos + (axes.Contains("x") ? quadSize : 0), yPos + (axes.Contains("y") ? quadSize : 0), zPos + (axes.Contains("z") ? quadSize : 0), axes);

		//step 2: generate the necessary tris (because this method adds a single quad, we need two new triangles, or 6 points in our list of tris)
		int topLeftIndex = getVertAxes(xPos + (axes.Contains("x") ? quadSize : 0), yPos + (axes.Contains("y") ? quadSize : 0), zPos + (axes.Contains("z") ? quadSize : 0), axes);
		int botLeftIndex = getVertAxes(xPos + (axes[0] == "x" ? quadSize : 0), yPos + (axes[0] == "y" ? quadSize : 0), zPos + (axes[0] == "z" ? quadSize : 0), axes);
		int topRightIndex = getVertAxes(xPos + (axes[0] == "x" ? 0 : axes.Contains("x") ? quadSize : 0), yPos + (axes[0] == "y" ? 0 : axes.Contains("y") ? quadSize : 0), zPos + (axes[0] == "z" ? 0 : axes.Contains("z") ? quadSize : 0), axes);
		int botRightIndex = getVertAxes(xPos, yPos, zPos, axes);
		//first new tri
		addTriAxes(botRightIndex, topRightIndex, botLeftIndex, flip);
		//second new tri
		addTriAxes(topRightIndex, topLeftIndex, botLeftIndex, flip);
	}

	void propagateQuadAxes(float[] positions, string[] axes, float quadSize, bool flip = false) {
		propagateQuadAxes(positions[0], positions[1], positions[2], axes, quadSize, flip);
	}

	//add a new vert with corresponding UVs if xPos,yPos does not already contain one, and add this vert's position in newVertices to vertIndicesAxes
	void addVertAxes(float xPos, float yPos, float zPos, string[] axes) {
		//make sure there is not already a vertex at xPos,yPos 
		if (getVertAxes(xPos, yPos, zPos, axes) != -1) {
			return;
		}
		newVertices.Add(new Vector3(xPos, yPos, zPos));
		newUVs.Add(new Vector2(axes[0] == "x" ? xPos : axes[0] == "y" ? yPos : zPos, axes[1] == "x" ? xPos : axes[1] == "y" ? yPos : zPos));
		setVertAxes(newVertices[newVertices.Count - 1], axes, newVertices.Count - 1);
	}

	//set vertex at pos, facing in dir axes
	void setVertAxes(Vector3 pos, string[] axes, int index) {
		if (!vertIndicesAxes.ContainsKey(pos)) {
			vertIndicesAxes[pos] = new Dictionary<string[], int>();
		}
		vertIndicesAxes[pos][axes] = index;
	}

	//return index of vert at xPos, yPos, zPos facing in dir axes, -1 if not present
	int getVertAxes(float xPos, float yPos, float zPos, string[] axes) {
		if (!vertIndicesAxes.ContainsKey(new Vector3(xPos, yPos, zPos))) {
			return -1;
		}
		Dictionary<string[], int> key = vertIndicesAxes[new Vector3(xPos, yPos, zPos)];
		if (!key.ContainsKey(axes)) {
			return -1;
		}
		return key[axes];
	}

	//simple helper method to add 3 points to the newTrianglePoints list
	void addTriAxes(int index1, int index2, int index3, bool flip = false) {
		newTrianglePoints.Add(flip ? index3 : index1);
		newTrianglePoints.Add(index2);
		newTrianglePoints.Add(flip ? index1 : index3);
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
					propagateQuadAxes(xPos, yPos, 0, axes, quadSize);
					yPos += quadSize;
				}
				xPos += quadSize;
			}
		}
		finalizeMesh();
	}

	//construct the new mesh, and attach the appropriate components
	void finalizeMesh(bool useUnityNormals = false) {
		Mesh mesh = new Mesh();
		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		if (!meshFilter) {
			meshFilter = gameObject.AddComponent<MeshFilter>();
		}
		meshFilter.mesh = mesh;
		mesh.vertices = newVertices.ToArray();
		mesh.triangles = newTrianglePoints.ToArray();
		mesh.uv = newUVs.ToArray();
		meshFilter.mesh.RecalculateBounds();
		
		meshFilter.mesh.RecalculateNormals();
		
		MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
		if (!meshRenderer) {
			meshRenderer = gameObject.AddComponent<MeshRenderer>();
		}
		Renderer renderer = meshRenderer.GetComponent<Renderer>();
		renderer.material.color = Color.blue;
		MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
		if (!meshCollider) {
			meshCollider = gameObject.AddComponent<MeshCollider>();
		}
		meshCollider.sharedMesh = mesh;
		if (material) {
			renderer.material = material;
		}
		else {
			renderer.material.mainTexture = debugTex;
			renderer.material.color = Color.white;
		}
	}
}