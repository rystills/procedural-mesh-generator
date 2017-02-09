using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GenerateMesh : MonoBehaviour {
    public Material material;
    List<int> triangles;
	VertexDict vertDict;
	List<Vector3> vertices;
	List<Vector2> uvs;
	List<Vector3> normals;
    Texture2D debugTex;

    void Start() {
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
        //List<int> verts = generateBox(2, 3, 4);
        List<int> verts = generateSpiral(2, 1, 100, 16);
        displaceVerts(.2f, verts[0], verts[1]);
        finalizeMesh();
    }

	//utility functions
	//rotate quaternion quat on axis rotAxis by amount 
	Quaternion rotateQuaternion(Quaternion quat, Vector3 rotAxis, float amount) {
		return quat * Quaternion.Euler(rotAxis * amount);
	}

	//return Quaternion quat rotated by 180 degrees to face in the opposite direction (useful for flipping normals)
	Quaternion flipQuaternion(Quaternion quat) {
		return (new Quaternion(1, 0, 0, 0)) * quat;
	}

	//find and return the rotation of the vertex which corresponds to vertIndex
	VertexData getVertData(int vertIndex) {
		VertexData vert = vertDict.getVert(vertices[vertIndex],normals[vertIndex]);
		if (vert != null) {
			return vert;
		}
		throw new System.Exception("error: vertex #" + vertIndex + " not found in vertDict");
	}

	//return the normal vector between vectors a,b,c
	Vector3 calculateNormal(Vector3 a, Vector3 b, Vector3 c) {
		Vector3 side1 = b - a;
		Vector3 side2 = c - a;
		Vector3 perp = Vector3.Cross(side1, side2);
		return perp / perp.magnitude;
	}

	//mesh construction blueprints
	//construct a spiral, with segs quads of width, extents, rotating each quad by iterAngle
	List<int> generateSpiral(float width, float extents, int segs, float iterAngle) {
		int startVertIndex = vertices.Count;
		Vector3 rotAxis = Vector3.forward;
		Quaternion rot = new Quaternion(0, 0, 0, 1);
		Vector3 pos = new Vector3(0, 0, 0);
		float curExtents = extents;
		for (int i = 0; i < segs; ++i, curExtents -= extents / segs) { //decrease segment length after each iteration
			propagateQuad(pos, rot, width, curExtents, true); //generate back-facing quad (flipped normal)
			pos = propagateQuad(pos, rot, width, curExtents, false); //generate forward-facing quad and update current vertex position
			rot = rotateQuaternion(rot, rotAxis, iterAngle); //update rotation
		}
		if (segs == 0 || startVertIndex == vertices.Count) { //if we didnt make any new verts, return an empty list
			return null;
		}
		return new List<int> { startVertIndex, vertices.Count - 1 };

	}

    //construct a box, with length, width, height segs
    List<int> generateBox(float length, float width, float height) {
        int startVertIndex = vertices.Count;
        Vector3 rotAxis = Vector3.forward;
        Quaternion rot = new Quaternion(0, 0, 0, 1);
        Vector3 pos = new Vector3(0, 0, 0);
        for (int i = 0; i < 4; ++i) { //generate a strip of 4 sides
            pos = propagateQuad(pos, rot, 1, 1, true); //generate forward-facing quad and update current vertex position
            rot = rotateQuaternion(rot, rotAxis, 90); //update rotation
        }
        Quaternion leftRot = rotateQuaternion(rot, Vector3.up, 90);
        propagateQuad(pos, leftRot, 1,1,false); //generate 'left' sidee
        propagateQuad(pos + Vector3.forward.normalized, leftRot, 1,1,true); //generate 'right' side
        return new List<int> { startVertIndex, vertices.Count - 1 };
    }

	//apply a random displacement between -maxDisp and +maxDisp from vert startIndex to vert endIndex (both inclusive) - for simplicity, each vertex uses the normal of the first vert in its group
	void displaceVerts(float maxDisp, int startIndex, int endIndex) {
		foreach (Dictionary<Quaternion,VertexData> dict in vertDict.verts.Values) {
			float curDisp = Random.Range(-1 * maxDisp, maxDisp);
			VertexData[] curVerts = dict.Values.ToArray();
			for (int i = 0; i < curVerts.Length; ++i) {
				if (!(startIndex <= curVerts[i].verticesIndex && curVerts[i].verticesIndex <= endIndex)) {
					continue;
				}
				vertices[curVerts[i].verticesIndex] += (normals[curVerts[0].verticesIndex].normalized * curDisp);
			}
		}
	}

	//core generators  
	//create an additional quad from position[] of size quadsize in direction dir (returns ending position)
	Vector3 propagateQuad(Vector3 pos, Quaternion dir, float width, float extents, bool flip = false, float vertSmoothnessthreshold = 0, string uvMode = "per face") {
        //step 1: generate the necessary verts, and corresponding UVs
        //calculate forward and left vectors from rotation Quaternion 
        Vector3 forwardDir = dir * Vector3.forward;
        Quaternion leftRotation = rotateQuaternion(dir, new Vector3(1, 0, 0), 90);
        Vector3 leftDir = leftRotation * Vector3.forward;

		//calculate 3 remaining positions from forward and left vectors
		Vector3 topRightPos = pos + (forwardDir.normalized * width);
		Vector3 botLeftPos = pos + (leftDir.normalized * extents);
        Vector3 topLeftPos = botLeftPos + (forwardDir.normalized * width);

		//calculate normal dir
		Vector3 normal;
		if (flip) {
			normal = calculateNormal(pos, botLeftPos, topRightPos);
		}
		else {
			normal = calculateNormal(pos, topRightPos, botLeftPos);
		}
		
		//generate botRight vert
		VertexData botRightVert = vertDict.getVert(pos, normal);
		if (botRightVert == null) {
			botRightVert = addVert(pos, normal, vertSmoothnessthreshold);
			addUV(botRightVert, pos, topRightPos, botLeftPos, uvMode, flip);
		}
		//generate topRight vert
		VertexData topRightVert = vertDict.getVert(topRightPos, normal);
		if (topRightVert == null) {
			topRightVert = addVert(topRightPos, normal, vertSmoothnessthreshold);
			addUV(topRightVert, pos, topRightPos, botLeftPos, uvMode, flip);
		}
		//generate botLeft vert
		VertexData botLeftVert = vertDict.getVert(botLeftPos, normal);
		if (botLeftVert == null) {
			botLeftVert = addVert(botLeftPos, normal, vertSmoothnessthreshold);
			addUV(botLeftVert, pos, topRightPos, botLeftPos, uvMode, flip);
		}
		//generate topLeft vert
		VertexData topLeftVert = vertDict.getVert(topLeftPos, normal);
		if (topLeftVert == null) {
			topLeftVert= addVert(topLeftPos, normal, vertSmoothnessthreshold);
			addUV(topLeftVert, pos, topRightPos, botLeftPos, uvMode, flip);
		}

        //step 2: generate the necessary tris (because this method adds a single quad, we need two new triangles, or 6 points in our list of tris)
		addQuad(botRightVert.verticesIndex, topRightVert.verticesIndex, botLeftVert.verticesIndex, topLeftVert.verticesIndex, dir, flip);
        return botLeftPos;
    }

	//tri modifiers
	//simple helper method to add two tris with the same normal, forming a single quad (recommended in most cases)
	void addQuad(int botRightIndex, int topRightIndex, int botLeftIndex, int topLeftIndex, Quaternion dir, bool flip) {
		//first new tri
		addTri(botRightIndex, topRightIndex, botLeftIndex, dir, flip);
		//second new tri
		addTri(topRightIndex, topLeftIndex, botLeftIndex, dir, flip);
	}
	//simple helper method to add 3 points to the triangles list
	void addTri(int index1, int index2, int index3, Quaternion dir, bool flip = false) {
		triangles.Add(flip ? index3 : index1);
		triangles.Add(index2);
		triangles.Add(flip ? index1 : index3);
	}

	//vert modifiers
	//add a new vert with corresponding UVs if xPos,yPos does not already contain one, and add this vert's position in vertices to vertIndicesAxes
	VertexData addVert(Vector3 pos, Vector3 normal, float vertSmoothnessthreshold) {
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
	void addUV(VertexData vertData, Vector3 a, Vector3 b, Vector3 c, string uvMode, bool flip = false) {
		if (flip) { //change the vertex order when flipping, so that normals are flipped as well
			Vector3 d = c;
			c = b;
			b = d;
		}
		Vector3 pos = vertices[vertData.verticesIndex];

        if (uvMode == "per face") {
            uvs.Add(pos == a ? new Vector2(0, 0) : pos == b ? new Vector2(0, 1) : pos == c ? new Vector2(1, 0) : new Vector2(1, 1));
        }
        else if (uvMode == "per face merge duplicates") {
			int id = vertData.verticesIndex;
            if (id <= 3) {
                id = 0;
            }
            else {
                id = ((int)((id - 2) / 2));
            }
            uvs.Add(pos == a ? new Vector2(id, id) : pos == b ? new Vector2(id, id+1) : pos == c ? new Vector2(id+1, id) : new Vector2(id+1, id+1));
        }
    }

	//normal modifiers
    //average the normals of verts which have the same position, to create smooth lighting
    void averageNormals() {
		foreach (Dictionary<Quaternion, VertexData> dict in vertDict.verts.Values) { //loop over all verts in each group
			VertexData[] curVerts = dict.Values.ToArray();
			Vector3[] newNormals = new Vector3[curVerts.Length];
			int[] newVertsAveraged = new int[curVerts.Length];
			for (int r = 0; r < curVerts.Length; ++r) { //loop over all other verts and average with this vert if within max normal difference
				Vector3 avgNormal = new Vector3(0, 0, 0);
				int vertsAveraged = 0;
				for (int i = 0; i < curVerts.Length; ++i) { //calculate average normal
					if (Vector3.Angle(normals[curVerts[i].verticesIndex], normals[curVerts[r].verticesIndex]) <= VertexDict.normalAverageMaxDifference + VertexDict.smoothnessFloatTolerance) {
						avgNormal += normals[curVerts[i].verticesIndex];
						vertsAveraged++;
					}
				}
				newNormals[r] = avgNormal / vertsAveraged;
				newVertsAveraged[r] = vertsAveraged;
			}
			for (int i = 0; i < newNormals.Length; ++i) { //apply all new normals at the end, so later normal calculations are not swayed by earlier normal calculations
				normals[curVerts[i].verticesIndex] = newNormals[i] / newVertsAveraged[i];
			}
			
		}
    }

    //construct the new mesh, and attach the appropriate components
    void finalizeMesh(bool useUnityNormals = false) {
        Mesh mesh = new Mesh();
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (!meshFilter) {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = mesh;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        meshFilter.mesh.RecalculateBounds();
        if (useUnityNormals) {
            meshFilter.mesh.RecalculateNormals();
        }
        else {
            averageNormals();
            meshFilter.mesh.normals = normals.ToArray();
        }
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

    void Update() {
		//wave vert groups by the first vert's normal for the sake of simplicity
		int count = 0;
		foreach (Dictionary<Quaternion, VertexData> dict in vertDict.verts.Values) {
			VertexData[] curVerts = dict.Values.ToArray();
			for (int i = 0; i < curVerts.Length; ++i) {
				vertices[curVerts[i].verticesIndex] = vertices[curVerts[i].verticesIndex] + normals[curVerts[0].verticesIndex].normalized * (Mathf.Sin(2 * Time.time + count) / 400);
			}
			++count;
		}
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		mesh.vertices = vertices.ToArray();
    }
}