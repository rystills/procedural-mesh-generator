using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GenerateMesh : MonoBehaviour {
	public Material material;
	public Material material2;
	public List<int> triangles;
	public VertexDict vertDict;
	public List<Vector3> vertices;
	public List<Vector2> uvs;
	public List<Vector3> normals;
	Texture2D debugTex;

	void Awake() {
		//new mesh lists
		triangles = new List<int>();
		vertDict = new VertexDict();
		vertices = new List<Vector3>();
		uvs = new List<Vector2>();
		normals = new List<Vector3>();
		//build debug texture as a fallback if no material is supplied
		debugTex = new Texture2D(2, 2);
		debugTex.SetPixel(0, 0, Color.red);
		debugTex.SetPixel(1, 0, Color.magenta);
		debugTex.SetPixel(0, 1, Color.blue);
		debugTex.SetPixel(1, 1, Color.cyan);
		debugTex.wrapMode = TextureWrapMode.Repeat;
		debugTex.Apply();
	}

	//utility functions
	//rotate quaternion quat on axis rotAxis by amount 
	public Quaternion rotateQuaternion(Quaternion quat, Vector3 rotAxis, float amount) {
		return quat * Quaternion.Euler(rotAxis * amount);
	}

	//return Quaternion quat rotated by 180 degrees to face in the opposite direction (useful for flipping normals)
	public Quaternion flipQuaternion(Quaternion quat) {
		return (new Quaternion(1, 0, 0, 0)) * quat;
	}

	//find and return the rotation of the vertex which corresponds to vertIndex
	public VertexData getVertData(int vertIndex) {
		VertexData vert = vertDict.getVert(vertices[vertIndex], normals[vertIndex]);
		if (vert != null) {
			return vert;
		}
		throw new System.Exception("error: vertex #" + vertIndex + " not found in vertDict");
	}

	//return the normal vector between vectors a,b,c
	public Vector3 calculateNormal(Vector3 a, Vector3 b, Vector3 c) {
		Vector3 side1 = b - a;
		Vector3 side2 = c - a;
		Vector3 perp = Vector3.Cross(side1, side2);
		return perp / perp.magnitude;
	}	

	//core generators  
	//create an additional quad from position[] of size quadsize in direction dir (returns ending position)
	public Vector3 propagateQuad(Vector3 pos, Quaternion dir, float width, float extents, bool flip = false, float vertSmoothnessThreshold = 0, string uvMode = "per face") {
		//calculate forward and left vectors from rotation Quaternion 
		Vector3 forwardDir = dir * Vector3.forward;
		Vector3 leftDir = rotateQuaternion(dir, new Vector3(1, 0, 0), 90) * Vector3.forward;

		//calculate 3 remaining positions from forward and left vectors
		Vector3 topRightPos = pos + (forwardDir.normalized * width);
		Vector3 botLeftPos = pos + (leftDir.normalized * extents);
		Vector3 topLeftPos = botLeftPos + (forwardDir.normalized * width);

		return generateQuad(pos, flip ? botLeftPos : topRightPos, flip ? topRightPos : botLeftPos, topLeftPos, vertSmoothnessThreshold, flip? "topRight" : "botLeft", uvMode);
	}

	public Vector3 generateQuad(Vector3 botRightPos, Vector3 topRightPos, Vector3 botLeftPos, Vector3? topLeftPos = null, float vertSmoothnessThreshold = 0, string returnPos = "botLeft", string uvMode = "per face") {
		//calculate normal dir
		Vector3 normal = calculateNormal(botRightPos, topRightPos, botLeftPos);

		//generate botRight vert
		VertexData botRightVert = vertDict.getVert(botRightPos, normal);
		if (botRightVert == null) {
			botRightVert = addVert(botRightPos, normal, vertSmoothnessThreshold);
			addUV(botRightVert, botRightPos, topRightPos, botLeftPos, uvMode);
		}
		//generate topRight vert
		VertexData topRightVert = vertDict.getVert(topRightPos, normal);
		if (topRightVert == null) {
			topRightVert = addVert(topRightPos, normal, vertSmoothnessThreshold);
			addUV(topRightVert, botRightPos, topRightPos, botLeftPos, uvMode);
		}
		//generate botLeft vert
		VertexData botLeftVert = vertDict.getVert(botLeftPos, normal);
		if (botLeftVert == null) {
			botLeftVert = addVert(botLeftPos, normal, vertSmoothnessThreshold);
			addUV(botLeftVert, botRightPos, topRightPos, botLeftPos, uvMode);
		}
		//generate topLeft vert, if it exists (otherwise we are just going to build one tri)
		VertexData topLeftVert = null;
		if (topLeftPos.HasValue) {
			topLeftVert = vertDict.getVert(topLeftPos.Value, normal);
			if (topLeftVert == null) {
				topLeftVert = addVert(topLeftPos.Value, normal, vertSmoothnessThreshold);
				addUV(topLeftVert, botRightPos, topRightPos, botLeftPos, uvMode);
			}
		}

		//generate the necessary tris (because this method adds a single quad, we need two new triangles, or 6 points in our list of tris)
		//first new tri
		addTri(botRightVert, topRightVert, botLeftVert);
		//second new tri
		if (topLeftVert != null) {
			addTri(topRightVert, topLeftVert, botLeftVert);
		}
		
		return returnPos == "botLeft" ? botLeftPos : topRightPos;
	}

	//tri modifiers

	//simple helper method to add 3 points to the triangles list
	public void addTri(VertexData vert1, VertexData vert2, VertexData vert3, bool flip = false, bool addTriangleIndices = true) {
		triangles.Add(flip ? vert3.verticesIndex : vert1.verticesIndex);
		triangles.Add(vert2.verticesIndex);
		triangles.Add(flip ? vert1.verticesIndex : vert3.verticesIndex);

		if (addTriangleIndices) {
			//add a triangles index to each vert
			vert1.addTriangleIndex(triangles.Count - (flip ? 1 : 3));
			vert1.addTriangleIndex(triangles.Count - 2);
			vert1.addTriangleIndex(triangles.Count - (flip ? 3 : 1));
		}
	}

	//vert modifiers
	//add a new vert with corresponding UVs if xPos,yPos does not already contain one, and add this vert's position in vertices to vertIndicesAxes
	public VertexData addVert(Vector3 pos, Vector3 normal, float vertSmoothnessthreshold) {
		//if vert already exists, return it. if not, create it first
		VertexData newVert = vertDict.getVert(pos, normal);
		if (newVert == null) {
			vertices.Add(pos);
			normals.Add(normal);
			newVert = vertDict.addVert(vertices.Count - 1, pos, normal);
		}
		return newVert;
	}

	//UV modifiers
	//calculate UV for point pos given points a,b,c (pos will typically be equivalent to one of these 3 points)
	public void addUV(VertexData vertData, Vector3 a, Vector3 b, Vector3 c, string uvMode, bool flip = false) {
		if (flip) { //change the vertex order when flipping, so that normals are flipped as well
			Vector3 d = c;
			c = b;
			b = d;
		}
		Vector3 pos = vertices[vertData.verticesIndex];

		if (uvMode == "per face") {
			uvs.Add(pos == a ? new Vector2(0, 0) : pos == b ? new Vector2(0, 1) : pos == c ? new Vector2(1, 0) : new Vector2(1, 1));
		}
		else if (uvMode == "per face merge duplicates") { //hacky, legacy solution to UV mapping a line of quads with merged verts
			int id = vertData.verticesIndex;
			if (id <= 3) {
				id = 0;
			}
			else {
				id = ((int)((id - 2) / 2));
			}
			uvs.Add(pos == a ? new Vector2(id, id) : pos == b ? new Vector2(id, id + 1) : pos == c ? new Vector2(id + 1, id) : new Vector2(id + 1, id + 1));
		}
	}

	//normal modifiers
	//average the normals of verts which have the same position, to create smooth lighting
	public void averageNormals() {
		foreach (Dictionary<Quaternion, VertexData> dict in vertDict.verts.Values) { //loop over all verts in each group
			VertexData[] curVerts = dict.Values.ToArray();
			Vector3[] newNormals = new Vector3[curVerts.Length];
			for (int r = 0; r < curVerts.Length; ++r) { //loop over all other verts and average with this vert if within max normal difference
				Vector3 avgNormal = new Vector3(0, 0, 0);
				int vertsAveraged = 0;
				for (int i = 0; i < curVerts.Length; ++i) { //calculate average normal
					if (Vector3.Angle(normals[curVerts[i].verticesIndex], normals[curVerts[r].verticesIndex]) <= VertexDict.normalAverageMaxDifference + VertexDict.smoothnessFloatTolerance) {
						avgNormal += normals[curVerts[i].verticesIndex];
						vertsAveraged++;
					}
				}
				newNormals[r] = avgNormal / (float)vertsAveraged;
			}
			for (int i = 0; i < newNormals.Length; ++i) { //apply all new normals at the end, so later normal calculations are not swayed by earlier normal calculations
				normals[curVerts[i].verticesIndex] = newNormals[i];
			}

		}
	}

	//construct the new mesh, and attach the appropriate components
	public void finalizeMesh(bool useUnityNormals = false, bool smoothNormals = true, bool calculateBounds = false, bool calculateCollider = false) {
		Mesh mesh = new Mesh();
		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		if (!meshFilter) {
			meshFilter = gameObject.AddComponent<MeshFilter>();
		}
		meshFilter.mesh = mesh;
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();
		if (calculateBounds) {
			meshFilter.mesh.RecalculateBounds();
		}
		if (useUnityNormals) {
			meshFilter.mesh.RecalculateNormals();
		}
		else {
			if (smoothNormals) {
				averageNormals();
			}
			meshFilter.mesh.normals = normals.ToArray();
		}
		MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
		if (!meshRenderer) {
			meshRenderer = gameObject.AddComponent<MeshRenderer>();
		}
		Renderer renderer = meshRenderer.GetComponent<Renderer>();
		renderer.material.color = Color.blue;
		MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
		if (calculateCollider) {
			if (!meshCollider) {
				meshCollider = gameObject.AddComponent<MeshCollider>();
			}
			meshCollider.sharedMesh = mesh;
		}
		
		if (material) {
			renderer.material = material;
		}
		else {
			renderer.material.mainTexture = debugTex;
			renderer.material.color = Color.white;
		}
	}
}