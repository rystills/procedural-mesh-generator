using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshShapes : MonoBehaviour {
	GenerateMesh meshGenerator;

	public string shape;
	public List<string> args;
	public float displacementStrength;

	// Use this for initialization
	void Start() {
		float startTime = Time.realtimeSinceStartup; //record start time at beginning of function
		List<int> verts = null;
		meshGenerator = this.GetComponent<GenerateMesh>();
		if (shape == "box") {
			verts = generateBox(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
		}
		else if (shape == "spiral") {
			verts = generateSpiral(float.Parse(args[0]), float.Parse(args[1]), int.Parse(args[2]), float.Parse(args[3]));
		}
		else if (shape == "cylinder") {
			verts = generateCylinder(float.Parse(args[0]), float.Parse(args[1]), int.Parse(args[2]), args[3] == "1");
		}
		else if (shape == "sphere") {
			verts = generateSphere(float.Parse(args[0]), float.Parse(args[1]), (args.Count >= 3 ? args[2] == "1" : false));
		}
		else if (shape == "plane") {
			verts = generatePlane(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
		}
		else if (shape == "flower") {
			verts = generateFlower(int.Parse(args[0]));
		}
		if (displacementStrength > 0) {
			displaceVerts(displacementStrength, verts[0], verts[1]);
		}
		meshGenerator.finalizeMesh();

		//calculate end time and print args and time
		float endTime = Time.realtimeSinceStartup;
		string debugString = "time to generate " + shape + "(";
		for (int i = 0; i < args.Count; ++i) {
			debugString += args[i] + (i == args.Count - 1 ? ")" : ", "); 
		}
		debugString += ": " + (endTime - startTime) + " seconds";
		Debug.Log(debugString);
	}

	//apply a movement by moveBy to verts starting at startIndex and ending at endIndex (both inclusive)
	public void moveVerts(Vector3 moveBy, int startIndex, int endIndex) {
		for (int i = startIndex; i < endIndex; ++i) {
			meshGenerator.vertices[i] += moveBy;
		}
	}

	//apply a random displacement between -maxDisp and +maxDisp from vert startIndex to vert endIndex (both inclusive) - for simplicity, each vertex uses the normal of the first vert in its group
	public void displaceVerts(float maxDisp, int startIndex, int endIndex) {
		foreach (Dictionary<Quaternion, VertexData> dict in meshGenerator.vertDict.verts.Values) {
			float curDisp = Random.Range(-1 * maxDisp, maxDisp);
			VertexData[] curVerts = dict.Values.ToArray();
			for (int i = 0; i < curVerts.Length; ++i) {
				if (!(startIndex <= curVerts[i].verticesIndex && curVerts[i].verticesIndex <= endIndex)) {
					continue;
				}
				meshGenerator.vertices[curVerts[i].verticesIndex] += (meshGenerator.normals[curVerts[0].verticesIndex].normalized * curDisp);
			}
		}
	}

	//construct a plane, with lSegs and wSegs segs totaling length and width
	List<int> generatePlane(float lSegs, float wSegs, float length, float width) {
		int startVertIndex = meshGenerator.vertices.Count;
		float lIncrement = length / lSegs;
		float wIncrement = width / wSegs;
		Quaternion rot = meshGenerator.rotateQuaternion(new Quaternion(0,0,0,1),Vector3.forward,90);
		for (int i = 0; i < lSegs; ++i) {
			for (int r = 0; r < wSegs; ++r) {
				meshGenerator.propagateQuad(new Vector3(i*wIncrement, 0,r * lIncrement), rot, lIncrement, wIncrement); 

				for (int j = 0; j < 4; ++j) { //manually map vertices to a plan
					int curIndex = meshGenerator.vertices.Count - j - 1;
					meshGenerator.uvs[curIndex] = new Vector2(meshGenerator.vertices[curIndex].x, meshGenerator.vertices[curIndex].z);
				}
			}
		}
		return new List<int> { startVertIndex, meshGenerator.vertices.Count - 1 };
	}

	List<int> generateFlower(int numPetals) {
		int startVertIndex = meshGenerator.vertices.Count;
		for (int i = 0; i < 1; ++i) {
			Vector3 curPos = new Vector3(i, 0, 0);
			for (int r = 0; r < 1; ++r) {
				generateCylinder(.2f, .025f, 3, true, "centerCap", curPos);
				generateSphere(.05f, 4, true, curPos, meshGenerator.rotateQuaternion(new Quaternion(0, 0, 0, 1), Vector3.left, -90));
				curPos.y += 1;
			}
		}
		transform.rotation = meshGenerator.rotateQuaternion(transform.rotation, Vector3.left, -90);
		return new List<int> { startVertIndex, meshGenerator.vertices.Count - 1 };
	}

	//construct a sphere, with segs connecting verts which are radius distance from position
	List<int> generateSphere(float radius, float  segs, bool isHemisphere = false, Vector3? basePos = null, Quaternion? baseRot = null) {
		if (!baseRot.HasValue) {
			baseRot = new Quaternion(0, 0, 0, 1);
		}
		if (!basePos.HasValue) {
			basePos = Vector3.zero;
		}
		int startVertIndex = meshGenerator.vertices.Count;
		float increment = 360f / segs;
		Vector3 center = basePos.Value;
		List<int> centerVerts = new List<int>();
		for (int sign = 1; sign >= (isHemisphere ? 1 : -1); sign -= 2) { //iterate a hemisphere down from equator, then up from equator (if we don't break it up this way, the top pole ends up with peculiar geometry)
			for (float i = 0; i < 90; i += increment) {
				for (float j = 0; j < 360; j += increment) {
					//generate direction vectors for current point, one forward by i, one forward by j, and one forward by i and j
					Vector3 dir1 = baseRot.Value * (Quaternion.Euler(i * sign, j * sign, 0) * Vector3.forward);
					Vector3 dir2 = baseRot.Value * (Quaternion.Euler((i + increment)* sign, j * sign, 0) * Vector3.forward);
					Vector3 dir3 = baseRot.Value * (Quaternion.Euler(i * sign, (j + increment)* sign, 0) * Vector3.forward);
					Vector3 dir4 = baseRot.Value * (Quaternion.Euler((i + increment) * sign, (j + increment) * sign, 0) * Vector3.forward);

					//generate 4 position vectors from center point and direction vectors
					Vector3 pos1 = center + (dir1.normalized * radius);
					Vector3 pos2 = center + (dir2.normalized * radius);
					Vector3 pos3 = center + (dir3.normalized * radius);
					Vector3 pos4 = center + (dir4.normalized * radius);

					//call generateQuad on these points
					if (i + increment >= 90) { //we are just before the current pole; generate a tri to the pole rather than a quad
						meshGenerator.generateQuad(pos1, pos2, pos3);
					}
					else {
						meshGenerator.generateQuad(pos1, pos2, pos3, pos4);
					}
					if (i == 0 && isHemisphere) {
						//if we have so few segments that we are just generating a diamond, need to shift back the vert indices here or we will get the top vert instead
						centerVerts.Add(meshGenerator.vertices.Count - (segs <= 4 ? 3 : 2));
					}
				}
			}
		}
		//if we are generating a hemisphere rather than a whole sphere, cap the exposed side
		if (isHemisphere) {
			generateCapCenter(centerVerts);
		}
		
		return new List<int> { startVertIndex, meshGenerator.vertices.Count - 1 };
	}

	//construct a spiral, with segs quads of width, extents, rotating each quad by iterAngle
	List<int> generateSpiral(float width, float extents, int segs, float iterAngle) {
		int startVertIndex = meshGenerator.vertices.Count;
		Vector3 rotAxis = Vector3.forward;
		Quaternion rot = new Quaternion(0, 0, 0, 1);
		Vector3 pos = new Vector3(0, 0, 0);
		float curExtents = extents;
		for (int i = 0; i < segs; ++i, curExtents -= extents / (float)segs) { //decrease segment length after each iteration
			meshGenerator.propagateQuad(pos, rot, width, curExtents, true); //generate back-facing quad (flipped normal)
			pos = meshGenerator.propagateQuad(pos, rot, width, curExtents, false); //generate forward-facing quad and update current vertex position
			rot = meshGenerator.rotateQuaternion(rot, rotAxis, iterAngle); //update rotation
		}
		if (segs == 0 || startVertIndex == meshGenerator.vertices.Count) { //if we didnt make any new verts, return an empty list
			return null;
		}
		return new List<int> { startVertIndex, meshGenerator.vertices.Count - 1 };

	}

	//construct a segs-sided cylinder with width and extents
	List<int> generateCylinder(float width, float extents, int segs, bool cap = false, string startType = "edge", Vector3? basePos = null, Quaternion? baseRot = null) {
		if (!baseRot.HasValue) {
			baseRot = new Quaternion(0, 0, 0, 1);
		}
		if (!basePos.HasValue) {
			basePos = Vector3.zero;
		}
		int startVertIndex = meshGenerator.vertices.Count;
		Vector3 rotAxis = Vector3.forward;
		Quaternion rot = baseRot.Value;
		Vector3 pos = basePos.Value;
		float iterAngle = 360 / (float)segs;
		float iterExtents = extents / (float)segs;
		List<int> frontVerts = new List<int>();
		List<int> backVerts = new List<int>();
		for (int i = 0; i < segs; ++i) {
			
			if (!cap) { //don't bother generating interior faces if we are capped, since they will not be visible anyway
				meshGenerator.propagateQuad(pos, rot, width, iterExtents, false); //generate forward-facing quad and update current vertex position
			}
			pos = meshGenerator.propagateQuad(pos, rot, width, iterExtents, true); //generate back-facing quad (flipped normal)
			rot = meshGenerator.rotateQuaternion(rot, rotAxis, iterAngle); //update rotation
			if (cap) {
				frontVerts.Add(meshGenerator.vertices.Count - 2);
				//frontVerts.Add(meshGenerator.vertices.Count - 4);
				backVerts.Add(meshGenerator.vertices.Count - 3);
				//backVerts.Add(meshGenerator.vertices.Count - 1);
			}
		}
		if (segs == 0 || startVertIndex == meshGenerator.vertices.Count) { //if we didnt make any new verts, return an empty list
			return null;
		}

		if (cap) { //cap front and back of cylinder
			generateCapCenter(frontVerts,false);
			generateCapCenter(backVerts,true);			
		}

		if (startType == "centerCap") {
			moveVerts(pos - calculateCenter(backVerts), startVertIndex, meshGenerator.vertices.Count - 1);
		}

		return new List<int> { startVertIndex, meshGenerator.vertices.Count - 1 };
	}

	//calculate the center position of a list of verts
	Vector3 calculateCenter(List<int> verts) {
		Vector3 center = Vector3.zero;
		for (int i = 0; i < verts.Count; ++i) {
			center += meshGenerator.vertices[verts[i]];
		}
		return center / (float)verts.Count;
	}

	//generate cap-faces between verts, using a new center vert to conncet all of the faces
	public Vector3 generateCapCenter(List<int> verts, bool flip = false) {
		//calculate center position
		Vector3 center = calculateCenter(verts);
		//add center as new vert, with two other reference verts to get the normal right
		for (int i = 0; i < verts.Count - 1; ++i) {
			meshGenerator.generateQuad(meshGenerator.vertices[verts[i + (flip ? 1 : 0)]], meshGenerator.vertices[verts[i + (flip ? 0 : 1)]], center);
		}
		//generate final face between last vert, first vert, and center
		meshGenerator.generateQuad(meshGenerator.vertices[verts[flip ? 0 : verts.Count - 1]], meshGenerator.vertices[verts[flip ? verts.Count - 1 : 0]], center);
		return center;
	}

	//generate cap-faces between verts, using the first vert to connect all of the faces
	public void generateCapEdge(List<int> verts, bool flip = false) {
		for (int i = 1; i < verts.Count - 1; ++i) {
			meshGenerator.generateQuad(meshGenerator.vertices[verts[0]], meshGenerator.vertices[verts[i + (flip ? 1 : 0)]], meshGenerator.vertices[verts[i + (flip ? 0 : 1)]]);
		}
	}

	//construct a box, with length, width, height segs
	List<int> generateBox(float length, float width, float height) {
		int startVertIndex = meshGenerator.vertices.Count;
		Vector3 rotAxis = Vector3.forward;
		Quaternion rot = new Quaternion(0, 0, 0, 1);
		Vector3 pos = new Vector3(0, 0, 0);
		for (int i = 0; i < 4; ++i) { //generate a strip of 4 sides
			pos = meshGenerator.propagateQuad(pos, rot, 1, 1, true); //generate forward-facing quad and update current vertex position
			rot = meshGenerator.rotateQuaternion(rot, rotAxis, 90); //update rotation
		}
		Quaternion leftRot = meshGenerator.rotateQuaternion(rot, Vector3.up, 90);
		meshGenerator.propagateQuad(pos, leftRot, 1, 1, false); //generate 'left' sidee
		meshGenerator.propagateQuad(pos + Vector3.forward.normalized, leftRot, 1, 1, true); //generate 'right' side
		return new List<int> { startVertIndex, meshGenerator.vertices.Count - 1 };
	}
}